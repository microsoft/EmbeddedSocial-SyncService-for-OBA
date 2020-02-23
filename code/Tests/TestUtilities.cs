// <copyright file="TestUtilities.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using OBAService.Storage;
    using OBAService.Storage.Model;

    /// <summary>
    /// Utilities for tests
    /// </summary>
    public static class TestUtilities
    {
        /// <summary>
        /// Creates a region entity with a random Id and name
        /// </summary>
        /// <returns>region entity</returns>
        public static RegionEntity FakeRegionEntity()
        {
            RegionEntity region = new RegionEntity()
            {
                Id = Guid.NewGuid().ToString(),
                RegionName = Guid.NewGuid().ToString(),
                ObaBaseUrl = string.Empty,
                RecordType = RecordType.Region.ToString(),
                RowState = DataRowState.Default.ToString(),
                RawContent = string.Empty
            };

            return region;
        }

        /// <summary>
        /// Creates an agency entity with a random Id and name
        /// </summary>
        /// <param name="regionId">optional region Id; if not specified, one will be created</param>
        /// <returns>agency entity</returns>
        public static AgencyEntity FakeAgencyEntity(string regionId = null)
        {
            AgencyEntity agency = new AgencyEntity()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Url = string.Empty,
                Phone = string.Empty,
                RegionId = regionId == null ? Guid.NewGuid().ToString() : regionId,
                RecordType = RecordType.Agency.ToString(),
                RowState = DataRowState.Default.ToString(),
                RawContent = string.Empty
            };

            return agency;
        }

        /// <summary>
        /// Creates a route entity with a random Id and name
        /// </summary>
        /// <param name="regionId">optional region Id; if not specified, one will be created</param>
        /// <param name="agencyId">optional agency Id; if not specified, one will be created</param>
        /// <returns>route entity</returns>
        public static RouteEntity FakeRouteEntity(string regionId = null, string agencyId = null)
        {
            RouteEntity route = new RouteEntity()
            {
                Id = Guid.NewGuid().ToString(),
                ShortName = Guid.NewGuid().ToString(),
                LongName = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Url = string.Empty,
                AgencyId = agencyId == null ? Guid.NewGuid().ToString() : agencyId,
                RegionId = regionId == null ? Guid.NewGuid().ToString() : regionId,
                RecordType = RecordType.Route.ToString(),
                RowState = DataRowState.Default.ToString(),
                RawContent = string.Empty
            };

            return route;
        }

        /// <summary>
        /// Creates a stop entity with a random Id and name
        /// </summary>
        /// <param name="regionId">optional region Id; if not specified, one will be created</param>
        /// <returns>stop entity</returns>
        public static StopEntity FakeStopEntity(string regionId = null)
        {
            StopEntity stop = new StopEntity()
            {
                Id = Guid.NewGuid().ToString(),
                Lat = 0,
                Lon = 0,
                Direction = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                RegionId = regionId == null ? Guid.NewGuid().ToString() : regionId,
                RecordType = RecordType.Stop.ToString(),
                RowState = DataRowState.Default.ToString(),
                RawContent = string.Empty
            };

            return stop;
        }

        /// <summary>
        /// Deletes and recreates publish storage
        /// </summary>
        /// <param name="publishStorage">interface to publish storage</param>
        /// <returns>task that cleans and recreates publish storage</returns>
        public static async Task CleanPublishStorage(StorageManager publishStorage)
        {
            await publishStorage.DeleteTables();
            int tries = TestConstants.AzureTableCreateTries;
            while (tries > 0)
            {
                try
                {
                    await publishStorage.CreateTables();
                    return;
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("(409) Conflict") || tries == 1)
                    {
                        throw e;
                    }

                    // Azure Tables will temporarily throw an error message right after a table has been
                    // deleted and you try to recreate that table.
                    Thread.Sleep(TestConstants.AzureTableDelay);
                    tries--;
                }
            }
        }
    }
}
