// <copyright file="RouteEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    using OBAService.Utils;

    /// <summary>
    /// OneBusAway Route in Azure Table storage
    /// </summary>
    public class RouteEntity
    {
        /// <summary>
        /// Gets or sets Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets ShortName
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Gets or sets LongName
        /// </summary>
        public string LongName { get; set; }

        /// <summary>
        /// Gets or sets Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets AgencyId
        /// </summary>
        public string AgencyId { get; set; }

        /// <summary>
        /// Gets or sets RegionId
        /// </summary>
        public string RegionId { get; set; }

        /// <summary>
        /// Gets a checksum of the subset of properties that Embedded Social uses
        /// </summary>
        public int Checksum
        {
            get
            {
                return new { this.RowKey, this.ShortName, this.LongName }.GetHashCode();
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
