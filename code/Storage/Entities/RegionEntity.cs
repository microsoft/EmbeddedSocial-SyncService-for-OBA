// <copyright file="RegionEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    using OBAService.Utils;

    /// <summary>
    /// OneBusAway Region in Azure Table storage
    /// </summary>
    public class RegionEntity
    {
        /// <summary>
        /// Gets or sets Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets RegionName
        /// </summary>
        public string RegionName { get; set; }

        /// <summary>
        /// Gets or sets ObaBaseUrl
        /// </summary>
        public string ObaBaseUrl { get; set; }

        /// <summary>
        /// Gets checksum of a subset of properties that we care about to determine if a record has changed substantially
        /// </summary>
        public int Checksum
        {
            get
            {
                return new { this.RegionName, this.ObaBaseUrl }.GetHashCode();
            }
        }

        /// <summary>
        /// Gets or sets RecordType
        /// </summary>
        public string RecordType { get; set; }

        /// <summary>
        /// Gets or sets RowState
        /// </summary>
        public string RowState { get; set; }

        /// <summary>
        /// Gets or sets RawContent
        /// </summary>
        public string RawContent { get; set; }

        /// <summary>
        /// Gets PartitionKey
        /// </summary>
        internal string PartitionKey
        {
            get
            {
                return this.Id;
            }
        }

        /// <summary>
        /// Gets RowKey
        /// </summary>
        internal string RowKey
        {
            get
            {
                return (this.RecordType + "_" + this.Id).StringToTableKey();
            }
        }
    }
}
