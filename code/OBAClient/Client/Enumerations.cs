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
    /// <summary>
    /// A subset of collection of methods that OneBusAway supports.
    /// The complete list is available here http://developer.onebusaway.org/modules/onebusaway-application-modules/current/api/where/methods/
    /// </summary>
    public enum ObaMethod
    {
        /// <summary>
        /// list all supported agencies along with the center of their coverage area
        /// </summary>
        Agencies_with_coverage,

        /// <summary>
        /// get a list of all routes for an agency
        /// </summary>
        Routes_for_agency,

        /// <summary>
        /// get a list of all stops for a route
        /// </summary>
        Stops_for_route,

        /// <summary>
        /// get a list of all stops for an agency
        /// </summary>
        Stop_ids_for_agency,

        /// <summary>
        /// get details for a specific stop
        /// </summary>
        Stop,
    }
}