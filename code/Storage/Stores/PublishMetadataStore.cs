// <copyright file="PublishMetadataStore.cs" company="Microsoft">
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
    /// Stores and retrieves metadata records for publish activity.
    /// </summary>
    public class PublishMetadataStore : Store
    {
        /// <summary>
        /// Initializes static members of the <see cref="PublishMetadataStore"/> class.
        /// </summary>
        static PublishMetadataStore()
        {
            // Map partition key, row key for this type
            AbstractEntityAdapter.RegisterType<PublishMetadataEntity>(
                (publishMetadata) => publishMetadata.PartitionKey,
                (publishMetadata) => publishMetadata.RowKey,
                (publishMetadata, pk) => { },
                (publishMetadata, rk) => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishMetadataStore"/> class.
        /// </summary>
        /// <param name="cloudTableClient">client to Azure Table</param>
        /// <param name="tableType">type of table</param>
        /// <param name="runId">uniquely identifies an execution of the OBA service</param>
        public PublishMetadataStore(CloudTableClient cloudTableClient, TableNames.TableType tableType, string runId)
            : base(cloudTableClient, TableNames.TableName(tableType, runId))
        {
        }

        /// <summary>
        /// Stores a list of publish metadata entities in Azure Tables
        /// </summary>
        /// <param name="records">list of publish metadata entities</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<PublishMetadataEntity> records)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();
            if (records.Any())
            {
                foreach (var publishMetadata in records)
                {
                    batchOperation.Add(TableOperation.Insert(new EntityAdapter<PublishMetadataEntity>(publishMetadata)));
                }

                await this.Table.ExecuteBatchInChunkAsync(batchOperation);
            }
        }

        /// <summary>
        /// Stores a publish metadata entity in Azure Tables
        /// </summary>
        /// <param name="record">publish metadata entity</param>
        /// <returns>task that stores the entity</returns>
        public async Task Insert(PublishMetadataEntity record)
        {
            await this.Insert(new List<PublishMetadataEntity>() { record });
        }

        /// <summary>
        /// Get a list of publish metadata entities from Azure Tables
        /// </summary>
        /// <param name="runId">which run Id to get the entities for</param>
        /// <returns>list of publish metadata entities</returns>
        public IEnumerable<PublishMetadataEntity> Get(string runId)
        {
            var retrievedPublishMetadataEntities = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                                 where entry.PartitionKey == runId
                                                 select entry).Resolve(AbstractEntityAdapter.AdapterResolver<PublishMetadataEntity>).ToList();

            // return null if no records were retrieved
            if (retrievedPublishMetadataEntities == null || retrievedPublishMetadataEntities.Count == 0)
            {
                return null;
            }

            return retrievedPublishMetadataEntities;
        }

        /// <summary>
        /// Deletes all publish metadata entities from Azure Tables for a runId
        /// </summary>
        /// <param name="runId">which run Id to delete the entities for</param>
        /// <returns>task that deletes entities</returns>
        public async Task Delete(string runId)
        {
            var retrievedPublishMetadataEntities = from entry in this.Table.CreateQuery<TableEntity>()
                                                where entry.PartitionKey == runId
                                                select entry;

            TableBatchOperation batch = new TableBatchOperation();
            foreach (TableEntity entity in retrievedPublishMetadataEntities)
            {
                batch.Add(TableOperation.Delete(entity));
            }

            await this.Table.ExecuteBatchInChunkAsync(batch);
        }
    }
}
