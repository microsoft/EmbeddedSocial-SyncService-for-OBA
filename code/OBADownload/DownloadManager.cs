// <copyright file="DownloadManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBADownload
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OBAService.OBAClient;
    using OBAService.OBAClient.Model;
    using OBAService.Storage;

    /// <summary>
    /// Fetches OBA data using the OBAClient class and stores it using the Storage class
    /// </summary>
    public class DownloadManager
    {
        /// <summary>
        /// Interface to the storage layer for OBA data
        /// </summary>
        private StorageManager downloadStorage;

        /// <summary>
        /// Interface to the storage layer for download metadata
        /// </summary>
        private StorageManager downloadMetadataStorage;

        /// <summary>
        /// Interface to the OBA servers
        /// </summary>
        private Client obaClient;

        /// <summary>
        /// Uniquely identifies a unique run of the OBA service
        /// </summary>
        private string runId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadManager"/> class.
        /// You should call InitializeStorage after this method.
        /// </summary>
        /// <param name="azureStorageConnectionString">Azure storage connection string</param>
        /// <param name="runId">Uniquely identifies an execution of the OBA service</param>
        /// <param name="obaApiKey">OBA API key</param>
        /// <param name="obaRegionsListUri">OBA regions list URI</param>
        public DownloadManager(string azureStorageConnectionString, string runId, string obaApiKey, string obaRegionsListUri)
        {
            this.runId = runId;

            // create storage managers
            this.downloadStorage = new StorageManager(azureStorageConnectionString, TableNames.TableType.Download, runId);
            this.downloadMetadataStorage = new StorageManager(azureStorageConnectionString, TableNames.TableType.DownloadMetadata, runId);

            // create OBA client
            this.obaClient = new Client(obaApiKey, obaRegionsListUri);
        }

        /// <summary>
        /// Creates tables needed to store OBA data
        /// </summary>
        /// <returns>task that creates tables</returns>
        public async Task InitializeStorage()
        {
            await this.downloadStorage.CreateTables();
            await this.downloadMetadataStorage.CreateTables();
        }

        /// <summary>
        /// Downloads all OBA data and stores it
        /// </summary>
        /// <returns>task that fetches all OBA data and stores it</returns>
        public async Task DownloadAndStore()
        {
            RegionsList regionsList = await DownloadRegionsList.DownloadAndStore(this.obaClient, this.downloadStorage, this.downloadMetadataStorage, this.runId);
            IEnumerable<Region> regions = await ProcessRegions.Store(regionsList, this.downloadStorage, this.downloadMetadataStorage, this.runId);
            IEnumerable<Tuple<Region, Agency>> agencies = await DownloadAgencies.DownloadAndStore(regions, this.obaClient, this.downloadStorage, this.downloadMetadataStorage, this.runId);
            IEnumerable<Tuple<Region, Agency, IEnumerable<Route>>> routes = await DownloadRoutes.DownloadAndStore(agencies, this.obaClient, this.downloadStorage, this.downloadMetadataStorage, this.runId);
            await DownloadStops.DownloadAndStore(routes, this.obaClient, this.downloadStorage, this.downloadMetadataStorage, this.runId);
        }

        /// <summary>
        /// Deletes all data downloaded with this runId
        /// </summary>
        /// <returns>task that deletes the downloaded data</returns>
        public async Task DeleteDownload()
        {
            await this.downloadStorage.DeleteDataTables();
            await this.downloadMetadataStorage.DownloadMetadataStore.Delete(this.runId);
        }
    }
}
