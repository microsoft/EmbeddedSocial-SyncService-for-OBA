// <copyright file="Tables.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tools.ManageServerState
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using OBAService.Storage;

    /// <summary>
    /// Clean and create tables in Azure storage
    /// </summary>
    public static class Tables
    {
        /// <summary>
        /// Provision tables
        ///
        /// We cannot provision all the tables because many include a runid, so there is no way to know a priori
        /// what the table names will be.
        /// </summary>
        /// <param name="azureConnectionString">azure table connection string</param>
        /// <returns>task</returns>
        public static async Task Create(string azureConnectionString)
        {
            await Task.Delay(0);
            return;
        }

        /// <summary>
        /// Clean tables
        ///
        /// Delete all the existing tables in the given storage account
        /// </summary>
        /// <param name="azureConnectionString">azure table connection string</param>
        /// <returns>task</returns>
        public static async Task Clean(string azureConnectionString)
        {
            Console.WriteLine("Deleting all tables in Azure Table Store...");
            CloudStorageAccount account = CloudStorageAccount.Parse(azureConnectionString);

            CloudTableClient tableClient = new CloudTableClient(account.TableEndpoint, account.Credentials);

            foreach (var result in tableClient.ListTables())
            {
                // filter out Azure diagnostics tables
                if (IsWADTable(result.Name))
                {
                    continue;
                }

                // if the table name does not have a prefix match on OBA table names,
                // then we should not delete it
                if (!IsOBATable(result.Name))
                {
                    continue;
                }

                await result.DeleteIfExistsAsync();
                Console.WriteLine("  " + result.Name + " - Table Deleted");
            }
        }

        /// <summary>
        /// check to see if this table name matches one of the Azure Diagnostics table names
        /// </summary>
        /// <param name="name">name to check</param>
        /// <returns>true if the name matches</returns>
        private static bool IsWADTable(string name)
        {
            foreach (string wadName in Program.WADTableNames)
            {
                if (name == wadName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// check for prefix match on the table name with each of the OBA tables defined in OBAService.Storage
        /// </summary>
        /// <param name="name">name to check</param>
        /// <returns>true if the name starts with any of the oba table names</returns>
        private static bool IsOBATable(string name)
        {
            foreach (var obaTableName in Enum.GetValues(typeof(TableNames.TableType)))
            {
                if (name.StartsWith(obaTableName.ToString()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
