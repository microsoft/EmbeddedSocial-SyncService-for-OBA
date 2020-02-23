// <copyright file="AgencyEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    using OBAService.Utils;

    /// <summary>
    /// OneBusAway Agency in Azure Table storage
    /// </summary>
    public class AgencyEntity
    {
        /// <summary>
        /// Gets or sets Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets Phone
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets RegionId
        /// </summary>
        public string RegionId { get; set; }

        /// <summary>
        /// Gets checksum of a subset of properties that we care about to determine if a record has changed substantially
        /// </summary>
        public int Checksum
        {
            get
            {
                return new { this.Name, this.Phone, this.Url }.GetHashCode();
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
                return this.RegionId;
            }
        }

        /// <summary>
        /// Gets RowKey
        /// </summary>
        internal string RowKey
        {
            get
            {
                return (this.RecordType + "_" + this.RegionId + "_" + this.Id).StringToTableKey();
            }
        }
    }
}
