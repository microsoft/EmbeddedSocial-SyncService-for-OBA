// <copyright file="Agency.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBAClient.Model
{
    using System.Xml.Serialization;

    /// <summary>
    /// OneBusAway Agency Class Representation from XML http://developer.onebusaway.org/modules/onebusaway-application-modules/current/api/where/elements/agency.html
    /// </summary>
    [XmlRoot(ElementName = "agency", IsNullable = false)]
    public class Agency
    {
        private string id;

        private string name;

        private string url;

        private string timezone;

        private string lang;

        private string phone;

        private string disclaimer;

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
        /// Gets or sets Name
        /// </summary>
        [XmlElement(ElementName = "name")]
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
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
        /// Gets or sets Timezone
        /// </summary>
        [XmlElement(ElementName = "timezone")]
        public string Timezone
        {
            get
            {
                return this.timezone;
            }

            set
            {
                this.timezone = value;
            }
        }

        /// <summary>
        /// Gets or sets Lang
        /// </summary>
        [XmlElement(ElementName = "lang")]
        public string Lang
        {
            get
            {
                return this.lang;
            }

            set
            {
                this.lang = value;
            }
        }

        /// <summary>
        /// Gets or sets Phone
        /// </summary>
        [XmlElement(ElementName = "phone")]
        public string Phone
        {
            get
            {
                return this.phone;
            }

            set
            {
                this.phone = value;
            }
        }

        /// <summary>
        /// Gets or sets Disclaimer
        /// </summary>
        [XmlElement(ElementName = "disclaimer")]
        public string Disclaimer
        {
            get
            {
                return this.disclaimer;
            }

            set
            {
                this.disclaimer = value;
            }
        }

        /// <summary>
        /// Gets or sets Xml raw content for Agency
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
