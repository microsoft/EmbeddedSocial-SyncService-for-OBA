// <copyright file="Region.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBAClient.Model
{
    using System.Xml.Serialization;

    /// <summary>
    /// OneBusAway Region Class Representation from XML http://regions.onebusaway.org/regions-v3.xml
    /// </summary>
    [XmlRoot(ElementName = "region", IsNullable = false)]
    public class Region
    {
        private string siriBaseUrl;

        private string obaVersionInfo;

        private bool supportsSiriRealtimeApis;

        private bool supportsObaDiscoveryApis;

        private string language;

        private string twitterUrl;

        private bool supportsObaRealtimeApis;

        private RegionBound[] bounds;

        private string otpBaseUrl;

        private string contactEmail;

        private string otpContactEmail;

        private string stopInfoUrl;

        private RegionOpen311Server[] open311Servers;

        private bool active;

        private bool supportsEmbeddedSocial;

        private string facebookUrl;

        private string obaBaseUrl;

        private string id;

        private bool experimental;

        private string regionName;

        private string rawContent;

        /// <summary>
        /// Gets or sets SiriBaseUrl
        /// </summary>
        [XmlElement(ElementName = "siriBaseUrl")]
        public string SiriBaseUrl
        {
            get
            {
                return this.siriBaseUrl;
            }

            set
            {
                this.siriBaseUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets ObaVersionInfo
        /// </summary>
        [XmlElement(ElementName = "obaVersionInfo")]
        public string ObaVersionInfo
        {
            get
            {
                return this.obaVersionInfo;
            }

            set
            {
                this.obaVersionInfo = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether SupportsSiriRealtimeApis is set or not
        /// </summary>
        [XmlElement(ElementName = "supportsSiriRealtimeApis")]
        public bool SupportsSiriRealtimeApis
        {
            get
            {
                return this.supportsSiriRealtimeApis;
            }

            set
            {
                this.supportsSiriRealtimeApis = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether SupportsObaDiscoveryApis is set or not
        /// </summary>
        [XmlElement(ElementName = "supportsObaDiscoveryApis")]
        public bool SupportsObaDiscoveryApis
        {
            get
            {
                return this.supportsObaDiscoveryApis;
            }

            set
            {
                this.supportsObaDiscoveryApis = value;
            }
        }

        /// <summary>
        /// Gets or sets Language
        /// </summary>
        [XmlElement(ElementName = "language")]
        public string Language
        {
            get
            {
                return this.language;
            }

            set
            {
                this.language = value;
            }
        }

        /// <summary>
        /// Gets or sets TwitterUrl
        /// </summary>
        [XmlElement(ElementName = "twitterUrl")]
        public string TwitterUrl
        {
            get
            {
                return this.twitterUrl;
            }

            set
            {
                this.twitterUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether supportsObaRealtimeApis is set or not
        /// </summary>
        [XmlElement(ElementName = "supportsObaRealtimeApis")]
        public bool SupportsObaRealtimeApis
        {
            get
            {
                return this.supportsObaRealtimeApis;
            }

            set
            {
                this.supportsObaRealtimeApis = value;
            }
        }

        /// <summary>
        /// Gets or sets bounds
        /// </summary>
        [XmlArray("bounds")]
        [XmlArrayItem("bound", IsNullable = false)]
        public RegionBound[] Bounds
        {
            get
            {
                return this.bounds;
            }

            set
            {
                this.bounds = value;
            }
        }

        /// <summary>
        /// Gets or sets OtpBaseUrl
        /// </summary>
        [XmlElement(ElementName = "otpBaseUrl")]
        public string OtpBaseUrl
        {
            get
            {
                return this.otpBaseUrl;
            }

            set
            {
                this.otpBaseUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets ContactEmail
        /// </summary>
        [XmlElement(ElementName = "contactEmail")]
        public string ContactEmail
        {
            get
            {
                return this.contactEmail;
            }

            set
            {
                this.contactEmail = value;
            }
        }

        /// <summary>
        /// Gets or sets OtpContactEmail
        /// </summary>
        [XmlElement(ElementName = "otpContactEmail")]
        public string OtpContactEmail
        {
            get
            {
                return this.otpContactEmail;
            }

            set
            {
                this.otpContactEmail = value;
            }
        }

        /// <summary>
        /// Gets or sets StopInfoUrl
        /// </summary>
        [XmlElement(ElementName = "stopInfoUrl")]
        public string StopInfoUrl
        {
            get
            {
                return this.stopInfoUrl;
            }

            set
            {
                this.stopInfoUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets open311Servers
        /// </summary>
        [XmlArray("open311Servers")]
        [XmlArrayItem("open311Server", IsNullable = false)]
        public RegionOpen311Server[] Open311Servers
        {
            get
            {
                return this.open311Servers;
            }

            set
            {
                this.open311Servers = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Active is set or not
        /// </summary>
        [XmlElement(ElementName = "active")]
        public bool Active
        {
            get
            {
                return this.active;
            }

            set
            {
                this.active = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether supportsEmbeddedSocial is set or not
        /// </summary>
        [XmlElement(ElementName = "supportsEmbeddedSocial")]
        public bool SupportsEmbeddedSocial
        {
            get
            {
                return this.supportsEmbeddedSocial;
            }

            set
            {
                this.supportsEmbeddedSocial = value;
            }
        }

        /// <summary>
        /// Gets or sets FacebookUrl
        /// </summary>
        [XmlElement(ElementName = "facebookUrl")]
        public string FacebookUrl
        {
            get
            {
                return this.facebookUrl;
            }

            set
            {
                this.facebookUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets ObaBaseUrl
        /// </summary>
        [XmlElement(ElementName = "obaBaseUrl")]
        public string ObaBaseUrl
        {
            get
            {
                return this.obaBaseUrl;
            }

            set
            {
                this.obaBaseUrl = value;
            }
        }

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
        /// Gets or sets a value indicating whether Experimental is set or not
        /// </summary>
        [XmlElement(ElementName = "experimental")]
        public bool Experimental
        {
            get
            {
                return this.experimental;
            }

            set
            {
                this.experimental = value;
            }
        }

        /// <summary>
        /// Gets or sets RegionName
        /// </summary>
        [XmlElement(ElementName = "regionName")]
        public string RegionName
        {
            get
            {
                return this.regionName;
            }

            set
            {
                this.regionName = value;
            }
        }

        /// <summary>
        /// Gets or sets Xml raw content for Region
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
