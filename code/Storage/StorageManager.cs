// <copyright file="StorageManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Manages the various stores.
    /// </summary>
    public class StorageManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageManager"/> class.
        /// </summary>
        /// <param name="connectionString">Azure storage connection string</param>
        /// <param name="tableType">The type of tables to interact with</param>
        /// <param name="runId">Uniquely identifies an execution of the OBA service</param>
        public StorageManager(string connectionString, TableNames.TableType tableType, string runId)
        {
            // setup access to Azure Tables
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();

            // instantiate every one of the stores
            switch (tableType)
            {
                // Download table stores data received from OBA servers.
                case TableNames.TableType.Download:
                // Diff table stores diffs between a download table and a publish table.
                case TableNames.TableType.Diff:
                // Publish table stores data that has been published to Embedded Social.
                case TableNames.TableType.Publish:
                    this.RegionsListStore = new RegionsListStore(tableClient, tableType, runId);
                    this.RegionStore = new RegionStore(tableClient, tableType, runId);
                    this.AgencyStore = new AgencyStore(tableClient, tableType, runId);
                    this.RouteStore = new RouteStore(tableClient, tableType, runId);
                    this.StopStore = new StopStore(tableClient, tableType, runId);
                    break;

                // DownloadMetada table stores bookkeeping records of download activity.
                case TableNames.TableType.DownloadMetadata:
                    this.DownloadMetadataStore = new DownloadMetadataStore(tableClient, tableType, runId);
                    break;

                // DiffMetadata table stores bookkeeping records of diff activity.
                case TableNames.TableType.DiffMetadata:
                    this.DiffMetadataStore = new DiffMetadataStore(tableClient, tableType, runId);
                    break;

                // PublishMetadata table stores bookkeeping records of publish activity.
                case TableNames.TableType.PublishMetadata:
                    this.PublishMetadataStore = new PublishMetadataStore(tableClient, tableType, runId);
                    break;

                // Metadata table stores bookkeeping records of overall activity in this service.
                case TableNames.TableType.Metadata:
                    // not implemented yet
                    throw new ArgumentOutOfRangeException("tableType");

                default:
                    throw new ArgumentOutOfRangeException("tableType");
            }
        }

        /// <summary>
        /// Gets the download metadata store
        /// </summary>
        public DownloadMetadataStore DownloadMetadataStore { get; private set; }

        /// <summary>
        /// Gets the diff metadata store
        /// </summary>
        public DiffMetadataStore DiffMetadataStore { get; private set; }

        /// <summary>
        /// Gets the publish metadata store
        /// </summary>
        public PublishMetadataStore PublishMetadataStore { get; private set; }

        /// <summary>
        /// Gets the regions list store
        /// </summary>
        public RegionsListStore RegionsListStore { get; private set; }

        /// <summary>
        /// Gets the region store
        /// </summary>
        public RegionStore RegionStore { get; private set; }

        /// <summary>
        /// Gets the agency store
        /// </summary>
        public AgencyStore AgencyStore { get; private set; }

        /// <summary>
        /// Gets the route store
        /// </summary>
        public RouteStore RouteStore { get; private set; }

        /// <summary>
        /// Gets the stop store
        /// </summary>
        public StopStore StopStore { get; private set; }

        /// <summary>
        /// Creates all the tables
        /// </summary>
        /// <returns>task that creates all the tables</returns>
        public async Task CreateTables()
        {
            List<Task> tasks = new List<Task>();

            if (this.RegionsListStore != null)
            {
                tasks.Add(this.RegionsListStore.CreateTable());
            }

            if (this.RegionStore != null)
            {
                tasks.Add(this.RegionStore.CreateTable());
            }

            if (this.AgencyStore != null)
            {
                tasks.Add(this.AgencyStore.CreateTable());
            }

            if (this.RouteStore != null)
            {
                tasks.Add(this.RouteStore.CreateTable());
            }

            if (this.StopStore != null)
            {
                tasks.Add(this.StopStore.CreateTable());
            }

            if (this.DownloadMetadataStore != null)
            {
                tasks.Add(this.DownloadMetadataStore.CreateTable());
            }

            if (this.DiffMetadataStore != null)
            {
                tasks.Add(this.DiffMetadataStore.CreateTable());
            }

            if (this.PublishMetadataStore != null)
            {
                tasks.Add(this.PublishMetadataStore.CreateTable());
            }

            await Task.WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// Deletes all the tables
        /// </summary>
        /// <returns>task that deletes all the tables</returns>
        public async Task DeleteTables()
        {
            List<Task> tasks = new List<Task>();

            if (this.DownloadMetadataStore != null)
            {
                tasks.Add(this.DownloadMetadataStore.DeleteTable());
            }

            if (this.DiffMetadataStore != null)
            {
                tasks.Add(this.DiffMetadataStore.DeleteTable());
            }

            if (this.PublishMetadataStore != null)
            {
                tasks.Add(this.PublishMetadataStore.DeleteTable());
            }

            tasks.Add(this.DeleteDataTables());
            await Task.WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// Deletes all the non-metadata tables
        /// </summary>
        /// <returns>task that deletes all the download tables</returns>
        public async Task DeleteDataTables()
        {
            List<Task> tasks = new List<Task>();

            if (this.RegionsListStore != null)
            {
                tasks.Add(this.RegionsListStore.DeleteTable());
            }

            if (this.RegionStore != null)
            {
                tasks.Add(this.RegionStore.DeleteTable());
            }

            if (this.AgencyStore != null)
            {
                tasks.Add(this.AgencyStore.DeleteTable());
            }

            if (this.RouteStore != null)
            {
                tasks.Add(this.RouteStore.DeleteTable());
            }

            if (this.StopStore != null)
            {
                tasks.Add(this.StopStore.DeleteTable());
            }

            await Task.WhenAll(tasks.ToArray());
        }
    }
}