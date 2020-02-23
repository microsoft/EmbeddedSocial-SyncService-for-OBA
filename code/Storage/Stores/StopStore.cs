// <copyright file="StopStore.cs" company="Microsoft">
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
    /// Stores and retrieves stop records from Azure Table storage.
    /// </summary>
    public class StopStore : Store
    {
        /// <summary>
        /// Initializes static members of the <see cref="StopStore"/> class.
        /// </summary>
        static StopStore()
        {
            // Map partition key, row key for this type
            AbstractEntityAdapter.RegisterType<StopEntity>(
                (stop) => stop.PartitionKey,
                (stop) => stop.RowKey,
                (stop, pk) => { },
                (stop, rk) => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StopStore"/> class.
        /// </summary>
        /// <param name="cloudTableClient">client to Azure Table</param>
        /// <param name="tableType">type of table</param>
        /// <param name="runId">uniquely identifies an execution of the OBA service</param>
        public StopStore(CloudTableClient cloudTableClient, TableNames.TableType tableType, string runId)
            : base(cloudTableClient, TableNames.TableName(tableType, runId))
        {
        }

        /// <summary>
        /// Stores a list of stops in Azure Tables
        /// </summary>
        /// <param name="stops">list of stops</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<StopEntity> stops)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();
            if (stops.Any())
            {
                foreach (var stop in stops)
                {
                    batchOperation.Add(TableOperation.Insert(new EntityAdapter<StopEntity>(stop)));
                }

                await this.Table.ExecuteBatchInChunkAsync(batchOperation);
            }
        }

        /// <summary>
        /// Stores a stop in Azure Tables
        /// </summary>
        /// <param name="stop">stop</param>
        /// <returns>task that stores the entity</returns>
        public async Task Insert(StopEntity stop)
        {
            await this.Insert(new List<StopEntity>() { stop });
        }

        /// <summary>
        /// Stores a list of OBA client stops in Azure Tables
        /// </summary>
        /// <param name="stops">list of stops</param>
        /// <param name="regionId">uniquely identifies the region that these stops belong to</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<OBAClient.Model.Stop> stops, string regionId)
        {
            // convert the input stops into stop entities
            List<StopEntity> stopEntities = new List<StopEntity>();
            foreach (OBAClient.Model.Stop stop in stops)
            {
                StopEntity stopEntity = new StopEntity
                {
                    Id = stop.Id,
                    Lat = stop.Lat,
                    Lon = stop.Lon,
                    Direction = stop.Direction,
                    Name = stop.Name,
                    Code = stop.Code,
                    RegionId = regionId,
                    RecordType = RecordType.Stop.ToString(),
                    RowState = DataRowState.Default.ToString(),
                    RawContent = stop.RawContent,
                };

                stopEntities.Add(stopEntity);
            }

            await this.Insert(stopEntities);
        }

        /// <summary>
        /// Updates a stop in Azure Tables
        /// </summary>
        /// <param name="stop">stop</param>
        /// <returns>task that updates the entity</returns>
        public async Task Update(StopEntity stop)
        {
            // note: this is dangerous because StopEntity is actually not a table entity.
            // hence it does not contain an Etag that would be used for concurrency control.
            ITableEntity entity = new EntityAdapter<StopEntity>(stop);
            entity.ETag = "*";
            TableOperation replace = TableOperation.Replace(entity);
            await this.Table.ExecuteAsync(replace);
        }

        /// <summary>
        /// Get all stops for a specified region Id
        /// </summary>
        /// <param name="regionId">regionId</param>
        /// <returns>List of stops</returns>
        public async Task<IEnumerable<StopEntity>> GetAllStops(string regionId)
        {
            var downloadedStops = new List<StopEntity>();
            var retrievedStops = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                  where entry.PartitionKey == regionId
                                  && entry.Properties["RecordType"].StringValue == RecordType.Stop.ToString()
                                  select entry).Resolve(AbstractEntityAdapter.AdapterResolver<StopEntity>);

            TableContinuationToken continuationToken = null;
            do
            {
                // Execute the query until there are no more results
                var queryResult = await retrievedStops.ExecuteSegmentedAsync(continuationToken);
                foreach (var stop in queryResult)
                {
                    downloadedStops.Add(stop);
                }

                continuationToken = queryResult.ContinuationToken;
            }
            while (continuationToken != null);

            return downloadedStops;
        }

        /// <summary>
        /// Get a particular stop from table storage
        /// </summary>
        /// <param name="regionId">region Id</param>
        /// <param name="id">stop Id</param>
        /// <returns>stop entity</returns>
        public StopEntity Get(string regionId, string id)
        {
            StopEntity partialStop = new StopEntity { Id = id, RegionId = regionId, RecordType = Enum.GetName(typeof(RecordType), RecordType.Stop) };

            var retrievedStops = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                  where entry.PartitionKey == partialStop.PartitionKey
                                  && entry.RowKey == partialStop.RowKey
                                  && entry.Properties["RecordType"].StringValue == RecordType.Stop.ToString()
                                  select entry).Resolve(AbstractEntityAdapter.AdapterResolver<StopEntity>).ToList();

            // return null if no records were retrieved
            if (retrievedStops == null || retrievedStops.Count == 0)
            {
                return null;
            }

            // throw an exception if more than 1 record was received
            if (retrievedStops.Count > 1)
            {
                throw new Exception("Expected 1 record but retrieved " + retrievedStops.Count + " records.");
            }

            return retrievedStops[0];
        }
    }
}
