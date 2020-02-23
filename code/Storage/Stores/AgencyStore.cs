// <copyright file="AgencyStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage.Table;
    using Microsoft.WindowsAzure.Storage.Table.Queryable;
    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Stores and retrieves agency records from Azure Table storage.
    /// </summary>
    public class AgencyStore : Store
    {
        /// <summary>
        /// Initializes static members of the <see cref="AgencyStore"/> class.
        /// </summary>
        static AgencyStore()
        {
            // Map partition key, row key for this type
            AbstractEntityAdapter.RegisterType<AgencyEntity>(
                (agency) => agency.PartitionKey,
                (agency) => agency.RowKey,
                (agency, pk) => { },
                (agency, rk) => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyStore"/> class.
        /// </summary>
        /// <param name="cloudTableClient">client to Azure Table</param>
        /// <param name="tableType">type of table</param>
        /// <param name="runId">uniquely identifies an execution of the OBA service</param>
        public AgencyStore(CloudTableClient cloudTableClient, TableNames.TableType tableType, string runId)
            : base(cloudTableClient, TableNames.TableName(tableType, runId))
        {
        }

        /// <summary>
        /// Stores a list of agencies in Azure Tables
        /// </summary>
        /// <param name="agencies">list of agencies</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<AgencyEntity> agencies)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();
            if (agencies.Any())
            {
                foreach (var agency in agencies)
                {
                    batchOperation.Add(TableOperation.Insert(new EntityAdapter<AgencyEntity>(agency)));
                }

                await this.Table.ExecuteBatchInChunkAsync(batchOperation);
            }
        }

        /// <summary>
        /// Stores a list of OBA client agencies in Azure Tables
        /// </summary>
        /// <param name="agencies">list of agencies</param>
        /// <param name="regionId">uniquely identifies the region</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<OBAClient.Model.Agency> agencies, string regionId)
        {
            // convert the input agencies into agency entities
            List<AgencyEntity> agencyEntities = new List<AgencyEntity>();
            foreach (OBAClient.Model.Agency agency in agencies)
            {
                AgencyEntity agencyEntity = new AgencyEntity()
                {
                    Id = agency.Id,
                    Name = agency.Name,
                    Url = agency.Url,
                    Phone = agency.Phone,
                    RegionId = regionId,
                    RecordType = RecordType.Agency.ToString(),
                    RowState = DataRowState.Default.ToString(),
                    RawContent = agency.RawContent
                };

                agencyEntities.Add(agencyEntity);
            }

            await this.Insert(agencyEntities);
        }

        /// <summary>
        /// Get all agencies for a specified region Id
        /// </summary>
        /// <param name="regionId">region Id</param>
        /// <returns>list of agency entities</returns>
        public async Task<IEnumerable<AgencyEntity>> GetAllAgencies(string regionId)
        {
            var downloadedAgencies = new List<AgencyEntity>();

            var retrievedAgencies = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                     where entry.PartitionKey == regionId
                                     && entry.Properties["RecordType"].StringValue == RecordType.Agency.ToString()
                                     select entry).Resolve(AbstractEntityAdapter.AdapterResolver<AgencyEntity>);

            TableContinuationToken continuationToken = null;
            do
            {
                // Execute the query until there are no more results
                var queryResult = await retrievedAgencies.ExecuteSegmentedAsync(continuationToken);
                foreach (var downloadedAgency in queryResult)
                {
                    downloadedAgencies.Add(downloadedAgency);
                }

                continuationToken = queryResult.ContinuationToken;
            }
            while (continuationToken != null);

            return downloadedAgencies;
        }

        /// <summary>
        /// Get a particular agency from table storage
        /// </summary>
        /// <param name="regionId">region Id</param>
        /// <param name="agencyId">agency Id</param>
        /// <returns>agency entity</returns>
        public AgencyEntity Get(string regionId, string agencyId)
        {
            AgencyEntity partialAgency = new AgencyEntity { Id = agencyId, RegionId = regionId, RecordType = Enum.GetName(typeof(RecordType), RecordType.Agency) };

            var retrievedAgencies = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                     where entry.PartitionKey == partialAgency.PartitionKey
                                     && entry.RowKey == partialAgency.RowKey
                                     && entry.Properties["RecordType"].StringValue == RecordType.Agency.ToString()
                                     select entry).Resolve(AbstractEntityAdapter.AdapterResolver<AgencyEntity>).ToList();

            // return null if no records were retrieved
            if (retrievedAgencies == null || retrievedAgencies.Count == 0)
            {
                return null;
            }

            // throw an exception if more than 1 record was received
            if (retrievedAgencies.Count > 1)
            {
                throw new Exception("Expected 1 record but retrieved " + retrievedAgencies.Count + " records.");
            }

            return retrievedAgencies[0];
        }
    }
}
