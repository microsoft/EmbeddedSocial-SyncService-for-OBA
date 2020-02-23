// <copyright file="DownloadRegionsList.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBADownload
{
    using System;
    using System.Threading.Tasks;

    using OBAService.OBAClient;
    using OBAService.OBAClient.Model;
    using OBAService.Storage;

    /// <summary>
    /// Fetches OBA regions list and stores it
    /// </summary>
    public static class DownloadRegionsList
    {
        /// <summary>
        /// Fetches OBA regions list and stores it
        /// </summary>
        /// <param name="obaClient">interface to the OBA servers</param>
        /// <param name="downloadStorage">interface to storage for downloaded data</param>
        /// <param name="downloadMetadataStorage">interface to storage for download metadata</param>
        /// <param name="runId">uniquely identifies a run of the OBA service</param>
        /// <returns>downloaded regions list</returns>
        public static async Task<RegionsList> DownloadAndStore(Client obaClient, StorageManager downloadStorage, StorageManager downloadMetadataStorage, string runId)
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

            // fetch the regions list
            RegionsList regionsList = await obaClient.GetAllRegionsAsync();

            // check the regions list
            if (regionsList == null)
            {
                throw new Exception("regions list is null");
            }
            else if (regionsList.Regions == null)
            {
                throw new Exception("regions in regions list is null");
            }
            else if (regionsList.Regions.Count <= 0)
            {
                throw new Exception("regions in regions list is empty");
            }
            else if (string.IsNullOrWhiteSpace(regionsList.RegionsRawContent))
            {
                throw new Exception("raw content in regions list is empty");
            }

            // store it
            await downloadStorage.RegionsListStore.Insert(regionsList);

            // add a metadata entry
            await downloadMetadataStorage.DownloadMetadataStore.Insert(runId, string.Empty, string.Empty, RecordType.RegionsList, 1);

            return regionsList;
        }
    }
}
