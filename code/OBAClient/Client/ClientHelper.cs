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
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using OBAService.OBAClient.Model;

    /// <summary>
    /// Service Class used to talk to an OBA web service.
    /// </summary>
    internal class ClientHelper
    {
        /// <summary>
        /// This is the API key for the application.
        /// </summary>
        private string apiKey;

        /// <summary>
        /// This Uri builder is used to create the URI of the OBA REST service.
        /// </summary>
        private UriBuilder uriBuilder;

        /// <summary>
        /// The oba method.
        /// </summary>
        private ObaMethod obaMethod;

        /// <summary>
        /// The service Url.
        /// </summary>
        private string serviceUrl;

        /// <summary>
        /// This is the region where the request is being made to.
        /// </summary>
        private Region region;

        /// <summary>
        /// If there is an id for the request we store it here.
        /// </summary>
        private string id;

        /// <summary>
        /// Maps name / value pairs to the query string.
        /// </summary>
        private Dictionary<string, string> queryStringMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientHelper"/> class.
        /// </summary>
        /// <param name="region">region</param>
        /// <param name="obaMethod">oba method</param>
        /// <param name="apiKey">OBA API key</param>
        internal ClientHelper(Region region, ObaMethod obaMethod, string apiKey)
        {
            this.obaMethod = obaMethod;
            this.region = region;
            this.serviceUrl = this.region.ObaBaseUrl;
            this.id = null;
            this.apiKey = apiKey;

            this.uriBuilder = new UriBuilder(this.serviceUrl);
            this.SetDefaultPath();

            this.queryStringMap = new Dictionary<string, string>();
            this.queryStringMap["key"] = apiKey;
            this.queryStringMap["Version"] = "2";
        }

        /// <summary>
        /// Gets the region name that the helper will talk to.
        /// </summary>
        internal string RegionName
        {
            get
            {
                return this.region.RegionName;
            }
        }

        /// <summary>
        /// Adds a name / value pair to the query string.
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="value">value</param>
        internal void AddToQueryString(string name, string value)
        {
            this.queryStringMap[name] = value;
        }

        /// <summary>
        /// Sets the id for the rest query, if it exists.
        /// </summary>
        /// <param name="id">id</param>
        internal void SetId(string id)
        {
            this.id = id;
            this.uriBuilder = new UriBuilder(this.serviceUrl);
            this.SetPath(id);
        }

        /// <summary>
        /// Sends a payload to the service asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task<XDocument> SendAndReceiveAsync()
        {
            XDocument doc = null;

            // multiple retry attempts
            for (int i = 0; i < Constants.RequestRetryCount; i++)
            {
                try
                {
                    this.uriBuilder.Query = this.CreateQueryString();
                    doc = await this.SendAsync(this.uriBuilder.ToString());

                    // Verify that OBA sent us a valid document and that it's status code is 200:
                    int returnCode = doc.Root.GetFirstElementValue<int>("code");
                    if (returnCode != Convert.ToInt32(HttpStatusCode.OK))
                    {
                        string text = doc.Root.GetFirstElementValue<string>("text");
                        throw new ObaException(returnCode, text);
                    }

                    return doc;
                }
                catch (Exception e)
                {
                    // HTTP 401 or HTTP 429 from OBA servers means we were throttled due to requests not being spaced apart enough.
                    // HTTP 408 means server timeout.
                    // In both cases, retry after waiting.
                    if (e is ObaException &&
                        ((((ObaException)e).ErrorCode == Convert.ToInt32(HttpStatusCode.Unauthorized)) || (((ObaException)e).ErrorCode == 429) || (((ObaException)e).ErrorCode == Convert.ToInt32(HttpStatusCode.RequestTimeout))) &&
                        i < Constants.RequestRetryCount - 1)
                    {
                        // wait before trying again
                        int delay = Constants.MinimumThrottlingDelay * (i + 1);
                        delay = (delay < Constants.MinimumThrottlingDelay) ? Constants.MinimumThrottlingDelay : delay;
                        delay = (delay > Constants.MaximumThrottlingDelay) ? Constants.MaximumThrottlingDelay : delay;
                        await Task.Delay(delay);
                    }
                    else
                    {
                        throw new Exception("Failure on URI: " + this.uriBuilder.ToString(), e);
                    }
                }
            }

            // execution should never reach here
            throw new Exception("unreachable code has been reached");
        }

        /// <summary>
        /// Sets the default path.
        /// </summary>
        private void SetDefaultPath()
        {
            this.SetPath(null);
        }

        /// <summary>
        /// Sets the path of the uri.
        /// </summary>
        /// <param name="id">The ID to set</param>
        private void SetPath(string id)
        {
            // If the URI we get back is missing a backslash, add it first:
            if (!string.IsNullOrEmpty(this.uriBuilder.Path) && !this.uriBuilder.Path.EndsWith("/"))
            {
                this.uriBuilder.Path += "/";
            }

            this.uriBuilder.Path += "api/where/";

            string obaMethodString = this.obaMethod.ToString().ToLowerInvariant();
            obaMethodString = obaMethodString.Replace('_', '-');

            if (!string.IsNullOrEmpty(id))
            {
                obaMethodString += "/";
                obaMethodString += id;
            }

            obaMethodString += ".xml";
            this.uriBuilder.Path += obaMethodString;
        }

        /// <summary>
        /// Creates the query string out of the current queryStringMap object.
        /// </summary>
        /// <returns>Query string</returns>
        private string CreateQueryString()
        {
            return string.Join("&", from keyValuePair in this.queryStringMap select string.Format(CultureInfo.CurrentCulture, "{0}={1}", keyValuePair.Key, keyValuePair.Value));
        }

        /// <summary>
        /// Returns XDocument from queries uri
        /// </summary>
        /// <param name="uri">Oba uri</param>
        /// <returns>XDocument</returns>
        private async Task<XDocument> SendAsync(string uri)
        {
            string responseString = null;
            try
            {
                using (CancellationTokenSource source = new CancellationTokenSource(Constants.HttpTimeoutMs))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var message = await client.GetAsync(uri, source.Token);
                        responseString = await message.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (TaskCanceledException)
            {
                throw new ObaException(Convert.ToInt32(HttpStatusCode.RequestTimeout), "A timeout error prevented the request from completing for URI: " + uri);
            }

            try
            {
                    XDocument doc = XDocument.Parse(responseString);
                    return doc;
            }
            catch (Exception e)
            {
                throw new Exception("Failure on parsing XML from URI: " + uri, e);
            }
        }
    }
}