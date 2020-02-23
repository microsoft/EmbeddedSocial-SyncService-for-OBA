// <copyright file="DownloadMetadataStore.cs" company="Microsoft">
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
    using OBAService.Utils;

    /// <summary>
    /// Stores and retrieves metadata records for OBA downloads to/from Azure Table storage.
    /// </summary>
    public class DownloadMetadataStore : Store
    {
        /// <summary>
        /// Initializes static members of the <see cref="DownloadMetadataStore"/> class.
        /// </summary>
        static DownloadMetadataStore()
        {
            // Map partition key, row key for this type
            AbstractEntityAdapter.RegisterType<DownloadMetadataEntity>(
                (downloadMetadata) => downloadMetadata.PartitionKey,
                (downloadMetadata) => downloadMetadata.RowKey,
                (downloadMetadata, pk) => { },
                (downloadMetadata, rk) => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadMetadataStore"/> class.
        /// </summary>
        /// <param name="cloudTableClient">client to Azure Table</param>
        /// <param name="tableType">type of table</param>
        /// <param name="runId">uniquely identifies an execution of the OBA service</param>
        public DownloadMetadataStore(CloudTableClient cloudTableClient, TableNames.TableType tableType, string runId)
            : base(cloudTableClient, TableNames.TableName(tableType, runId))
        {
        }

        /// <summary>
        /// Stores a list of download metadata entities in Azure Tables
        /// </summary>
        /// <param name="records">list of download metadata entities</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<DownloadMetadataEntity> records)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();
            if (records.Any())
            {
                foreach (var downloadMetadata in records)
                {
                    batchOperation.Add(TableOperation.Insert(new EntityAdapter<DownloadMetadataEntity>(downloadMetadata)));
                }

                await this.Table.ExecuteBatchInChunkAsync(batchOperation);
            }
        }

        /// <summary>
        /// Create and store a new metadata record
        /// </summary>
        /// <param name="runId">uniquely identifies a run of the OBA service</param>
        /// <param name="regionId">uniquely identifies a region</param>
        /// <param name="agencyId">uniquely identifies an agency</param>
        /// <param name="recordType">type of record to store</param>
        /// <param name="count">number of records stored</param>
        /// <returns>a task that inserts a new metadata record</returns>
        public async Task Insert(string runId, string regionId, string agencyId, RecordType recordType, int count)
        {
            DownloadMetadataEntity entity = new DownloadMetadataEntity()
            {
                RunId = runId,
                RegionId = regionId,
                AgencyId = agencyId,
                RecordType = recordType.ToString(),
                Count = count
            };

            List<DownloadMetadataEntity> entities = new List<DownloadMetadataEntity>();
            entities.Add(entity);
            await this.Insert(entities);
        }

        /// <summary>
        /// Get a list of download metadata entities from Azure Tables
        /// </summary>
        /// <param name="runId">which run Id to get the entities for</param>
        /// <returns>list of download metadata entities</returns>
        public IEnumerable<DownloadMetadataEntity> Get(string runId)
        {
            var retrievedDownloadMetadataEntities = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                                     where entry.PartitionKey == runId
                                                     select entry).Resolve(AbstractEntityAdapter.AdapterResolver<DownloadMetadataEntity>).ToList();

            // return null if no records were retrieved
            if (retrievedDownloadMetadataEntities == null || retrievedDownloadMetadataEntities.Count == 0)
            {
                return null;
            }

            return retrievedDownloadMetadataEntities;
        }

        /// <summary>
        /// Deletes all download metadata entities from Azure Tables for a runId
        /// </summary>
        /// <param name="runId">which run Id to delete the entities for</param>
        /// <returns>task that deletes entities</returns>
        public async Task Delete(string runId)
        {
            var retrievedDownloadMetadataEntities = from entry in this.Table.CreateQuery<TableEntity>()
                                                    where entry.PartitionKey == runId
                                                    select entry;

            TableBatchOperation batch = new TableBatchOperation();
            foreach (TableEntity entity in retrievedDownloadMetadataEntities)
            {
                batch.Add(TableOperation.Delete(entity));
            }

            await this.Table.ExecuteBatchInChunkAsync(batch);
        }
    }
}
