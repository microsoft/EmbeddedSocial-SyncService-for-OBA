// <copyright file="RegionsListStoreTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests.Storage
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.OBAClient;
    using OBAService.OBAClient.Model;
    using OBAService.Storage;
    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Tests the regions list store
    /// </summary>
    [TestClass]
    public class RegionsListStoreTests
    {
        /// <summary>
        /// Tests the insert and get of a regions list entity
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task InsertAndGet()
        {
            // create tables
            StorageManager storageManager = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, RunId.GenerateTestRunId());
            await storageManager.CreateTables();

            // fetch a regionslist file
            Client obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            RegionsList regionsList = await obaClient.GetAllRegionsAsync();

            // stick it into store
            await storageManager.RegionsListStore.Insert(regionsList);

            // get it from the store
            RegionsListEntity regionsListEntity = storageManager.RegionsListStore.Get();

            // clean up
            await storageManager.DeleteDataTables();

            // check the retrieved record
            Assert.IsNotNull(regionsListEntity);
            Assert.AreEqual(regionsListEntity.RawContent, regionsList.RegionsRawContent);
            Assert.AreEqual(regionsListEntity.RegionsServiceUri, TestConstants.OBARegionsListUri);
            Assert.AreEqual(regionsListEntity.RecordType, Enum.GetName(typeof(RecordType), RecordType.RegionsList));
            Assert.AreEqual(TestConstants.OBARegionsListUri, regionsList.Uri);
        }
    }
}
