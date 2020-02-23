// <copyright file="DiffMetadataStoreTests.cs" company="Microsoft">
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
    /// Tests the diff metadata store
    /// </summary>
    [TestClass]
    public class DiffMetadataStoreTests
    {
        /// <summary>
        /// Tests the insert and get of a diff metadata entity
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task InsertAndGet()
        {
            // create tables
            string runId = RunId.GenerateTestRunId();
            StorageManager storageManager = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
            await storageManager.CreateTables();

            // create a bogus diff metadata entity
            DiffMetadataEntity entity = new DiffMetadataEntity()
            {
                RunId = runId,
                RegionId = "abcd",
                AgencyId = "1234",
                RecordType = RecordType.Route.ToString(),
                AddedCount = 50,
                UpdatedCount = 75,
                DeletedCount = 1,
                ResurrectedCount = 5
            };

            // stick it into store
            List<DiffMetadataEntity> entities = new List<DiffMetadataEntity>();
            entities.Add(entity);
            await storageManager.DiffMetadataStore.Insert(entities);

            // get it from the store
            IEnumerable<DiffMetadataEntity> retrievedEntities1 = storageManager.DiffMetadataStore.Get(runId);

            // clean up
            await storageManager.DiffMetadataStore.Delete(runId);

            // get it from the store again
            IEnumerable<DiffMetadataEntity> retrievedEntities2 = storageManager.DiffMetadataStore.Get(runId);

            // check the retrieved records
            Assert.IsNotNull(retrievedEntities1);
            Assert.AreEqual(retrievedEntities1.Count(), entities.Count);
            Assert.IsNull(retrievedEntities2);
            Assert.AreEqual(entity.RunId, retrievedEntities1.First().RunId);
            Assert.AreEqual(entity.RegionId, retrievedEntities1.First().RegionId);
            Assert.AreEqual(entity.AgencyId, retrievedEntities1.First().AgencyId);
            Assert.AreEqual(entity.RecordType, retrievedEntities1.First().RecordType);
            Assert.AreEqual(entity.AddedCount, retrievedEntities1.First().AddedCount);
            Assert.AreEqual(entity.UpdatedCount, retrievedEntities1.First().UpdatedCount);
            Assert.AreEqual(entity.DeletedCount, retrievedEntities1.First().DeletedCount);
            Assert.AreEqual(entity.ResurrectedCount, retrievedEntities1.First().ResurrectedCount);
        }
    }
}
