// <copyright file="RegionsListStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage.Table;
    using Microsoft.WindowsAzure.Storage.Table.Queryable;
    using OBAService.OBAClient.Model;
    using OBAService.Storage.Model;

    /// <summary>
    /// Stores and retrieves OBA Regions List entities from Azure Table storage.
    /// </summary>
    public class RegionsListStore : Store
    {
        /// <summary>
        /// Initializes static members of the <see cref="RegionsListStore"/> class.
        /// </summary>
        static RegionsListStore()
        {
            // map partition key and row key for this type
            AbstractEntityAdapter.RegisterType<RegionsListEntity>(
                (regionsList) => regionsList.PartitionKey,
                (regionsList) => regionsList.RowKey,
                (regionsList, pk) => { },
                (regionsList, rk) => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionsListStore"/> class.
        /// </summary>
        /// <param name="cloudTableClient">client to Azure Table</param>
        /// <param name="tableType">type of table</param>
        /// <param name="runId">uniquely identifies an execution of the OBA service</param>
        public RegionsListStore(CloudTableClient cloudTableClient, TableNames.TableType tableType, string runId)
            : base(cloudTableClient, TableNames.TableName(tableType, runId))
        {
        }

        /// <summary>
        /// Stores a regions list entity in Azure Tables
        /// </summary>
        /// <param name="regionsList">regions list entity</param>
        /// <returns>task that stores the entity</returns>
        public async Task Insert(RegionsListEntity regionsList)
        {
            await this.Table.ExecuteAsync(TableOperation.Insert(new EntityAdapter<RegionsListEntity>(regionsList)));
        }

        /// <summary>
        /// Stores a OBA Source model in Azure Tables
        /// </summary>
        /// <param name="regionsList">OBA Source model from the OBA client</param>
        /// <returns>task that stores the entity</returns>
        public async Task Insert(RegionsList regionsList)
        {
            // create a new storage entity
            RegionsListEntity regionsListEntity = new RegionsListEntity
            {
                RawContent = regionsList.RegionsRawContent,
                RecordType = RecordType.RegionsList.ToString(),
                RegionsServiceUri = regionsList.Uri
            };

            // insert it
            await this.Insert(regionsListEntity);
        }

        /// <summary>
        /// Get a regions list entity from Azure Tables
        /// </summary>
        /// <returns>regions list entity</returns>
        public RegionsListEntity Get()
        {
            // there should be exactly 1 regions list record in any table
            var retrievedRegionsLists = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                         where entry.Properties["RecordType"].StringValue == RecordType.RegionsList.ToString()
                                         select entry).Resolve(AbstractEntityAdapter.AdapterResolver<RegionsListEntity>).ToList();

            // return null if no records were retrieved
            if (retrievedRegionsLists == null || retrievedRegionsLists.Count == 0)
            {
                return null;
            }

            // throw an exception if more than 1 record was received
            if (retrievedRegionsLists.Count > 1)
            {
                throw new Exception("Expected 1 record but retrieved " + retrievedRegionsLists.Count + " records.");
            }

            return retrievedRegionsLists[0];
        }
    }
}
