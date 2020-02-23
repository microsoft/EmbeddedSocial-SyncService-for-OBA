// <copyright file="DownloadRoutes.cs" company="Microsoft">
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
    /// Fetches OBA routes and stores them
    /// </summary>
    public static class DownloadRoutes
    {
        /// <summary>
        /// Fetches OBA routes for a set of regions and agencies and stores them
        /// </summary>
        /// <param name="agencies">pairs of OBA region and agency from DownloadAgencies</param>
        /// <param name="obaClient">interface to the OBA servers</param>
        /// <param name="downloadStorage">interface to storage for downloaded data</param>
        /// <param name="downloadMetadataStorage">interface to storage for download metadata</param>
        /// <param name="runId">uniquely identifies a run of the OBA service</param>
        /// <returns>downloaded routes for each region, agency pair</returns>
        public static async Task<IEnumerable<Tuple<Region, Agency, IEnumerable<Route>>>> DownloadAndStore(IEnumerable<Tuple<Region, Agency>> agencies, Client obaClient, StorageManager downloadStorage, StorageManager downloadMetadataStorage, string runId)
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
            else if (agencies == null || agencies.Count() < 1)
            {
                throw new ArgumentNullException("agencies");
            }

            // fetch routes from every agency in parallel and stick them into a concurrent bag.
            // it is safe to do this in parallel because :
            // (a) use of a ConcurrentBag in the final step of collecting routes into a single list
            // (b) each of the calls into obaClient, downloadStorage, downloadMetadataStorage are thread safe (no shared static variables that change; operations are independent of each other)
            List<Task> tasks = new List<Task>();
            ConcurrentBag<Tuple<Region, Agency, IEnumerable<Route>>> routes = new ConcurrentBag<Tuple<Region, Agency, IEnumerable<Route>>>();
            foreach (Tuple<Region, Agency> pair in agencies)
            {
                Task task = new Task(() =>
                {
                    IEnumerable<Route> fetchedRoutes = DownloadAndStore(pair.Item1, pair.Item2, obaClient, downloadStorage, downloadMetadataStorage, runId).Result;
                    routes.Add(new Tuple<Region, Agency, IEnumerable<Route>>(pair.Item1, pair.Item2, fetchedRoutes));
                });
                tasks.Add(task);
                task.Start();
            }

            await Task.WhenAll(tasks.ToArray());
            return routes.ToList();
        }

        /// <summary>
        /// Fetches OBA routes from one agency and stores them
        /// </summary>
        /// <param name="region">an OBA region</param>
        /// <param name="agency">an OBA agency</param>
        /// <param name="obaClient">interface to the OBA servers</param>
        /// <param name="downloadStorage">interface to storage for downloaded data</param>
        /// <param name="downloadMetadataStorage">interface to storage for download metadata</param>
        /// <param name="runId">uniquely identifies a run of the OBA service</param>
        /// <returns>downloaded routes</returns>
        private static async Task<IEnumerable<Route>> DownloadAndStore(Region region, Agency agency, Client obaClient, StorageManager downloadStorage, StorageManager downloadMetadataStorage, string runId)
        {
            IEnumerable<Route> routes = await obaClient.GetAllRoutesAsync(region, agency);

            // Check routes.
            // Will not do extensive checks because technically we can get back no routes from an agency
            if (routes == null)
            {
                throw new Exception("routes is null");
            }

            // store them
            await downloadStorage.RouteStore.Insert(routes, region.Id, agency.Id);

            // add a metadata entry
            await downloadMetadataStorage.DownloadMetadataStore.Insert(runId, region.Id, agency.Id, RecordType.Route, routes.Count());

            return routes;
        }
    }
}
