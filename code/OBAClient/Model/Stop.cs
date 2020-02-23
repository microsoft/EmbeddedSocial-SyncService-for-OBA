/* Copyright 2014 Michael Braude and individual contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace OBAService.OBAClient.Model
{
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// OneBusAway Stop Class Representation from XML http://developer.onebusaway.org/modules/onebusaway-application-modules/current/api/where/elements/stop.html
    /// </summary>
    [XmlRoot(ElementName = "entry", IsNullable = false)]
    public class Stop
    {
        private string id;

        private decimal lat;

        private decimal lon;

        private string direction;

        private string name;

        private string code;

        private int locationType;

        private string wheelchairBoarding;

        private string[] routeIds;

        private string rawContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stop"/> class.
        /// </summary>
        /// <param name="stopElement">XElement to use to construct a stop class</param>
        public Stop(XElement stopElement)
        {
            this.ParseStopElement(stopElement);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stop"/> class.
        /// </summary>
        protected Stop()
        {
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
        /// Gets or sets Direction
        /// </summary>
        [XmlElement(ElementName = "direction")]
        public string Direction
        {
            get
            {
                return this.direction;
            }

            set
            {
                this.direction = value;
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
        /// Gets or sets Code
        /// </summary>
        [XmlElement(ElementName = "code")]
        public string Code
        {
            get
            {
                return this.code;
            }

            set
            {
                this.code = value;
            }
        }

        /// <summary>
        /// Gets or sets LocationType
        /// </summary>
        [XmlElement(ElementName = "locationType")]
        public int LocationType
        {
            get
            {
                return this.locationType;
            }

            set
            {
                this.locationType = value;
            }
        }

        /// <summary>
        /// Gets or sets WheelchairBoarding
        /// </summary>
        [XmlElement(ElementName = "wheelchairBoarding")]
        public string WheelchairBoarding
        {
            get
            {
                return this.wheelchairBoarding;
            }

            set
            {
                this.wheelchairBoarding = value;
            }
        }

        /// <summary>
        /// Gets or sets RouteIds
        /// </summary>
        [XmlArray("routeIds")]
        [XmlArrayItem(IsNullable = false)]
        public string[] RouteIds
        {
            get
            {
                return this.routeIds;
            }

            set
            {
                this.routeIds = value;
            }
        }

        /// <summary>
        /// Gets or sets Xml raw content for Stop
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

        /// <summary>
        /// Fills in the appropriate values from the XElement
        /// </summary>
        /// <param name="stopElement">XElement to get values from</param>
        protected void ParseStopElement(XElement stopElement)
        {
            this.id = stopElement.GetFirstElementValue<string>("id");
            this.lat = stopElement.GetFirstElementValue<decimal>("lat");
            this.lon = stopElement.GetFirstElementValue<decimal>("lon");
            this.direction = stopElement.GetFirstElementValue<string>("direction");
            this.name = stopElement.GetFirstElementValue<string>("name");
            this.code = stopElement.GetFirstElementValue<string>("code");
            this.locationType = stopElement.GetFirstElementValue<int>("locationType");
            this.wheelchairBoarding = stopElement.GetFirstElementValue<string>("wheelchairBoarding");
            this.routeIds = (from routeString in stopElement.Element("routeIds").Descendants("string")
                             select routeString.Value).ToArray();
            this.rawContent = stopElement.ToString();
        }
    }
}
