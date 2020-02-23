// <copyright file="RouteStoreTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests.Storage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.OBAClient;
    using OBAService.OBAClient.Model;
    using OBAService.Storage;
    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Tests the route store
    /// </summary>
    [TestClass]
    public class RouteStoreTests
    {
        /// <summary>
        /// Tests the insert and get of route entities from one agency
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task InsertAndGet()
        {
            // create tables
            StorageManager storageManager = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, RunId.GenerateTestRunId());
            await storageManager.CreateTables();

            // fetch routes
            var region = new Region() { Id = "1", RegionName = "Puget Sound", ObaBaseUrl = "http://api.pugetsound.onebusaway.org/" };
            var agency = new Agency() { Id = "1", Name = "Metro Transit" };
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var routes = await obaClient.GetAllRoutesAsync(region, agency);

            // stick it into store
            await storageManager.RouteStore.Insert(routes, region.Id, agency.Id);

            // get the list from the store
            IEnumerable<RouteEntity> routeEntities = await storageManager.RouteStore.GetAllRoutes(region.Id);

            // clean up
            await storageManager.DeleteDataTables();

            // check the retrieved records
            Assert.AreEqual(routeEntities.Count(), routes.Count());

            HashSet<string> id1 = new HashSet<string>(routes.Select(o => o.Id));
            HashSet<string> id2 = new HashSet<string>(routeEntities.Select(o => o.Id));
            Assert.IsTrue(id1.IsSubsetOf(id2));
            Assert.IsTrue(id2.IsSubsetOf(id1));

            HashSet<string> longName1 = new HashSet<string>(routes.Select(o => o.LongName));
            HashSet<string> longName2 = new HashSet<string>(routeEntities.Select(o => o.LongName));
            Assert.IsTrue(longName1.IsSubsetOf(longName2));
            Assert.IsTrue(longName2.IsSubsetOf(longName1));

            HashSet<string> shortName1 = new HashSet<string>(routes.Select(o => o.ShortName));
            HashSet<string> shortName2 = new HashSet<string>(routeEntities.Select(o => o.ShortName));
            Assert.IsTrue(shortName1.IsSubsetOf(shortName2));
            Assert.IsTrue(shortName2.IsSubsetOf(shortName1));

            HashSet<string> description1 = new HashSet<string>(routes.Select(o => o.Description));
            HashSet<string> description2 = new HashSet<string>(routeEntities.Select(o => o.Description));
            Assert.IsTrue(description1.IsSubsetOf(description2));
            Assert.IsTrue(description2.IsSubsetOf(description1));

            HashSet<string> raw1 = new HashSet<string>(routes.Select(o => o.RawContent));
            HashSet<string> raw2 = new HashSet<string>(routeEntities.Select(o => o.RawContent));
            Assert.IsTrue(raw1.IsSubsetOf(raw2));
            Assert.IsTrue(raw2.IsSubsetOf(raw1));
        }
    }
}
