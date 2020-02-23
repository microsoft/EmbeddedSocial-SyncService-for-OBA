// <copyright file="ProcessRegions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBADownload
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OBAService.OBAClient.Model;
    using OBAService.Storage;

    /// <summary>
    /// Processes and stores OBA regions downloaded by DownloadRegionsList
    /// </summary>
    public static class ProcessRegions
    {
        /// <summary>
        /// Stores downloaded OBA regions
        /// </summary>
        /// <param name="regionsList">regions list from DownloadRegionsList class</param>
        /// <param name="downloadStorage">interface to storage for downloaded data</param>
        /// <param name="downloadMetadataStorage">interface to storage for download metadata</param>
        /// <param name="runId">uniquely identifies a run of the OBA service</param>
        /// <returns>list of regions</returns>
        public static async Task<IEnumerable<Region>> Store(RegionsList regionsList, StorageManager downloadStorage, StorageManager downloadMetadataStorage, string runId)
        {
            // check inputs
            if (downloadStorage == null)
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

            // check the regions list
            if (regionsList == null)
            {
                throw new ArgumentNullException("regionsList");
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

            // filter them
            IEnumerable<Region> regions = FilterRegionsForEmbeddedSocial(regionsList.Regions);

            // store them
            await downloadStorage.RegionStore.Insert(regions);

            // add a metadata entry
            await downloadMetadataStorage.DownloadMetadataStore.Insert(runId, string.Empty, string.Empty, RecordType.Region, regions.Count());

            return regions;
        }

        /// <summary>
        /// Removes regions that are not active or do not support Embedded Social
        /// </summary>
        /// <param name="inputRegions">full list of regions</param>
        /// <returns>list of regions that are active and support Embedded Social</returns>
        private static IEnumerable<Region> FilterRegionsForEmbeddedSocial(IEnumerable<Region> inputRegions)
        {
            List<Region> outputRegions = new List<Region>();

            foreach (Region inputRegion in inputRegions)
            {
                // include only those regions that are active and support Embedded Social
                if (inputRegion.Active && inputRegion.SupportsEmbeddedSocial)
                {
                    outputRegions.Add(inputRegion);
                }
            }

            return outputRegions;
        }
    }
}
