// <copyright file="PublishRoutes.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.PublishToEmbeddedSocial
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OBAService.Storage;
    using OBAService.Storage.Model;

    /// <summary>
    /// Publishes route information in diff storage to Embedded Social
    /// </summary>
    public static class PublishRoutes
    {
        /// <summary>
        /// Publishes route information in diff storage to Embedded Social
        /// </summary>
        /// <param name="diffStorage">interface to diff data</param>
        /// <param name="diffMetadataStorage">interface to diff metadata</param>
        /// <param name="publishStorage">interface to data published to Embedded Social</param>
        /// <param name="publishMetadataStorage">inteface to publish metadata</param>
        /// <param name="embeddedSocial">interface to Embedded Social</param>
        /// <param name="runId">uniquely identifies a run of the service</param>
        /// <returns>task that publishes routes from diff tables to Embedded Social</returns>
        public static async Task PublishRoutesToEmbeddedSocial(StorageManager diffStorage, StorageManager diffMetadataStorage, StorageManager publishStorage, StorageManager publishMetadataStorage, EmbeddedSocial embeddedSocial, string runId)
        {
            // get all the diff metadata
            IEnumerable<DiffMetadataEntity> diffMetadata = diffMetadataStorage.DiffMetadataStore.Get(runId);

            // if there are no entries, then there is nothing to do
            if (diffMetadata == null || diffMetadata.Count() < 1)
            {
                return;
            }

            // select the route diff metadata entries
            diffMetadata = from entity in diffMetadata
                           where entity.RecordType == RecordType.Route.ToString()
                           select entity;

            // if there are no entries, then there is nothing to do
            if (diffMetadata == null || diffMetadata.Count() < 1)
            {
                return;
            }

            // each entry covers an agency.
            // process routes per agency in parallel.
            // it is ok to execute these in parallel because the operations being done are independent of each other.
            List<Task> tasks = new List<Task>();
            foreach (DiffMetadataEntity diffMetadataEntity in diffMetadata)
            {
                Task task = Task.Run(async () =>
                {
                    string regionId = diffMetadataEntity.RegionId;
                    string agencyId = diffMetadataEntity.AgencyId;
                    IEnumerable<RouteEntity> diffRoutes = await diffStorage.RouteStore.GetAllRoutes(regionId, agencyId);
                    await PublishRoutesToEmbeddedSocial(diffRoutes, regionId, agencyId, publishStorage, publishMetadataStorage, embeddedSocial, runId);
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// Publishes route information in diff storage to Embedded Social for an agency
        /// </summary>
        /// <param name="diffRoutes">list of route entities to publish</param>
        /// <param name="regionId">region that the route entities belong to</param>
        /// <param name="agencyId">agency that the route entities belong to</param>
        /// <param name="publishStorage">interface to data published to Embedded Social</param>
        /// <param name="publishMetadataStorage">inteface to publish metadata</param>
        /// <param name="embeddedSocial">interface to Embedded Social</param>
        /// <param name="runId">uniquely identifies a run of the service</param>
        /// <returns>task that publishes routes for an agency to Embedded Social</returns>
        public static async Task PublishRoutesToEmbeddedSocial(IEnumerable<RouteEntity> diffRoutes, string regionId, string agencyId, StorageManager publishStorage, StorageManager publishMetadataStorage, EmbeddedSocial embeddedSocial, string runId)
        {
            // create the publish metadata entity
            PublishMetadataEntity publishMetadataEntity = new PublishMetadataEntity()
            {
                RunId = runId,
                RegionId = regionId,
                AgencyId = agencyId,
                RecordType = Storage.RecordType.Route.ToString(),
                AddedCount = 0,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            };

            // iterate over each route
            foreach (RouteEntity diffRoute in diffRoutes)
            {
                switch ((DataRowState)Enum.Parse(typeof(DataRowState), diffRoute.RowState))
                {
                    case DataRowState.Create:
                        await embeddedSocial.CreateRoute(diffRoute);
                        await publishStorage.RouteStore.Insert(diffRoute);
                        publishMetadataEntity.AddedCount++;
                        break;

                    case DataRowState.Delete:
                        await embeddedSocial.DeleteRoute(diffRoute);
                        await publishStorage.RouteStore.Update(diffRoute);
                        publishMetadataEntity.DeletedCount++;
                        break;

                    case DataRowState.Update:
                        await embeddedSocial.UpdateRoute(diffRoute);
                        await publishStorage.RouteStore.Update(diffRoute);
                        publishMetadataEntity.UpdatedCount++;
                        break;

                    case DataRowState.Resurrect:
                        await embeddedSocial.ResurrectRoute(diffRoute);
                        await publishStorage.RouteStore.Update(diffRoute);
                        publishMetadataEntity.ResurrectedCount++;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("RowState");
                }
            }

            // publish the metadata entity
            await publishMetadataStorage.PublishMetadataStore.Insert(publishMetadataEntity);
        }
    }
}