// <copyright file="StopEqualityComparerByChecksum.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Compares two stops by the subset of properties that Embedded Social uses
    /// </summary>
    public class StopEqualityComparerByChecksum : IEqualityComparer<StopEntity>
    {
        /// <summary>
        /// Compares two stops by the subset of properties that Embedded Social uses
        /// </summary>
        /// <param name="stop1">first stop</param>
        /// <param name="stop2">second stop</param>
        /// <returns>true if the stop contents match</returns>
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
            else if (stop1.Checksum == stop2.Checksum)
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
        /// <param name="stop">stop</param>
        /// <returns>hashcode based on stop contents</returns>
        public int GetHashCode(StopEntity stop)
        {
            return stop.Checksum;
        }
    }
}