// <copyright file="RegionOpen311Server.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBAClient.Model
{
    using System.Xml.Serialization;

    /// <summary>
    /// OneBusAway region open311Server Class Representation
    /// </summary>
    [XmlType]
    public class RegionOpen311Server
    {
        private string juridisctionId;

        private string apiKey;

        private string baseUrl;

        /// <summary>
        /// Gets or sets JuridisctionId
        /// </summary>
        [XmlElement(ElementName = "juridisctionId")]
        public string JuridisctionId
        {
            get
            {
                return this.juridisctionId;
            }

            set
            {
                this.juridisctionId = value;
            }
        }

        /// <summary>
        /// Gets or sets ApiKey
        /// </summary>
        [XmlElement(ElementName = "apiKey")]
        public string ApiKey
        {
            get
            {
                return this.apiKey;
            }

            set
            {
                this.apiKey = value;
            }
        }

        /// <summary>
        /// Gets or sets BaseUrl
        /// </summary>
        [XmlElement(ElementName = "baseUrl")]
        public string BaseUrl
        {
            get
            {
                return this.baseUrl;
            }

            set
            {
                this.baseUrl = value;
            }
        }
    }
}
