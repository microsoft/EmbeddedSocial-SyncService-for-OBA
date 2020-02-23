// <copyright file="DiffMetadataStore.cs" company="Microsoft">
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
    /// Stores and retrieves metadata records for diffs to/from Azure Table storage.
    /// </summary>
    public class DiffMetadataStore : Store
    {
        /// <summary>
        /// Initializes static members of the <see cref="DiffMetadataStore"/> class.
        /// </summary>
        static DiffMetadataStore()
        {
            // Map partition key, row key for this type
            AbstractEntityAdapter.RegisterType<DiffMetadataEntity>(
                (diffMetadata) => diffMetadata.PartitionKey,
                (diffMetadata) => diffMetadata.RowKey,
                (diffMetadata, pk) => { },
                (diffMetadata, rk) => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffMetadataStore"/> class.
        /// </summary>
        /// <param name="cloudTableClient">client to Azure Table</param>
        /// <param name="tableType">type of table</param>
        /// <param name="runId">uniquely identifies an execution of the OBA service</param>
        public DiffMetadataStore(CloudTableClient cloudTableClient, TableNames.TableType tableType, string runId)
            : base(cloudTableClient, TableNames.TableName(tableType, runId))
        {
        }

        /// <summary>
        /// Stores a list of diff metadata entities in Azure Tables
        /// </summary>
        /// <param name="records">list of diff metadata entities</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<DiffMetadataEntity> records)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();
            if (records.Any())
            {
                foreach (var diffMetadata in records)
                {
                    batchOperation.Add(TableOperation.Insert(new EntityAdapter<DiffMetadataEntity>(diffMetadata)));
                }

                await this.Table.ExecuteBatchInChunkAsync(batchOperation);
            }
        }

        /// <summary>
        /// Get a list of diff metadata entities from Azure Tables
        /// </summary>
        /// <param name="runId">which run Id to get the entities for</param>
        /// <returns>list of diff metadata entities</returns>
        public IEnumerable<DiffMetadataEntity> Get(string runId)
        {
            var retrievedDiffMetadataEntities = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                                 where entry.PartitionKey == runId
                                                 select entry).Resolve(AbstractEntityAdapter.AdapterResolver<DiffMetadataEntity>).ToList();

            // return null if no records were retrieved
            if (retrievedDiffMetadataEntities == null || retrievedDiffMetadataEntities.Count == 0)
            {
                return null;
            }

            return retrievedDiffMetadataEntities;
        }

        /// <summary>
        /// Deletes all diff metadata entities from Azure Tables for a runId
        /// </summary>
        /// <param name="runId">which run Id to delete the entities for</param>
        /// <returns>task that deletes entities</returns>
        public async Task Delete(string runId)
        {
            var retrievedDiffMetadataEntities = from entry in this.Table.CreateQuery<TableEntity>()
                                                where entry.PartitionKey == runId
                                                select entry;

            TableBatchOperation batch = new TableBatchOperation();
            foreach (TableEntity entity in retrievedDiffMetadataEntities)
            {
                batch.Add(TableOperation.Delete(entity));
            }

            await this.Table.ExecuteBatchInChunkAsync(batch);
        }
    }
}
