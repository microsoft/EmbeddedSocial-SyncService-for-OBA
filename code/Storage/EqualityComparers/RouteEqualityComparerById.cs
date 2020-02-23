// <copyright file="RouteEqualityComparerById.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Compares two routes by their Ids
    /// </summary>
    public class RouteEqualityComparerById : IEqualityComparer<RouteEntity>
    {
        /// <summary>
        /// Compares two routes by route Id
        /// </summary>
        /// <param name="route1">first route</param>
        /// <param name="route2">second route</param>
        /// <returns>true if the route Ids match</returns>
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
            else if (route1.Id == route2.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the hashcode for the route Id
        /// </summary>
        /// <param name="route">route</param>
        /// <returns>hashcode based on route Id</returns>
        public int GetHashCode(RouteEntity route)
        {
            return route.Id.GetHashCode();
        }
    }
}