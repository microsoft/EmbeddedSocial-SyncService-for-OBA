// <copyright file="DiffManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Diff
{
    using System.Threading.Tasks;

    using OBAService.Storage;

    /// <summary>
    /// Compares OBA download data and publish data, computes a diff, and stores it using the Storage class
    /// </summary>
    public class DiffManager
    {
        /// <summary>
        /// Interface to the storage layer for OBA download data
        /// </summary>
        private StorageManager downloadStorage;

        /// <summary>
        /// Interface to the storage layer for OBA download data
        /// </summary>
        private StorageManager publishStorage;

        /// <summary>
        /// Interface to the storage layer for OBA diff data
        /// </summary>
        private StorageManager diffStorage;

        /// <summary>
        /// Interface to the storage layer for diff metadata
        /// </summary>
        private StorageManager diffMetadataStorage;

        /// <summary>
        /// Uniquely identifies a unique run of the OBA service
        /// </summary>
        private string runId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffManager"/> class.
        /// You should call InitializeStorage after this method.
        /// </summary>
        /// <param name="azureStorageConnectionString">Azure storage connection string</param>
        /// <param name="runId">Uniquely identifies an execution of the OBA service</param>
        public DiffManager(string azureStorageConnectionString, string runId)
        {
            this.runId = runId;

            // create storage managers
            this.downloadStorage = new StorageManager(azureStorageConnectionString, TableNames.TableType.Download, runId);
            this.publishStorage = new StorageManager(azureStorageConnectionString, TableNames.TableType.Publish, runId);
            this.diffStorage = new StorageManager(azureStorageConnectionString, TableNames.TableType.Diff, runId);
            this.diffMetadataStorage = new StorageManager(azureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
        }

        /// <summary>
        /// Creates tables needed to store diff data
        /// </summary>
        /// <returns>task that creates tables</returns>
        public async Task InitializeStorage()
        {
            // shouldn't need to create the download table; if it doesn't exist, then that's a bug
            await this.publishStorage.CreateTables();
            await this.diffStorage.CreateTables();
            await this.diffMetadataStorage.CreateTables();
        }

        /// <summary>
        /// Diffs all OBA data and stores it
        /// </summary>
        /// <returns>task that diffs OBA data and stores it</returns>
        public async Task DiffAndStore()
        {
            await DiffRoutes.DiffAndStore(this.downloadStorage, this.publishStorage, this.diffStorage, this.diffMetadataStorage, this.runId);
            await DiffStops.DiffAndStore(this.downloadStorage, this.publishStorage, this.diffStorage, this.diffMetadataStorage, this.runId);
        }

        /// <summary>
        /// Deletes all data diffed with this runId
        /// </summary>
        /// <returns>task that deletes the diffed data</returns>
        public async Task DeleteDiff()
        {
            await this.diffStorage.DeleteDataTables();
            await this.diffMetadataStorage.DiffMetadataStore.Delete(this.runId);
        }
    }
}
