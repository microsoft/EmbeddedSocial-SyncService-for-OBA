// <copyright file="PublishManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.PublishToEmbeddedSocial
{
    using System;
    using System.Threading.Tasks;

    using OBAService.Storage;

    /// <summary>
    /// Publishes changes sitting in the diff table to the Embedded Social service
    /// </summary>
    public class PublishManager
    {
        /// <summary>
        /// Interface to the storage layer for diff data
        /// </summary>
        private StorageManager diffStorage;

        /// <summary>
        /// Interface to the storage layer for diff metadata
        /// </summary>
        private StorageManager diffMetadataStorage;

        /// <summary>
        /// Interface to the storage layer for publish data
        /// </summary>
        private StorageManager publishStorage;

        /// <summary>
        /// Interface to the storage layer for publish metadata
        /// </summary>
        private StorageManager publishMetadataStorage;

        /// <summary>
        /// Uniquely identifies a unique run of the OBA service
        /// </summary>
        private string runId;

        /// <summary>
        /// Interface for publishing to Embedded Social
        /// </summary>
        private EmbeddedSocial embeddedSocial;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishManager"/> class.
        /// You should call InitializeStorage after this method.
        /// </summary>
        /// <param name="azureStorageConnectionString">Azure storage connection string</param>
        /// <param name="runId">Uniquely identifies an execution of the OBA service</param>
        /// <param name="embeddedSocialUri">URI to the Embedded Social service</param>
        /// <param name="appKey">Embedded Social application key</param>
        /// <param name="aadToken">AAD token for signing into Embedded Social</param>
        /// <param name="userHandle">Administrative user handle that will publish topics</param>
        public PublishManager(string azureStorageConnectionString, string runId, Uri embeddedSocialUri, string appKey, string aadToken, string userHandle)
        {
            this.runId = runId;

            // create storage managers
            this.diffStorage = new StorageManager(azureStorageConnectionString, TableNames.TableType.Diff, runId);
            this.diffMetadataStorage = new StorageManager(azureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
            this.publishStorage = new StorageManager(azureStorageConnectionString, TableNames.TableType.Publish, runId);
            this.publishMetadataStorage = new StorageManager(azureStorageConnectionString, TableNames.TableType.PublishMetadata, runId);

            // create Embedded Social publish interface
            this.embeddedSocial = new EmbeddedSocial(embeddedSocialUri, appKey, aadToken, userHandle);
        }

        /// <summary>
        /// Creates tables needed to publish to Embedded Social
        /// </summary>
        /// <returns>task that creates tables</returns>
        public async Task InitializeStorage()
        {
            // shouldn't need to create the diff & diff metadata tables; if they don't exist, then that's a bug
            await this.publishStorage.CreateTables();
            await this.publishMetadataStorage.CreateTables();
        }

        /// <summary>
        /// Publishes all changes in the diff table and store that in the publish table
        /// </summary>
        /// <returns>task that publishes all changes in the diff table</returns>
        public async Task PublishAndStore()
        {
            await PublishRoutes.PublishRoutesToEmbeddedSocial(this.diffStorage, this.diffMetadataStorage, this.publishStorage, this.publishMetadataStorage, this.embeddedSocial, this.runId);
            await PublishStops.PublishStopsToEmbeddedSocial(this.diffStorage, this.diffMetadataStorage, this.publishStorage, this.publishMetadataStorage, this.embeddedSocial, this.runId);
        }
    }
}
