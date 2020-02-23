// <copyright file="AgencyStoreTests.cs" company="Microsoft">
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
    /// Tests the agency store
    /// </summary>
    [TestClass]
    public class AgencyStoreTests
    {
        /// <summary>
        /// Tests the insert and get of agency entitities from a region
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task InsertAndGet()
        {
            // create tables
            StorageManager storageManager = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, RunId.GenerateTestRunId());
            await storageManager.CreateTables();

            // fetch agencies from a region
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var region = new Region() { Id = "1", RegionName = "Puget Sound", ObaBaseUrl = "http://api.pugetsound.onebusaway.org/" };
            List<Agency> agencies = (await obaClient.GetAllAgenciesAsync(region)).ToList();

            // stick them into the store
            await storageManager.AgencyStore.Insert(agencies, region.Id);

            // get them from the store
            IEnumerable<AgencyEntity> agencyEntities = await storageManager.AgencyStore.GetAllAgencies(region.Id);

            // clean up
            await storageManager.DeleteDataTables();

            // check the retrieved records
            Assert.IsNotNull(agencyEntities);
            Assert.AreEqual(agencyEntities.Count(), agencies.Count);

            HashSet<string> id1 = new HashSet<string>(agencyEntities.Select(o => o.Id));
            HashSet<string> id2 = new HashSet<string>(agencies.Select(o => o.Id));
            Assert.IsTrue(id1.IsSubsetOf(id2));
            Assert.IsTrue(id2.IsSubsetOf(id1));

            HashSet<string> name1 = new HashSet<string>(agencyEntities.Select(o => o.Name));
            HashSet<string> name2 = new HashSet<string>(agencies.Select(o => o.Name));
            Assert.IsTrue(name1.IsSubsetOf(name2));
            Assert.IsTrue(name2.IsSubsetOf(name1));

            HashSet<string> rawContent1 = new HashSet<string>(agencyEntities.Select(o => o.RawContent));
            HashSet<string> rawContent2 = new HashSet<string>(agencies.Select(o => o.RawContent));
            Assert.IsTrue(rawContent1.IsSubsetOf(rawContent2));
            Assert.IsTrue(rawContent2.IsSubsetOf(rawContent1));
        }
    }
}
