// <copyright file="Constants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.OBAClient
{
    /// <summary>
    /// A collection of constants used within this project
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Time in milliseconds before we time out network calls.
        /// </summary>
        public const int HttpTimeoutMs = 15000;

        /// <summary>
        /// Retry count for OBA requests in case servers are busy.
        /// </summary>
        public const int RequestRetryCount = 20;

        /// <summary>
        /// Minimum time in milliseconds to wait if an OBA server throttles us
        /// </summary>
        public const int MinimumThrottlingDelay = 1000;

        /// <summary>
        /// Maximum time in milliseconds to wait if an OBA server throttles us
        /// </summary>
        public const int MaximumThrottlingDelay = 20000;
    }
}
