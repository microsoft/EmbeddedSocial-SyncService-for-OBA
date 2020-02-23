// <copyright file="RunId.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Utils
{
    using System;

    /// <summary>
    /// Run Id utilities.
    /// </summary>
    public static class RunId
    {
        /// <summary>
        /// Creates a run id that encodes the current time and an indication that this was a test run
        /// </summary>
        /// <returns>runId</returns>
        public static string GenerateTestRunId()
        {
            return "Test" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        /// <summary>
        /// Creates a run id that encodes the current time
        /// </summary>
        /// <returns>runId</returns>
        public static string GenerateRunId()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
}
