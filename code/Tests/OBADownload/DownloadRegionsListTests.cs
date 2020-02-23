// <copyright file="DownloadRegionsListTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests.OBADownload
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.OBAClient;
    using OBAService.OBAClient.Model;
    using OBAService.OBADownload;
    using OBAService.Storage;
    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Tests the regions list download and store class
    /// </summary>
    [TestClass]
    public class DownloadRegionsListTests
    {
        /// <summary>
        /// Tests the download and store of a regions list entity
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task DownloadAndStore()
        {
            string runId = RunId.GenerateTestRunId();

            // create storage managers
            StorageManager downloadStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, runId);
            StorageManager downloadMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DownloadMetadata, runId);

            // create OBA client
            Client obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);

            // create tables
            await downloadStorage.CreateTables();
            await downloadMetadataStorage.CreateTables();

            // download and store
            RegionsList regionsList = await DownloadRegionsList.DownloadAndStore(obaClient, downloadStorage, downloadMetadataStorage, runId);

            // get it from store
            RegionsListEntity regionsListEntity = downloadStorage.RegionsListStore.Get();
            IEnumerable<DownloadMetadataEntity> downloadMetadataEntities = downloadMetadataStorage.DownloadMetadataStore.Get(runId);

            // clean up
            await downloadStorage.DeleteDataTables();
            await downloadMetadataStorage.DownloadMetadataStore.Delete(runId);

            // check
            Assert.IsNotNull(regionsList);
            Assert.IsNotNull(regionsListEntity);
            Assert.IsNotNull(downloadMetadataEntities);
            Assert.AreEqual(regionsList.RegionsRawContent, regionsListEntity.RawContent);
            Assert.AreEqual(regionsList.Uri, TestConstants.OBARegionsListUri);
            Assert.AreEqual(regionsListEntity.RecordType, Enum.GetName(typeof(RecordType), RecordType.RegionsList));
            Assert.AreEqual(TestConstants.OBARegionsListUri, regionsListEntity.RegionsServiceUri);
            Assert.AreEqual(downloadMetadataEntities.Count(), 1);
            Assert.AreEqual(downloadMetadataEntities.First().RecordType, RecordType.RegionsList.ToString());
            Assert.AreEqual(downloadMetadataEntities.First().Count, 1);
            Assert.AreEqual(downloadMetadataEntities.First().AgencyId, string.Empty);
            Assert.AreEqual(downloadMetadataEntities.First().RegionId, string.Empty);
            Assert.AreEqual(downloadMetadataEntities.First().RunId, runId);
        }
    }
}
