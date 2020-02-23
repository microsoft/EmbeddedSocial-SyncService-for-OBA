// <copyright file="TableNames.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage
{
    using System;

    /// <summary>
    /// Names of tables in Azure Storage
    /// </summary>
    public static class TableNames
    {
        /// <summary>
        /// Which type of table
        /// </summary>
        public enum TableType
        {
            /// <summary>
            /// Download table stores data received from OBA servers.
            /// </summary>
            Download,

            /// <summary>
            /// DownloadMetada table stores bookkeeping records of download activity.
            /// </summary>
            DownloadMetadata,

            /// <summary>
            /// Diff table stores diffs between a download table and a publish table.
            /// </summary>
            Diff,

            /// <summary>
            /// DiffMetadata table stores bookkeeping records of diff activity.
            /// </summary>
            DiffMetadata,

            /// <summary>
            /// Publish table stores data that has been published to Embedded Social.
            /// </summary>
            Publish,

            /// <summary>
            /// PublishMetadata table stores bookkeeping records of publish activity.
            /// </summary>
            PublishMetadata,

            /// <summary>
            /// Metadata table stores bookkeeping records of overall activity in this service.
            /// </summary>
            Metadata
        }

        /// <summary>
        /// Constructs the name of a table
        /// </summary>
        /// <param name="tableType">type of table</param>
        /// <param name="runId">uniquely identifies a run</param>
        /// <returns>the name of a table</returns>
        public static string TableName(TableNames.TableType tableType, string runId)
        {
            switch (tableType)
            {
                // Download table stores data received from OBA servers.
                case TableNames.TableType.Download:
                    return tableType.ToString() + runId;

                // DownloadMetada table stores bookkeeping records of download activity.
                case TableNames.TableType.DownloadMetadata:
                    return tableType.ToString();

                // Diff table stores diffs between a download table and a publish table.
                case TableNames.TableType.Diff:
                    return tableType.ToString() + runId;

                // DiffMetadata table stores bookkeeping records of diff activity.
                case TableNames.TableType.DiffMetadata:
                    return tableType.ToString();

                // Publish table stores data that has been published to Embedded Social.
                case TableNames.TableType.Publish:
                    return tableType.ToString();

                // PublishMetadata table stores bookkeeping records of publish activity.
                case TableNames.TableType.PublishMetadata:
                    return tableType.ToString();

                // Metadata table stores bookkeeping records of overall activity in this service.
                case TableNames.TableType.Metadata:
                    return tableType.ToString();

                default:
                    throw new ArgumentOutOfRangeException("tableType");
            }
        }
    }
}
