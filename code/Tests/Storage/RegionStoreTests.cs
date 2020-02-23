// <copyright file="RegionStoreTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests.Storage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.OBAClient;
    using OBAService.Storage;
    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Tests the region store
    /// </summary>
    [TestClass]
    public class RegionStoreTests
    {
        /// <summary>
        /// Tests the insert and get of region entities
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task InsertAndGet()
        {
            // create tables
            StorageManager storageManager = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, RunId.GenerateTestRunId());
            await storageManager.CreateTables();

            // fetch regions
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var regionsList = await obaClient.GetAllRegionsAsync();

            // stick it into store
            await storageManager.RegionStore.Insert(regionsList.Regions);

            // get the list from the store
            IEnumerable<RegionEntity> regionEntities = storageManager.RegionStore.GetAllRegions();

            // clean up
            await storageManager.DeleteDataTables();

            // check the retrieved records
            Assert.AreEqual(regionEntities.Count(), regionsList.Regions.Count);

            HashSet<string> id1 = new HashSet<string>(regionsList.Regions.Select(o => o.Id));
            HashSet<string> id2 = new HashSet<string>(regionEntities.Select(o => o.Id));
            Assert.IsTrue(id1.IsSubsetOf(id2));
            Assert.IsTrue(id2.IsSubsetOf(id1));

            HashSet<string> name1 = new HashSet<string>(regionsList.Regions.Select(o => o.RegionName));
            HashSet<string> name2 = new HashSet<string>(regionEntities.Select(o => o.RegionName));
            Assert.IsTrue(name1.IsSubsetOf(name2));
            Assert.IsTrue(name2.IsSubsetOf(name1));

            HashSet<string> url1 = new HashSet<string>(regionsList.Regions.Select(o => o.ObaBaseUrl));
            HashSet<string> url2 = new HashSet<string>(regionEntities.Select(o => o.ObaBaseUrl));
            Assert.IsTrue(url1.IsSubsetOf(url2));
            Assert.IsTrue(url2.IsSubsetOf(url1));

            HashSet<string> raw1 = new HashSet<string>(regionsList.Regions.Select(o => o.RawContent));
            HashSet<string> raw2 = new HashSet<string>(regionEntities.Select(o => o.RawContent));
            Assert.IsTrue(raw1.IsSubsetOf(raw2));
            Assert.IsTrue(raw2.IsSubsetOf(raw1));
        }
    }
}
