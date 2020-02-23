// <copyright file="RegionBound.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBAClient.Model
{
    using System.Xml.Serialization;

    /// <summary>
    /// OneBusAway region bound Class Representation
    /// </summary>
    [XmlType]
    public class RegionBound
    {
        private decimal lat;

        private decimal latSpan;

        private decimal lon;

        private decimal lonSpan;

        /// <summary>
        /// Gets or sets Lat
        /// </summary>
        [XmlElement(ElementName = "lat")]
        public decimal Lat
        {
            get
            {
                return this.lat;
            }

            set
            {
                this.lat = value;
            }
        }

        /// <summary>
        /// Gets or sets LatSpan
        /// </summary>
        [XmlElement(ElementName = "latSpan")]
        public decimal LatSpan
        {
            get
            {
                return this.latSpan;
            }

            set
            {
                this.latSpan = value;
            }
        }

        /// <summary>
        /// Gets or sets Lon
        /// </summary>
        [XmlElement(ElementName = "lon")]
        public decimal Lon
        {
            get
            {
                return this.lon;
            }

            set
            {
                this.lon = value;
            }
        }

        /// <summary>
        /// Gets or sets LonSpan
        /// </summary>
        [XmlElement(ElementName = "lonSpan")]
        public decimal LonSpan
        {
            get
            {
                return this.lonSpan;
            }

            set
            {
                this.lonSpan = value;
            }
        }
    }
}
