// <copyright file="ExtensionMethods.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Helpful extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Transforms a string so that it is acceptable as an Azure Table row key or partition key.
        /// </summary>
        /// <param name="rowKeyString">string</param>
        /// <returns>string appropriate for use as an Azure Table key</returns>
        public static string StringToTableKey(this string rowKeyString)
        {
            // check string is not null
            if (string.IsNullOrEmpty(rowKeyString))
            {
                throw new ArgumentNullException("rowKeyString");
            }

            // Replace characters that are not allowed (see https://msdn.microsoft.com/library/azure/dd179338.aspx ).
            // Note that the list on MSDN is incomplete. For example, + is not allowed
            Regex disallowedCharsInTableKeys = new Regex(@"[\\#%+/?\u0000-\u001F\u007F-\u009F]");
            StringBuilder safeRowKeyString = new StringBuilder();
            foreach (char c in rowKeyString)
            {
                // replace only the disallowed characters with encoded versions
                if (disallowedCharsInTableKeys.IsMatch(c.ToString()))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(new char[] { c });
                    string convertedString = Convert.ToBase64String(bytes);

                    // base64 strings can have '/' which is not allowed in Azure Table keys
                    convertedString = convertedString.Replace('/', '_');
                    safeRowKeyString.Append(convertedString);
                }
                else
                {
                    // preserve some human readability by keeping safe characters as is
                    safeRowKeyString.Append(c);
                }
            }

            // key can be up to 1KB in size
            if (safeRowKeyString.Length > 1024)
            {
                throw new ArgumentException("Azure Table keys can be up to 1 KB in length", "rowKeyString");
            }

            // key has to be at least 1 char long
            if (safeRowKeyString.Length < 1)
            {
                throw new ArgumentException("Azure Table keys have to be at least 1 char long", "rowKeyString");
            }

            return safeRowKeyString.ToString();
        }

        /// <summary>
        /// Execute a batch operation on a CloudTable in multiples of 100
        /// </summary>
        /// <param name="cloudTable">Cloud table to execute operations on</param>
        /// <param name="batch">Operations to issue to the cloud table</param>
        /// <returns>Task</returns>
        public static async Task ExecuteBatchInChunkAsync(this CloudTable cloudTable, TableBatchOperation batch)
        {
            List<Task> tasks = new List<Task>();
            var batchOperation = new TableBatchOperation();
            foreach (var item in batch)
            {
                batchOperation.Add(item);
                if (batchOperation.Count == 100)
                {
                    tasks.Add(cloudTable.ExecuteBatchAsync(batchOperation));
                    batchOperation = new TableBatchOperation();
                }
            }

            if (batchOperation.Count > 0)
            {
                tasks.Add(cloudTable.ExecuteBatchAsync(batchOperation));
            }

            await Task.WhenAll(tasks.ToArray());
        }
    }
}
