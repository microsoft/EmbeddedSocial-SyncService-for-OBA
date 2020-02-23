// <copyright file="PublishMetadataStoreTests.cs" company="Microsoft">
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
    /// Tests the publish metadata store
    /// </summary>
    [TestClass]
    public class PublishMetadataStoreTests
    {
        /// <summary>
        /// Tests the insert and get of a publish metadata entity
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task InsertAndGet()
        {
            // create tables
            string runId = RunId.GenerateTestRunId();
            StorageManager storageManager = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.PublishMetadata, runId);
            await storageManager.CreateTables();

            // create a bogus publish metadata entity
            PublishMetadataEntity entity = new PublishMetadataEntity()
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
            List<PublishMetadataEntity> entities = new List<PublishMetadataEntity>();
            entities.Add(entity);
            await storageManager.PublishMetadataStore.Insert(entities);

            // get it from the store
            IEnumerable<PublishMetadataEntity> retrievedEntities1 = storageManager.PublishMetadataStore.Get(runId);

            // clean up
            await storageManager.PublishMetadataStore.Delete(runId);

            // get it from the store again
            IEnumerable<PublishMetadataEntity> retrievedEntities2 = storageManager.PublishMetadataStore.Get(runId);

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
