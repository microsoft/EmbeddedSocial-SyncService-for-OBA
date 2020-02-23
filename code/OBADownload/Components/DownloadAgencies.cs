// <copyright file="DownloadAgencies.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBADownload
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OBAService.OBAClient;
    using OBAService.OBAClient.Model;
    using OBAService.Storage;

    /// <summary>
    /// Fetches OBA agencies and stores them
    /// </summary>
    public static class DownloadAgencies
    {
        /// <summary>
        /// Fetches OBA agencies for a set of regions and stores them
        /// </summary>
        /// <param name="regions">OBA regions from ProcessRegions</param>
        /// <param name="obaClient">interface to the OBA servers</param>
        /// <param name="downloadStorage">interface to storage for downloaded data</param>
        /// <param name="downloadMetadataStorage">interface to storage for download metadata</param>
        /// <param name="runId">uniquely identifies a run of the OBA service</param>
        /// <returns>downloaded pairs of regions and agencies</returns>
        public static async Task<IEnumerable<Tuple<Region, Agency>>> DownloadAndStore(IEnumerable<Region> regions, Client obaClient, StorageManager downloadStorage, StorageManager downloadMetadataStorage, string runId)
        {
            // check inputs
            if (obaClient == null)
            {
                throw new ArgumentNullException("obaClient");
            }
            else if (downloadStorage == null)
            {
                throw new ArgumentNullException("downloadStorage");
            }
            else if (downloadMetadataStorage == null)
            {
                throw new ArgumentNullException("downloadMetadataStorage");
            }
            else if (string.IsNullOrWhiteSpace(runId))
            {
                throw new ArgumentNullException("runId");
            }
            else if (regions == null || regions.Count() < 1)
            {
                throw new ArgumentNullException("regions");
            }

            // fetch agencies from every region in parallel and stick them into a concurrent bag.
            // it is safe to do this in parallel because :
            // (a) use of a ConcurrentBag in the final step of collecting agencies into a single list
            // (b) each of the calls into obaClient, downloadStorage, downloadMetadataStorage are thread safe (no shared static variables that change; operations are independent of each other)
            List<Task> tasks = new List<Task>();
            ConcurrentBag<Tuple<Region, Agency>> agencies = new ConcurrentBag<Tuple<Region, Agency>>();
            foreach (Region region in regions)
            {
                Task task = new Task(() =>
                {
                    IEnumerable<Agency> fetchedAgencies = DownloadAndStore(region, obaClient, downloadStorage, downloadMetadataStorage, runId).Result;
                    foreach (Agency fetchedAgency in fetchedAgencies)
                    {
                        agencies.Add(new Tuple<Region, Agency>(region, fetchedAgency));
                    }
                });
                tasks.Add(task);
                task.Start();
            }

            await Task.WhenAll(tasks.ToArray());
            return agencies.ToList();
        }

        /// <summary>
        /// Fetches OBA agencies from one region and stores them
        /// </summary>
        /// <param name="region">an OBA region</param>
        /// <param name="obaClient">interface to the OBA servers</param>
        /// <param name="downloadStorage">interface to storage for downloaded data</param>
        /// <param name="downloadMetadataStorage">interface to storage for download metadata</param>
        /// <param name="runId">uniquely identifies a run of the OBA service</param>
        /// <returns>downloaded agencies</returns>
        private static async Task<IEnumerable<Agency>> DownloadAndStore(Region region, Client obaClient, StorageManager downloadStorage, StorageManager downloadMetadataStorage, string runId)
        {
            IEnumerable<Agency> agencies = await obaClient.GetAllAgenciesAsync(region);

            // Check agencies.
            // Will not do extensive checks because technically we can get back no agencies from a region
            if (agencies == null)
            {
                throw new Exception("agencies is null");
            }

            // store them
            await downloadStorage.AgencyStore.Insert(agencies, region.Id);

            // add a metadata entry
            await downloadMetadataStorage.DownloadMetadataStore.Insert(runId, region.Id, string.Empty, RecordType.Agency, agencies.Count());

            return agencies;
        }
    }
}
