// <copyright file="RegionsList.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBAClient.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Class for data received from OBA. This includes Regions list and the RawContent received from the OBA URL
    /// </summary>
    public class RegionsList
    {
        /// <summary>
        /// Gets or sets Region
        /// </summary>
        public IList<Region> Regions { get; set; }

        /// <summary>
        /// Gets or sets RegionsRawContent
        /// </summary>
        public string RegionsRawContent { get; set; }

        /// <summary>
        /// Gets or sets the URI used to fetch the regions list
        /// </summary>
        public string Uri { get; set; }
    }
}
