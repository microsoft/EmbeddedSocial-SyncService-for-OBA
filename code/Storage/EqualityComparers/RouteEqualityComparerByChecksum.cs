// <copyright file="RouteEqualityComparerByChecksum.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Compares two routes by the subset of properties that Embedded Social uses
    /// </summary>
    public class RouteEqualityComparerByChecksum : IEqualityComparer<RouteEntity>
    {
        /// <summary>
        /// Compares two routes by the subset of properties that Embedded Social uses
        /// </summary>
        /// <param name="route1">first route</param>
        /// <param name="route2">second route</param>
        /// <returns>true if the route contents match</returns>
        public bool Equals(RouteEntity route1, RouteEntity route2)
        {
            if (route1 == null && route2 == null)
            {
                return true;
            }
            else if (route1 == null | route2 == null)
            {
                return false;
            }
            else if (route1.Checksum == route2.Checksum)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the hashcode for the subset of properties that Embedded Social uses
        /// </summary>
        /// <param name="route">route</param>
        /// <returns>hashcode based on route contents</returns>
        public int GetHashCode(RouteEntity route)
        {
            return route.Checksum;
        }
    }
}