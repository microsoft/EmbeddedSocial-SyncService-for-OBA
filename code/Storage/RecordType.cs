// <copyright file="RecordType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage
{
    /// <summary>
    /// Type of record stored in download tables
    /// </summary>
    public enum RecordType
    {
        /// <summary>
        /// Root record defining all the regions
        /// </summary>
        RegionsList,

        /// <summary>
        /// Region information
        /// </summary>
        Region,

        /// <summary>
        /// Agency information
        /// </summary>
        Agency,

        /// <summary>
        /// Route information
        /// </summary>
        Route,

        /// <summary>
        /// Stop information
        /// </summary>
        Stop
    }
}
