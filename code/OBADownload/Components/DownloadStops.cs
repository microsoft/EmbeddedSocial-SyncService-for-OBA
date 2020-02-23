// <copyright file="DownloadStops.cs" company="Microsoft">
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
    /// Fetches OBA stops and stores them
    /// </summary>
    public static class DownloadStops
    {
        /// <summary>
        /// Fetches OBA stops for a set of regions and agencies and stores them
        /// </summary>
        /// <param name="routes">downloaded routes for each region, agency pair from DownloadAgencies</param>
        /// <param name="obaClient">interface to the OBA servers</param>
        /// <param name="downloadStorage">interface to storage for downloaded data</param>
        /// <param name="downloadMetadataStorage">interface to storage for download metadata</param>
        /// <param name="runId">uniquely identifies a run of the OBA service</param>
        /// <returns>downloaded stops</returns>
        public static async Task<IEnumerable<Stop>> DownloadAndStore(IEnumerable<Tuple<Region, Agency, IEnumerable<Route>>> routes, Client obaClient, StorageManager downloadStorage, StorageManager downloadMetadataStorage, string runId)
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
            else if (routes == null || routes.Count() < 1)
            {
                throw new ArgumentNullException("routes");
            }

            // a single route can have stops across different agencies.
            // we assume that all stops for a single route will be within the region of that route.
            // hence, we need to download stops across the entire region, dedup those, and then store them.
            // hence, first make a list of all routes per region.
            Dictionary<Region, List<Route>> perRegionRoutes = new Dictionary<Region, List<Route>>();
            foreach (Tuple<Region, Agency, IEnumerable<Route>> triplet in routes)
            {
                if (!perRegionRoutes.ContainsKey(triplet.Item1))
                {
                    perRegionRoutes.Add(triplet.Item1, new List<Route>());
                }

                perRegionRoutes[triplet.Item1].AddRange(triplet.Item3);
            }

            // fetch stops from every region in parallel and stick them into a concurrent bag.
            // it is safe to do this in parallel because :
            // (a) use of a ConcurrentBag in the final step of collecting stops into a single list
            // (b) each of the calls into obaClient, downloadStorage, downloadMetadataStorage are thread safe (no shared static variables that change; operations are independent of each other)
            List<Task> tasks = new List<Task>();
            ConcurrentBag<Stop> stops = new ConcurrentBag<Stop>();
            foreach (KeyValuePair<Region, List<Route>> perRegionRoute in perRegionRoutes.ToList())
            {
                Task task = new Task(() =>
                {
                    IEnumerable<Stop> fetchedStops = DownloadAndStore(perRegionRoute.Key, perRegionRoute.Value, obaClient, downloadStorage, downloadMetadataStorage, runId).Result;
                    foreach (Stop fetchedStop in fetchedStops)
                    {
                        stops.Add(fetchedStop);
                    }
                });
                tasks.Add(task);
                task.Start();
            }

            await Task.WhenAll(tasks.ToArray());
            return stops.ToList();
        }

        /// <summary>
        /// Fetches OBA stops from one region and stores them
        /// </summary>
        /// <param name="region">an OBA region</param>
        /// <param name="routes">a list of routes for that region</param>
        /// <param name="obaClient">interface to the OBA servers</param>
        /// <param name="downloadStorage">interface to storage for downloaded data</param>
        /// <param name="downloadMetadataStorage">interface to storage for download metadata</param>
        /// <param name="runId">uniquely identifies a run of the OBA service</param>
        /// <returns>downloaded stops</returns>
        private static async Task<IEnumerable<Stop>> DownloadAndStore(Region region, IEnumerable<Route> routes, Client obaClient, StorageManager downloadStorage, StorageManager downloadMetadataStorage, string runId)
        {
            // fetch stop details for each route in parallel and stick them into a concurrent bag.
            // it is safe to do this in parallel because :
            // (a) use of a ConcurrentBag in the final step of collecting stop details into a single list
            // (b) each of the calls into obaClient are thread safe (no shared static variables that change; operations are independent of each other)
            List<Task> tasks = new List<Task>();
            ConcurrentBag<Stop> stops = new ConcurrentBag<Stop>();
            foreach (Route route in routes)
            {
                Task task = new Task(() =>
                {
                    Stop[] stopsForRoute = new Stop[0];
                    try
                    {
                        stopsForRoute = obaClient.GetStopsForRouteAsync(region, route.Id).Result;
                    }
                    catch
                    {
                        if (!route.Id.Contains("/"))
                        {
                            throw;
                        }
                    }
                    foreach (Stop stopForRoute in stopsForRoute)
                    {
                        stops.Add(stopForRoute);
                    }
                });
                tasks.Add(task);
                task.Start();
            }

            await Task.WhenAll(tasks.ToArray());

            // dedup stops
            IEnumerable<Stop> uniqueStops = stops.Distinct<Stop>(new StopEqualityComparerById());

            // store them
            await downloadStorage.StopStore.Insert(uniqueStops, region.Id);

            // add a metadata entry
            await downloadMetadataStorage.DownloadMetadataStore.Insert(runId, region.Id, string.Empty, RecordType.Stop, uniqueStops.Count());

            return uniqueStops;
        }
    }
}
