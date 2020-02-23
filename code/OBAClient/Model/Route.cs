// <copyright file="Route.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBAClient.Model
{
    using System.Xml.Serialization;

    /// <summary>
    /// OneBusAway Route Class Representation from XML http://developer.onebusaway.org/modules/onebusaway-application-modules/current/api/where/elements/route.html
    /// </summary>
    [XmlRoot(ElementName = "route", IsNullable = false)]
    public class Route
    {
        private string id;

        private string shortName;

        private string longName;

        private string description;

        private string type;

        private string url;

        private string color;

        private string textColor;

        private string agencyId;

        private string rawContent;

        /// <summary>
        /// Gets or sets Id
        /// </summary>
        [XmlElement(ElementName = "id")]
        public string Id
        {
            get
            {
                return this.id;
            }

            set
            {
                this.id = value;
            }
        }

        /// <summary>
        /// Gets or sets ShortName
        /// </summary>
        [XmlElement(ElementName = "shortName")]
        public string ShortName
        {
            get
            {
                return this.shortName;
            }

            set
            {
                this.shortName = value;
            }
        }

        /// <summary>
        /// Gets or sets LongName
        /// </summary>
        [XmlElement(ElementName = "longName")]
        public string LongName
        {
            get
            {
                return this.longName;
            }

            set
            {
                this.longName = value;
            }
        }

        /// <summary>
        /// Gets or sets Description
        /// </summary>
        [XmlElement(ElementName = "description")]
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = value;
            }
        }

        /// <summary>
        /// Gets or sets Type
        /// </summary>
        [XmlElement(ElementName = "type")]
        public string Type
        {
            get
            {
                return this.type;
            }

            set
            {
                this.type = value;
            }
        }

        /// <summary>
        /// Gets or sets Url
        /// </summary>
        [XmlElement(ElementName = "url")]
        public string Url
        {
            get
            {
                return this.url;
            }

            set
            {
                this.url = value;
            }
        }

        /// <summary>
        /// Gets or sets Color
        /// </summary>
        [XmlElement(ElementName = "color")]
        public string Color
        {
            get
            {
                return this.color;
            }

            set
            {
                this.color = value;
            }
        }

        /// <summary>
        /// Gets or sets TextColor
        /// </summary>
        [XmlElement(ElementName = "textColor")]
        public string TextColor
        {
            get
            {
                return this.textColor;
            }

            set
            {
                this.textColor = value;
            }
        }

        /// <summary>
        /// Gets or sets AgencyId
        /// </summary>
        [XmlElement(ElementName = "agencyId")]
        public string AgencyId
        {
            get
            {
                return this.agencyId;
            }

            set
            {
                this.agencyId = value;
            }
        }

        /// <summary>
        /// Gets or sets Xml raw content for Route
        /// </summary>
        public string RawContent
        {
            get
            {
                return this.rawContent;
            }

            set
            {
                this.rawContent = value;
            }
        }
    }
}
