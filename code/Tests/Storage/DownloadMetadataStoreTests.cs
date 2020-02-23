// <copyright file="DownloadMetadataStoreTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests.Storage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.Storage;
    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Tests the download metadata store
    /// </summary>
    [TestClass]
    public class DownloadMetadataStoreTests
    {
        /// <summary>
        /// Tests the insert and get of a download metadata entity
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task InsertAndGet()
        {
            // create tables
            string runId = RunId.GenerateTestRunId();
            StorageManager storageManager = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DownloadMetadata, runId);
            await storageManager.CreateTables();

            // create a bogus download metadata entity
            DownloadMetadataEntity entity = new DownloadMetadataEntity()
            {
                RunId = runId,
                RegionId = "abcd",
                AgencyId = "1234",
                RecordType = RecordType.Stop.ToString(),
                Count = 666
            };

            // stick it into store
            List<DownloadMetadataEntity> entities = new List<DownloadMetadataEntity>();
            entities.Add(entity);
            await storageManager.DownloadMetadataStore.Insert(entities);

            // get it from the store
            IEnumerable<DownloadMetadataEntity> retrievedEntities1 = storageManager.DownloadMetadataStore.Get(runId);

            // clean up
            await storageManager.DownloadMetadataStore.Delete(runId);

            // get it from the store again
            IEnumerable<DownloadMetadataEntity> retrievedEntities2 = storageManager.DownloadMetadataStore.Get(runId);

            // check the retrieved records
            Assert.IsNotNull(retrievedEntities1);
            Assert.AreEqual(retrievedEntities1.Count(), entities.Count);
            Assert.IsNull(retrievedEntities2);
            Assert.AreEqual(entity.RunId, retrievedEntities1.First().RunId);
            Assert.AreEqual(entity.RegionId, retrievedEntities1.First().RegionId);
            Assert.AreEqual(entity.AgencyId, retrievedEntities1.First().AgencyId);
            Assert.AreEqual(entity.RecordType, retrievedEntities1.First().RecordType);
            Assert.AreEqual(entity.Count, retrievedEntities1.First().Count);
        }
    }
}
