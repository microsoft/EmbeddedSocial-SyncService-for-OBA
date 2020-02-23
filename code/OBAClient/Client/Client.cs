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

namespace OBAService.OBAClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    using OBAService.OBAClient.Model;

    /// <summary>
    /// Class talks to the OneBusAway server and grabs data from it.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// key to access OBA servers with
        /// </summary>
        private string apiKey;

        /// <summary>
        /// URI to fetch the regions list from
        /// </summary>
        private string regionsServiceUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="apiKey">OBA API key</param>
        /// <param name="regionsServiceUri">URI to fetch the regions list from</param>
        public Client(string apiKey, string regionsServiceUri)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentNullException("apiKey");
            }

            this.apiKey = apiKey;

            if (string.IsNullOrWhiteSpace(regionsServiceUri))
            {
                throw new ArgumentNullException("regionsServiceUri");
            }

            this.regionsServiceUri = regionsServiceUri;
        }

        /// <summary>
        /// Get all regions and the raw content from OBA regions list URI
        /// </summary>
        /// <returns>List of regions and Raw content</returns>
        public async Task<RegionsList> GetAllRegionsAsync()
        {
            IList<Region> regions = new List<Region>();
            string regionsRawContent = string.Empty;
            using (CancellationTokenSource source = new CancellationTokenSource(Constants.HttpTimeoutMs))
            {
                using (HttpClient client = new HttpClient())
                {
                    var message = await client.GetAsync(this.regionsServiceUri, source.Token);
                    regionsRawContent = await message.Content.ReadAsStringAsync();
                    XDocument regionsXml = XDocument.Parse(regionsRawContent);
                    if (regionsXml != null)
                    {
                        var xmlSerializer = new XmlSerializer(typeof(Region));
                        foreach (var item in regionsXml.Descendants("region"))
                        {
                            using (var reader = item.CreateReader())
                            {
                                var regionItem = (Region)xmlSerializer.Deserialize(reader);

                                // Set the RawContent to Region XElement
                                regionItem.RawContent = item.ToString();
                                regions.Add(regionItem);
                            }
                        }
                    }
                }
            }

            return new RegionsList { Regions = regions, RegionsRawContent = regionsRawContent, Uri = this.regionsServiceUri };
        }

        /// <summary>
        /// Get all agencies for the specified region from Oba
        /// </summary>
        /// <param name="region">specified region</param>
        /// <returns>IEnumerable of agencies</returns>
        public async Task<IEnumerable<Agency>> GetAllAgenciesAsync(Region region)
        {
            IList<Agency> agencies = new List<Agency>();
            ObaMethod method = ObaMethod.Agencies_with_coverage;
            var helper = new ClientHelper(region, method, this.apiKey);
            var agenciesXml = await helper.SendAndReceiveAsync();
            if (agenciesXml != null)
            {
                var xmlSerializer = new XmlSerializer(typeof(Agency));
                foreach (var item in agenciesXml.Descendants("agency"))
                {
                    using (var reader = item.CreateReader())
                    {
                        var agencyItem = (Agency)xmlSerializer.Deserialize(reader);

                        // Set the RawContent to Agency XElement
                        agencyItem.RawContent = item.ToString();
                        agencies.Add(agencyItem);
                    }
                }
            }

            return agencies;
        }

        /// <summary>
        /// Get all routes for specified region and agency from Oba
        /// </summary>
        /// <param name="region">specified region</param>
        /// <param name="agency">specified agency</param>
        /// <returns>IEnumerable of routes</returns>
        public async Task<IEnumerable<Route>> GetAllRoutesAsync(Region region, Agency agency)
        {
            IList<Route> routes = new List<Route>();
            ObaMethod method = ObaMethod.Routes_for_agency;
            var helper = new ClientHelper(region, method, this.apiKey);
            helper.SetId(agency.Id);

            var routesXml = await helper.SendAndReceiveAsync();
            if (routesXml != null)
            {
                var xmlSerializer = new XmlSerializer(typeof(Route));
                foreach (var item in routesXml.Descendants("route"))
                {
                    using (var reader = item.CreateReader())
                    {
                        var routeItem = (Route)xmlSerializer.Deserialize(reader);

                        // Set the RawContent to route XElement
                        routeItem.RawContent = item.ToString();
                        routes.Add(routeItem);
                    }
                }
            }

            return routes;
        }

        /// <summary>
        /// Get all stop ids for specified region and agency from Oba
        /// </summary>
        /// <param name="region">specified region</param>
        /// <param name="agency">specified agency</param>
        /// <returns>IEnumerable of stop Ids</returns>
        public async Task<IEnumerable<string>> GetAllStopIdsAsync(Region region, Agency agency)
        {
            IList<string> stopIds = new List<string>();
            ObaMethod method = ObaMethod.Stop_ids_for_agency;
            var helper = new ClientHelper(region, method, this.apiKey);
            helper.SetId(agency.Id);

            var stopsXml = await helper.SendAndReceiveAsync();
            if (stopsXml != null)
            {
                foreach (var item in stopsXml.Descendants("string"))
                {
                    stopIds.Add(item.Value);
                }
            }

            return stopIds;
        }

        /// <summary>
        /// Get stop details for specified region and stop Id
        /// </summary>
        /// <param name="region">specified region</param>
        /// <param name="stopId">specified stop Id</param>
        /// <returns>Stop details</returns>
        public async Task<Stop> GetStopDetailsAsync(Region region, string stopId)
        {
            Stop stopItem = null;
            ObaMethod method = ObaMethod.Stop;
            var helper = new ClientHelper(region, method, this.apiKey);
            helper.SetId(stopId);

            var stopXml = await helper.SendAndReceiveAsync();
            if (stopXml != null)
            {
                var xmlSerializer = new XmlSerializer(typeof(Stop));
                var item = stopXml.Descendants("entry").First();
                using (var reader = item.CreateReader())
                {
                    stopItem = (Stop)xmlSerializer.Deserialize(reader);

                    // Set the RawContent to Stop XElement
                    stopItem.RawContent = item.ToString();
                }
            }

            return stopItem;
        }

        /// <summary>
        /// Get stops details for specified region and route Id
        /// </summary>
        /// <param name="region">specified region</param>
        /// <param name="routeId">specified route Id</param>
        /// <returns>Stops details</returns>
        public async Task<Stop[]> GetStopsForRouteAsync(Region region, string routeId)
        {
            Stop[] stops = null;
            ObaMethod method = ObaMethod.Stops_for_route;
            var helper = new ClientHelper(region, method, this.apiKey);
            helper.SetId(routeId);

            var stopsXml = await helper.SendAndReceiveAsync();
            if (stopsXml != null)
            {
                var xmlSerializer = new XmlSerializer(typeof(Stop));
                return (from stopElement in stopsXml.Descendants("stop")
                        select new Stop(stopElement)).ToArray();
            }

            return stops;
        }
    }
}
