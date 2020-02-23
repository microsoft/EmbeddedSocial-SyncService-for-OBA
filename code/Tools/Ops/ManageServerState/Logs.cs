// <copyright file="Logs.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tools.ManageServerState
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Clean and create Azure diagnostics logs
    /// </summary>
    public static class Logs
    {
        /// <summary>
        /// Provision Logs
        ///
        /// Nothing to do here.  Azure Diagnostics automatically creates the tables if needed.
        /// </summary>
        /// <returns>task</returns>
        public static async Task Create()
        {
            await Task.Delay(0);
            return;
        }

        /// <summary>
        /// Clean tables that contains Azure diagnostics logs
        /// </summary>
        /// <param name="azureDiagnosticsConnectionString">Azure diagnostics connection string</param>
        /// <returns>task</returns>
        public static async Task Clean(string azureDiagnosticsConnectionString)
        {
            Console.WriteLine("Deleting all Azure diagnostics tables...");
            CloudStorageAccount account = CloudStorageAccount.Parse(azureDiagnosticsConnectionString);

            CloudTableClient tableClient = new CloudTableClient(account.TableEndpoint, account.Credentials);

            foreach (string tableName in Program.WADTableNames)
            {
                var table = tableClient.GetTableReference(tableName);
                await table.DeleteIfExistsAsync();
                Console.WriteLine("  " + tableName + " - Table Deleted");
            }
        }
    }
}
