// <copyright file="ProdConfiguration.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Utils
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Utility to check whether a configuration is using production services
    /// </summary>
    public static class ProdConfiguration
    {
        /// <summary>
        /// List of production service names
        /// </summary>
        private static readonly List<string> ProductionServices = new List<string>()
        {
            // include here substrings that will identify production services in the connection string
            // or service name for an Azure service.
            // Note that we include some names of production instances of SocialPlus so that if someone
            // accidentally runs the OBA version of CleanServerState on a SocialPlus instance then hopefully
            // we will avoid accidental deletions of important state.
            "prod",
            "api",
            "mobisys",
            "beihai"
        };

        /// <summary>
        /// Is the setting using a production service?
        /// </summary>
        /// <param name="setting">name of service or connection string</param>
        /// <returns>true if production</returns>
        public static bool IsProduction(string setting)
        {
            // empty strings are always false
            if (string.IsNullOrWhiteSpace(setting))
            {
                return false;
            }

            // go through each production service name
            foreach (string name in ProductionServices)
            {
                if (setting.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Is the setting using a production service?
        /// </summary>
        /// <param name="setting">URI of service</param>
        /// <returns>true if production</returns>
        public static bool IsProduction(Uri setting)
        {
            return IsProduction(setting.ToString());
        }
    }
}