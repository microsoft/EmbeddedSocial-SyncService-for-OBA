// <copyright file="Queues.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tools.ManageServerState
{
    using System.Threading.Tasks;

    /// <summary>
    /// Clean and create service bus queues
    /// </summary>
    public static class Queues
    {
        /// <summary>
        /// Provision queues
        ///
        /// Queues are not currently used by the OBA service.
        /// </summary>
        /// <returns>task</returns>
        public static async Task Create()
        {
            await Task.Delay(0);
            return;
        }

        /// <summary>
        /// Clean queues
        ///
        /// Queues are not currently used by the OBA service.
        /// </summary>
        /// <returns>task</returns>
        public static async Task Clean()
        {
            await Task.Delay(0);
            return;
        }
    }
}
