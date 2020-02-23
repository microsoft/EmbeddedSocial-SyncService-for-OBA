// <copyright file="RegionsListEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    /// <summary>
    /// OneBusAway information that defines all the regions in Azure Table storage
    /// </summary>
    public class RegionsListEntity
    {
        /// <summary>
        /// Gets or sets RegionsServiceUri
        /// </summary>
        public string RegionsServiceUri { get; set; }

        /// <summary>
        /// Gets or sets RecordType
        /// </summary>
        public string RecordType { get; set; }

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
                return "RegionsList";
            }
        }

        /// <summary>
        /// Gets RowKey
        /// </summary>
        internal string RowKey
        {
            get
            {
                return "RegionsList";
            }
        }
    }
}
