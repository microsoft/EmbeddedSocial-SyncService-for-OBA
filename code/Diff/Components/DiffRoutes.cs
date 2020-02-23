// <copyright file="DiffRoutes.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Diff
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OBAService.Storage;
    using OBAService.Storage.Model;

    /// <summary>
    /// Compares downloaded OBA routes to published OBA routes and stores a diff
    /// </summary>
    public static class DiffRoutes
    {
        /// <summary>
        /// Compares downloaded OBA routes to published OBA routes and stores a diff
        /// </summary>
        /// <param name="downloadStorage">interface to downloaded OBA data</param>
        /// <param name="publishStorage">interface to data published to Embedded Social</param>
        /// <param name="diffStorage">interface to diff data</param>
        /// <param name="diffMetadataStorage">inteface to diff metadata</param>
        /// <param name="runId">uniquely identifies a run of the service</param>
        /// <returns>task that computes and stores a diff of routes</returns>
        public static async Task DiffAndStore(StorageManager downloadStorage, StorageManager publishStorage, StorageManager diffStorage, StorageManager diffMetadataStorage, string runId)
        {
            // Note: This code assumes that an entire agency does not get deleted.
            // I.e., it assumes that an agency for which we previously published routes
            // continues to appear in downloaded OBA data. If the entire agency disappears,
            // then this code ignores that and leaves those routes active. An entire agency
            // dissapearing seems like a major event that a human operator should be involved
            // in the decision to delete those routes.

            // get all the downloaded regions
            IEnumerable<string> regionIds = downloadStorage.RegionStore.GetAllRegionIds();

            // process routes per agency in parallel.
            // it is ok to execute these in parallel because the operations being done are independent of each other.
            List<Task> tasks = new List<Task>();
            foreach (string regionId in regionIds)
            {
                IEnumerable<AgencyEntity> agencies = await downloadStorage.AgencyStore.GetAllAgencies(regionId);
                foreach (string agencyId in agencies.Select(a => a.Id))
                {
                    Task task = Task.Run(async () =>
                    {
                        IEnumerable<RouteEntity> downloadedRoutes = await downloadStorage.RouteStore.GetAllRoutes(regionId, agencyId);
                        IEnumerable<RouteEntity> publishedRoutes = await publishStorage.RouteStore.GetAllRoutes(regionId, agencyId);
                        await DiffAndStore(downloadedRoutes, publishedRoutes, diffStorage, diffMetadataStorage, regionId, agencyId, runId);
                    });
                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// Compares downloaded OBA routes to published OBA routes for a single region and stores a diff
        /// </summary>
        /// <param name="downloadedRoutes">list of routes downloaded from OBA servers</param>
        /// <param name="publishedRoutes">list of routes published to Embedded Social</param>
        /// <param name="diffStorage">interface to diff data</param>
        /// <param name="diffMetadataStorage">inteface to diff metadata</param>
        /// <param name="regionId">uniquely identifies the region</param>
        /// <param name="agencyId">uniquely identifies the agency</param>
        /// <param name="runId">uniquely identifies a run of the service</param>
        /// <returns>task that computes and stores a diff of routes</returns>
        private static async Task DiffAndStore(IEnumerable<RouteEntity> downloadedRoutes, IEnumerable<RouteEntity> publishedRoutes, StorageManager diffStorage, StorageManager diffMetadataStorage, string regionId, string agencyId, string runId)
        {
            // create empty lists of the four different types of changes to be published
            List<RouteEntity> newRoutes = new List<RouteEntity>();
            List<RouteEntity> updatedRoutes = new List<RouteEntity>();
            List<RouteEntity> deletedRoutes = new List<RouteEntity>();
            List<RouteEntity> resurrectedRoutes = new List<RouteEntity>();

            // in the common case, most route information will not change.
            // so, start by removing those items that are common to both considering their relevant content
            IEnumerable<RouteEntity> unchangedRoutes = downloadedRoutes.Intersect(publishedRoutes.Where(r => r.RowState != DataRowState.Delete.ToString()), new RouteEqualityComparerByChecksum());
            downloadedRoutes = downloadedRoutes.Except(unchangedRoutes, new RouteEqualityComparerById());
            publishedRoutes = publishedRoutes.Except(unchangedRoutes, new RouteEqualityComparerById());

            // new routes are those in downloadedRoutes but not in publishedRoutes when solely considering route Ids
            newRoutes = downloadedRoutes.Except(publishedRoutes, new RouteEqualityComparerById()).ToList();

            // deleted routes are those in publishedRoutes but not in downloadedRoutes when solely considering routeIds,
            // and are not already marked as deleted in publishedRoutes
            deletedRoutes = publishedRoutes.Except(downloadedRoutes, new RouteEqualityComparerById()).Where(r => r.RowState != DataRowState.Delete.ToString()).ToList();

            // resurrected routes are those in publishedRoutes that are marked as deleted but are in downloadedRoutes when solely considering routeIds
            resurrectedRoutes = downloadedRoutes.Intersect(publishedRoutes.Where(r => r.RowState == DataRowState.Delete.ToString()), new RouteEqualityComparerById()).ToList();

            // updated routes : first get the routes that are in common by Id that have not been resurrected,
            // then pick those who's contents are different
            updatedRoutes = downloadedRoutes.Intersect(publishedRoutes.Where(r => r.RowState != DataRowState.Delete.ToString()), new RouteEqualityComparerById()).ToList();
            updatedRoutes = updatedRoutes.Except(publishedRoutes, new RouteEqualityComparerByChecksum()).ToList();

            // set the appropriate datarowstate values
            newRoutes.ForEach(r => r.RowState = DataRowState.Create.ToString());
            updatedRoutes.ForEach(r => r.RowState = DataRowState.Update.ToString());
            deletedRoutes.ForEach(r => r.RowState = DataRowState.Delete.ToString());
            resurrectedRoutes.ForEach(r => r.RowState = DataRowState.Resurrect.ToString());

            // write to diff table
            await diffStorage.RouteStore.Insert(newRoutes);
            await diffStorage.RouteStore.Insert(updatedRoutes);
            await diffStorage.RouteStore.Insert(deletedRoutes);
            await diffStorage.RouteStore.Insert(resurrectedRoutes);

            // write to metadata table
            DiffMetadataEntity metadata = new DiffMetadataEntity
            {
                RunId = runId,
                RegionId = regionId,
                AgencyId = agencyId,
                RecordType = RecordType.Route.ToString(),
                AddedCount = newRoutes.Count,
                UpdatedCount = updatedRoutes.Count,
                DeletedCount = deletedRoutes.Count,
                ResurrectedCount = resurrectedRoutes.Count
            };
            await diffMetadataStorage.DiffMetadataStore.Insert(new List<DiffMetadataEntity> { metadata });
        }
    }
}