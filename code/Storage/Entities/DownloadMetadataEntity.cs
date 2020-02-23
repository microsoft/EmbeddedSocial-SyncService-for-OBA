// <copyright file="DownloadMetadataEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    using OBAService.Utils;

    /// <summary>
    /// Download Metadata row in Azure Table storage
    /// </summary>
    public class DownloadMetadataEntity
    {
        /// <summary>
        /// Gets or sets RunId
        /// </summary>
        public string RunId { get; set; }

        /// <summary>
        /// Gets or sets RegionId
        /// </summary>
        public string RegionId { get; set; }

        /// <summary>
        /// Gets or sets AgencyId
        /// </summary>
        public string AgencyId { get; set; }

        /// <summary>
        /// Gets or sets RecordType
        /// </summary>
        public string RecordType { get; set; }

        /// <summary>
        /// Gets or sets count of total processed records
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets PartitionKey
        /// </summary>
        internal string PartitionKey
        {
            get
            {
                return this.RunId;
            }
        }

        /// <summary>
        /// Gets RowKey
        /// </summary>
        internal string RowKey
        {
            get
            {
                return (this.RecordType + "_" + this.RegionId + "_" + this.AgencyId).StringToTableKey();
            }
        }
    }
}
