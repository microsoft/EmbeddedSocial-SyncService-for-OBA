// <copyright file="StopEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    using OBAService.Utils;

    /// <summary>
    /// OneBusAway Stop in Azure Table storage
    /// </summary>
    public class StopEntity
    {
        /// <summary>
        /// Gets or sets Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets Lat
        /// </summary>
        public decimal Lat { get; set; }

        /// <summary>
        /// Gets or sets Lon
        /// </summary>
        public decimal Lon { get; set; }

        /// <summary>
        /// Gets or sets Direction
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// Gets or sets Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets RegionId
        /// </summary>
        public string RegionId { get; set; }

        /// <summary>
        /// Gets checksum of the subset of properties that Embedded Social uses
        /// </summary>
        public int Checksum
        {
            get
            {
                return new { this.RowKey, this.Name, this.Direction }.GetHashCode();
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
