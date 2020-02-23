// <copyright file="StopStoreTests.cs" company="Microsoft">
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
    /// Tests the stop store
    /// </summary>
    [TestClass]
    public class StopStoreTests
    {
        /// <summary>
        /// Tests the insert and get of a stop entity from one agency
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task InsertAndGet()
        {
            // create tables
            StorageManager storageManager = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, RunId.GenerateTestRunId());
            await storageManager.CreateTables();

            // fetch stop
            var region = new Region() { Id = "1", RegionName = "Puget Sound", ObaBaseUrl = "http://api.pugetsound.onebusaway.org/" };
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var stop = await obaClient.GetStopDetailsAsync(region, "1_10620");
            List<OBAClient.Model.Stop> stops = new List<Stop>();
            stops.Add(stop);

            // stick it into store
            await storageManager.StopStore.Insert(stops, region.Id);

            // get the list from the store
            IEnumerable<StopEntity> stopEntities = await storageManager.StopStore.GetAllStops(region.Id);

            // clean up
            await storageManager.DeleteDataTables();

            // check the retrieved records
            Assert.AreEqual(stopEntities.Count(), stops.Count());

            HashSet<string> id1 = new HashSet<string>(stops.Select(o => o.Id));
            HashSet<string> id2 = new HashSet<string>(stopEntities.Select(o => o.Id));
            Assert.IsTrue(id1.IsSubsetOf(id2));
            Assert.IsTrue(id2.IsSubsetOf(id1));

            HashSet<string> name1 = new HashSet<string>(stops.Select(o => o.Name));
            HashSet<string> name2 = new HashSet<string>(stopEntities.Select(o => o.Name));
            Assert.IsTrue(name1.IsSubsetOf(name2));
            Assert.IsTrue(name2.IsSubsetOf(name1));

            HashSet<string> code = new HashSet<string>(stops.Select(o => o.Code));
            HashSet<string> description2 = new HashSet<string>(stopEntities.Select(o => o.Code));
            Assert.IsTrue(code.IsSubsetOf(description2));
            Assert.IsTrue(description2.IsSubsetOf(code));

            HashSet<string> raw1 = new HashSet<string>(stops.Select(o => o.RawContent));
            HashSet<string> raw2 = new HashSet<string>(stopEntities.Select(o => o.RawContent));
            Assert.IsTrue(raw1.IsSubsetOf(raw2));
            Assert.IsTrue(raw2.IsSubsetOf(raw1));
        }
    }
}
