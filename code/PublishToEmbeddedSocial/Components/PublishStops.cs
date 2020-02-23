// <copyright file="PublishStops.cs" company="Microsoft">
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
    /// Publishes stop information in diff storage to Embedded Social
    /// </summary>
    public static class PublishStops
    {
        /// <summary>
        /// Publishes stop information in diff storage to Embedded Social
        /// </summary>
        /// <param name="diffStorage">interface to diff data</param>
        /// <param name="diffMetadataStorage">interface to diff metadata</param>
        /// <param name="publishStorage">interface to data published to Embedded Social</param>
        /// <param name="publishMetadataStorage">inteface to publish metadata</param>
        /// <param name="embeddedSocial">interface to Embedded Social</param>
        /// <param name="runId">uniquely identifies a run of the service</param>
        /// <returns>task that publishes stops from diff tables to Embedded Social</returns>
        public static async Task PublishStopsToEmbeddedSocial(StorageManager diffStorage, StorageManager diffMetadataStorage, StorageManager publishStorage, StorageManager publishMetadataStorage, EmbeddedSocial embeddedSocial, string runId)
        {
            // get all the diff metadata
            IEnumerable<DiffMetadataEntity> diffMetadata = diffMetadataStorage.DiffMetadataStore.Get(runId);

            // if there are no entries, then there is nothing to do
            if (diffMetadata == null || diffMetadata.Count() < 1)
            {
                return;
            }

            // select the stop diff metadata entries
            diffMetadata = from entity in diffMetadata
                           where entity.RecordType == RecordType.Stop.ToString()
                           select entity;

            // if there are no entries, then there is nothing to do
            if (diffMetadata == null || diffMetadata.Count() < 1)
            {
                return;
            }

            // each entry covers a region.
            // process stops per region in parallel.
            // it is ok to execute these in parallel because the operations being done are independent of each other.
            List<Task> tasks = new List<Task>();
            foreach (DiffMetadataEntity diffMetadataEntity in diffMetadata)
            {
                Task task = Task.Run(async () =>
                {
                    string regionId = diffMetadataEntity.RegionId;
                    IEnumerable<StopEntity> diffStops = await diffStorage.StopStore.GetAllStops(regionId);
                    await PublishStopsToEmbeddedSocial(diffStops, regionId, publishStorage, publishMetadataStorage, embeddedSocial, runId);
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// Publishes stop information in diff storage to Embedded Social for a region
        /// </summary>
        /// <param name="diffStops">list of stop entities to publish</param>
        /// <param name="regionId">region that the stop entities belong to</param>
        /// <param name="publishStorage">interface to data published to Embedded Social</param>
        /// <param name="publishMetadataStorage">inteface to publish metadata</param>
        /// <param name="embeddedSocial">interface to Embedded Social</param>
        /// <param name="runId">uniquely identifies a run of the service</param>
        /// <returns>task that publishes stops for an agency to Embedded Social</returns>
        public static async Task PublishStopsToEmbeddedSocial(IEnumerable<StopEntity> diffStops, string regionId, StorageManager publishStorage, StorageManager publishMetadataStorage, EmbeddedSocial embeddedSocial, string runId)
        {
            // create the publish metadata entity
            PublishMetadataEntity publishMetadataEntity = new PublishMetadataEntity()
            {
                RunId = runId,
                RegionId = regionId,
                AgencyId = string.Empty,
                RecordType = Storage.RecordType.Stop.ToString(),
                AddedCount = 0,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            };

            // iterate over each stop
            foreach (StopEntity diffStop in diffStops)
            {
                switch ((DataRowState)Enum.Parse(typeof(DataRowState), diffStop.RowState))
                {
                    case DataRowState.Create:
                        await embeddedSocial.CreateStop(diffStop);
                        await publishStorage.StopStore.Insert(diffStop);
                        publishMetadataEntity.AddedCount++;
                        break;

                    case DataRowState.Delete:
                        await embeddedSocial.DeleteStop(diffStop);
                        await publishStorage.StopStore.Update(diffStop);
                        publishMetadataEntity.DeletedCount++;
                        break;

                    case DataRowState.Update:
                        await embeddedSocial.UpdateStop(diffStop);
                        await publishStorage.StopStore.Update(diffStop);
                        publishMetadataEntity.UpdatedCount++;
                        break;

                    case DataRowState.Resurrect:
                        await embeddedSocial.ResurrectStop(diffStop);
                        await publishStorage.StopStore.Update(diffStop);
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