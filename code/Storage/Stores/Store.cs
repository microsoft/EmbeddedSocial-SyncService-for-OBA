// <copyright file="Store.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage
{
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Stores and retrieves records to/from Azure Table storage.
    /// </summary>
    public abstract class Store
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Store"/> class.
        /// </summary>
        /// <param name="cloudTableClient">client to Azure Table</param>
        /// <param name="tableName">the name of the table</param>
        public Store(CloudTableClient cloudTableClient, string tableName)
        {
            this.Table = cloudTableClient.GetTableReference(tableName);
        }

        /// <summary>
        /// Gets interface to the Azure Storage table
        /// </summary>
        protected CloudTable Table { get; private set; }

        /// <summary>
        /// Create the table if it doesn't exist
        /// </summary>
        /// <returns>task that creates the table</returns>
        public async Task CreateTable()
        {
            await this.Table.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Delete the table if it exists
        /// </summary>
        /// <returns>task that deletes the table</returns>
        public async Task DeleteTable()
        {
            await this.Table.DeleteIfExistsAsync();
        }
    }
}
