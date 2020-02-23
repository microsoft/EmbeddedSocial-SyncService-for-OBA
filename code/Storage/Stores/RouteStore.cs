// <copyright file="RouteStore.cs" company="Microsoft">
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
    /// Stores and retrieves route records from Azure Table storage.
    /// </summary>
    public class RouteStore : Store
    {
        /// <summary>
        /// Initializes static members of the <see cref="RouteStore"/> class.
        /// </summary>
        static RouteStore()
        {
            // Map partition key, row key for this type
            AbstractEntityAdapter.RegisterType<RouteEntity>(
                (route) => route.PartitionKey,
                (route) => route.RowKey,
                (route, pk) => { },
                (route, rk) => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteStore"/> class.
        /// </summary>
        /// <param name="cloudTableClient">client to Azure Table</param>
        /// <param name="tableType">type of table</param>
        /// <param name="runId">uniquely identifies an execution of the OBA service</param>
        public RouteStore(CloudTableClient cloudTableClient, TableNames.TableType tableType, string runId)
            : base(cloudTableClient, TableNames.TableName(tableType, runId))
        {
        }

        /// <summary>
        /// Stores a list of routes in Azure Tables
        /// </summary>
        /// <param name="routes">list of routes</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<RouteEntity> routes)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();
            if (routes.Any())
            {
                foreach (var route in routes)
                {
                    batchOperation.Add(TableOperation.Insert(new EntityAdapter<RouteEntity>(route)));
                }

                await this.Table.ExecuteBatchInChunkAsync(batchOperation);
            }
        }

        /// <summary>
        /// Stores a route in Azure Tables
        /// </summary>
        /// <param name="route">route</param>
        /// <returns>task that stores the entity</returns>
        public async Task Insert(RouteEntity route)
        {
            await this.Insert(new List<RouteEntity>() { route });
        }

        /// <summary>
        /// Stores a list of OBA client routes in Azure Tables
        /// </summary>
        /// <param name="routes">list of routes</param>
        /// <param name="regionId">uniquely identifies the region that these routes belong to</param>
        /// <param name="agencyId">uniquely identifies the agency that these routes belong to</param>
        /// <returns>task that stores the entities</returns>
        public async Task Insert(IEnumerable<OBAClient.Model.Route> routes, string regionId, string agencyId)
        {
            // convert the input routes into route entities
            List<RouteEntity> routeEntities = new List<RouteEntity>();
            foreach (OBAClient.Model.Route route in routes)
            {
                RouteEntity routeEntity = new RouteEntity
                {
                    Id = route.Id,
                    ShortName = route.ShortName,
                    LongName = route.LongName,
                    Description = route.Description,
                    Url = route.Url,
                    AgencyId = agencyId,
                    RegionId = regionId,
                    RecordType = RecordType.Route.ToString(),
                    RowState = DataRowState.Default.ToString(),
                    RawContent = route.RawContent,
                };

                routeEntities.Add(routeEntity);
            }

            // issue the insert call
            await this.Insert(routeEntities);
        }

        /// <summary>
        /// Updates a route in Azure Tables
        /// </summary>
        /// <param name="route">route</param>
        /// <returns>task that updates the entity</returns>
        public async Task Update(RouteEntity route)
        {
            // note: this is dangerous because RouteEntity is actually not a table entity.
            // hence it does not contain an Etag that would be used for concurrency control.
            ITableEntity entity = new EntityAdapter<RouteEntity>(route);
            entity.ETag = "*";
            TableOperation replace = TableOperation.Replace(entity);
            await this.Table.ExecuteAsync(replace);
        }

        /// <summary>
        /// Get all routes for a specified region Id
        /// </summary>
        /// <param name="regionId">region Id</param>
        /// <returns>List of routes</returns>
        public async Task<IEnumerable<RouteEntity>> GetAllRoutes(string regionId)
        {
            var downloadedRoutes = new List<RouteEntity>();

            var retrievedRoutes = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                   where entry.PartitionKey == regionId
                                   && entry.Properties["RecordType"].StringValue == RecordType.Route.ToString()
                                   select entry).Resolve(AbstractEntityAdapter.AdapterResolver<RouteEntity>);

            TableContinuationToken continuationToken = null;
            do
            {
                // Execute the query until there are no more results
                var queryResult = await retrievedRoutes.ExecuteSegmentedAsync(continuationToken);
                foreach (var route in queryResult)
                {
                    downloadedRoutes.Add(route);
                }

                continuationToken = queryResult.ContinuationToken;
            }
            while (continuationToken != null);

            return downloadedRoutes;
        }

        /// <summary>
        /// Get all routes for a specified region Id and agency Id
        /// </summary>
        /// <param name="regionId">region Id</param>
        /// <param name="agencyId">agency Id</param>
        /// <returns>List of routes</returns>
        public async Task<IEnumerable<RouteEntity>> GetAllRoutes(string regionId, string agencyId)
        {
            var downloadedRoutes = new List<RouteEntity>();

            var retrievedRoutes = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                   where entry.PartitionKey == regionId
                                   && entry.Properties["RecordType"].StringValue == RecordType.Route.ToString()
                                   && entry.Properties["AgencyId"].StringValue == agencyId
                                   select entry).Resolve(AbstractEntityAdapter.AdapterResolver<RouteEntity>);

            TableContinuationToken continuationToken = null;
            do
            {
                // Execute the query until there are no more results
                var queryResult = await retrievedRoutes.ExecuteSegmentedAsync(continuationToken);
                foreach (var route in queryResult)
                {
                    downloadedRoutes.Add(route);
                }

                continuationToken = queryResult.ContinuationToken;
            }
            while (continuationToken != null);

            return downloadedRoutes;
        }

        /// <summary>
        /// Get a particular route from table storage
        /// </summary>
        /// <param name="regionId">region Id</param>
        /// <param name="agencyId">agency Id</param>
        /// <param name="id">route Id</param>
        /// <returns>route entity</returns>
        public RouteEntity Get(string regionId, string agencyId, string id)
        {
            RouteEntity partialRoute = new RouteEntity { Id = id, RegionId = regionId, AgencyId = agencyId, RecordType = Enum.GetName(typeof(RecordType), RecordType.Route) };

            var retrievedRoutes = (from entry in this.Table.CreateQuery<DynamicTableEntity>()
                                   where entry.PartitionKey == partialRoute.PartitionKey
                                   && entry.RowKey == partialRoute.RowKey
                                   && entry.Properties["RecordType"].StringValue == RecordType.Route.ToString()
                                   select entry).Resolve(AbstractEntityAdapter.AdapterResolver<RouteEntity>).ToList();

            // return null if no records were retrieved
            if (retrievedRoutes == null || retrievedRoutes.Count == 0)
            {
                return null;
            }

            // throw an exception if more than 1 record was received
            if (retrievedRoutes.Count > 1)
            {
                throw new Exception("Expected 1 record but retrieved " + retrievedRoutes.Count + " records.");
            }

            return retrievedRoutes[0];
        }
    }
}
