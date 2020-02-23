// <copyright file="DiffStops.cs" company="Microsoft">
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
    /// Compares downloaded OBA stops to published OBA stops and stores a diff
    /// </summary>
    public static class DiffStops
    {
        /// <summary>
        /// Compares downloaded OBA stops to published OBA stops and stores a diff
        /// </summary>
        /// <param name="downloadStorage">interface to downloaded OBA data</param>
        /// <param name="publishStorage">interface to data published to Embedded Social</param>
        /// <param name="diffStorage">interface to diff data</param>
        /// <param name="diffMetadataStorage">inteface to diff metadata</param>
        /// <param name="runId">uniquely identifies a run of the service</param>
        /// <returns>task that computes and stores a diff of stops</returns>
        public static async Task DiffAndStore(StorageManager downloadStorage, StorageManager publishStorage, StorageManager diffStorage, StorageManager diffMetadataStorage, string runId)
        {
            // Note: This code assumes that an entire region does not get deleted.
            // I.e., it assumes that a region for which we previously published stops
            // continues to appear in downloaded OBA data. If the entire region disappears,
            // then this code ignores that and leaves those stops active. An entire region
            // dissapearing seems like a major event that a human operator should be involved
            // in the decision to delete those stops.

            // get all the downloaded regions
            IEnumerable<string> regionIds = downloadStorage.RegionStore.GetAllRegionIds();

            // process stops per region in parallel.
            // it is ok to execute these in parallel because the operations being done are independent of each other.
            List<Task> tasks = new List<Task>();
            foreach (string regionId in regionIds)
            {
                Task task = Task.Run(async () =>
                {
                    IEnumerable<StopEntity> downloadedStops = await downloadStorage.StopStore.GetAllStops(regionId);
                    IEnumerable<StopEntity> publishedStops = await publishStorage.StopStore.GetAllStops(regionId);
                    await DiffAndStore(downloadedStops, publishedStops, diffStorage, diffMetadataStorage, regionId, runId);
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// Compares downloaded OBA stops to published OBA stops for a single region and stores a diff
        /// </summary>
        /// <param name="downloadedStops">list of stops downloaded from OBA servers</param>
        /// <param name="publishedStops">list of stops published to Embedded Social</param>
        /// <param name="diffStorage">interface to diff data</param>
        /// <param name="diffMetadataStorage">inteface to diff metadata</param>
        /// <param name="regionId">uniquely identifies the region</param>
        /// <param name="runId">uniquely identifies a run of the service</param>
        /// <returns>task that computes and stores a diff of stops</returns>
        private static async Task DiffAndStore(IEnumerable<StopEntity> downloadedStops, IEnumerable<StopEntity> publishedStops, StorageManager diffStorage, StorageManager diffMetadataStorage, string regionId, string runId)
        {
            // create empty lists of the four different types of changes to be published
            List<StopEntity> newStops = new List<StopEntity>();
            List<StopEntity> updatedStops = new List<StopEntity>();
            List<StopEntity> deletedStops = new List<StopEntity>();
            List<StopEntity> resurrectedStops = new List<StopEntity>();

            // in the common case, most stop information will not change.
            // so, start by removing those items that are common to both considering their relevant content
            IEnumerable<StopEntity> unchangedStops = downloadedStops.Intersect(publishedStops.Where(r => r.RowState != DataRowState.Delete.ToString()), new StopEqualityComparerByChecksum());
            downloadedStops = downloadedStops.Except(unchangedStops, new StopEqualityComparerById());
            publishedStops = publishedStops.Except(unchangedStops, new StopEqualityComparerById());

            // new stops are those in downloadedStops but not in publishedStops when solely considering stop Ids
            newStops = downloadedStops.Except(publishedStops, new StopEqualityComparerById()).ToList();

            // deleted stops are those in publishedStops but not in downloadedStops when solely considering stopIds,
            // and are not already marked as deleted in publishedStops
            deletedStops = publishedStops.Except(downloadedStops, new StopEqualityComparerById()).Where(r => r.RowState != DataRowState.Delete.ToString()).ToList();

            // resurrected stops are those in publishedStops that are marked as deleted but are in downloadedStops when solely considering stopIds
            resurrectedStops = downloadedStops.Intersect(publishedStops.Where(r => r.RowState == DataRowState.Delete.ToString()), new StopEqualityComparerById()).ToList();

            // updated stops : first get the stops that are in common by Id that have not been resurrected,
            // then pick those who's contents are different
            updatedStops = downloadedStops.Intersect(publishedStops.Where(r => r.RowState != DataRowState.Delete.ToString()), new StopEqualityComparerById()).ToList();
            updatedStops = updatedStops.Except(publishedStops, new StopEqualityComparerByChecksum()).ToList();

            // set the appropriate datarowstate values
            newStops.ForEach(r => r.RowState = DataRowState.Create.ToString());
            updatedStops.ForEach(r => r.RowState = DataRowState.Update.ToString());
            deletedStops.ForEach(r => r.RowState = DataRowState.Delete.ToString());
            resurrectedStops.ForEach(r => r.RowState = DataRowState.Resurrect.ToString());

            // write to diff table
            await diffStorage.StopStore.Insert(newStops);
            await diffStorage.StopStore.Insert(updatedStops);
            await diffStorage.StopStore.Insert(deletedStops);
            await diffStorage.StopStore.Insert(resurrectedStops);

            // write to metadata table
            DiffMetadataEntity metadata = new DiffMetadataEntity
            {
                RunId = runId,
                RegionId = regionId,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = newStops.Count,
                UpdatedCount = updatedStops.Count,
                DeletedCount = deletedStops.Count,
                ResurrectedCount = resurrectedStops.Count
            };
            await diffMetadataStorage.DiffMetadataStore.Insert(new List<DiffMetadataEntity> { metadata });
        }
    }
}