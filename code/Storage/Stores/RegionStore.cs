// <copyright file="RegionStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage.Table;
    using Microsoft.WindowsAzure.Storage.Table.Queryable;
    using OBAService.Storage.Model;

    /// <summary>
    /// Stores and retrieves region records from Azure Table storage.
    /// </summary>
    public class RegionStore : Store
    {
        /// <summary>
        /// Initializes static members of the <see cref="RegionStore"/> class.
        /// </summary>
        static RegionStore()
        {
            // Map partition key and row key for this type
            AbstractEntityAdapter.RegisterType<RegionEntity>(
                (region) => region.PartitionKey,
                (region) => region.RowKey,
                (region, pk) => { },
                (region, rk) => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionStore"/> class.
        /// </summary>
        /// <param name="cloudTableClient">client to Azure Table</param>
        /// <param name="tableType">type of table</param>
        /// <param name="runId">uniquely identifies an execution of the OBA service</param>
        public RegionStore(CloudTableClient cloudTableClient, TableNames.TableType tableType, string runId)
            : base(cloudTableClient, TableNames.TableName(tableType, runId))
        {
        }

        /// <summary>
        /// Get all region Ids from the region table
        /// </summary>
        /// <returns>List of region Ids</returns>
        public IEnumerable<string> GetAllRegionIds()
        {
            var retrievedRegions = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                    where entry.Properties["RecordType"].StringValue == RecordType.Region.ToString()
                                    select entry).Resolve(AbstractEntityAdapter.AdapterResolver<RegionEntity>);

            foreach (var region in retrievedRegions)
            {
                yield return region.Id;
            }
        }

        /// <summary>
        /// Get all regions from the region table
        /// </summary>
        /// <returns>List of region entities</returns>
        public IEnumerable<RegionEntity> GetAllRegions()
        {
            var retrievedRegions = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                    where entry.Properties["RecordType"].StringValue == RecordType.Region.ToString()
                                    select entry).Resolve(AbstractEntityAdapter.AdapterResolver<RegionEntity>);
            return retrievedRegions.ToList();
        }

        /// <summary>
        /// Stores a list of regions in Azure Tables
        /// </summary>
        /// <param name="regions">list of regions</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<RegionEntity> regions)
        {
            List<Task> tasks = new List<Task>();
            if (regions.Any())
            {
                // Each region has a different partition key. So cannot do a batch insert.
                foreach (var region in regions)
                {
                    tasks.Add(this.Table.ExecuteAsync(TableOperation.Insert(new EntityAdapter<RegionEntity>(region))));
                }

                await Task.WhenAll(tasks.ToArray());
            }
        }

        /// <summary>
        /// Stores a list of OBA client regions in Azure Tables
        /// </summary>
        /// <param name="regions">list of regions</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<OBAClient.Model.Region> regions)
        {
            // convert the input regions into region entities
            List<RegionEntity> regionEntities = new List<RegionEntity>();
            foreach (OBAClient.Model.Region region in regions)
            {
                RegionEntity regionEntity = new RegionEntity
                {
                    Id = region.Id,
                    RegionName = region.RegionName,
                    ObaBaseUrl = region.ObaBaseUrl,
                    RecordType = RecordType.Region.ToString(),
                    RowState = DataRowState.Default.ToString(),
                    RawContent = region.RawContent
                };

                regionEntities.Add(regionEntity);
            }

            // issue the insert call
            await this.Insert(regionEntities);
        }
    }
}
