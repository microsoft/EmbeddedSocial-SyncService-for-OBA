// <copyright file="StopEqualityComparerById.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Compares two stops by their Ids
    /// </summary>
    public class StopEqualityComparerById : IEqualityComparer<StopEntity>
    {
        /// <summary>
        /// Compares two stops by stop Id
        /// </summary>
        /// <param name="stop1">first stop</param>
        /// <param name="stop2">second stop</param>
        /// <returns>true if the stop Ids match</returns>
        public bool Equals(StopEntity stop1, StopEntity stop2)
        {
            if (stop1 == null && stop2 == null)
            {
                return true;
            }
            else if (stop1 == null | stop2 == null)
            {
                return false;
            }
            else if (stop1.Id == stop2.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the hashcode for the stop Id
        /// </summary>
        /// <param name="stop">stop</param>
        /// <returns>hashcode based on stop Id</returns>
        public int GetHashCode(StopEntity stop)
        {
            return stop.Id.GetHashCode();
        }
    }
}