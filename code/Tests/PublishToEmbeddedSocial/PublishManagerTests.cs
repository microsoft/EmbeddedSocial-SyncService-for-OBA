// <copyright file="PublishManagerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests.Publish
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.PublishToEmbeddedSocial;
    using OBAService.Storage;
    using OBAService.Storage.Model;
    using OBAService.Utils;
    using SocialPlus.Client.Models;

    /// <summary>
    /// Tests the publish manager class
    /// </summary>
    [TestClass]
    public class PublishManagerTests
    {
        /// <summary>
        /// Tests the publish manager with a new route and stop
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task CreateEntities()
        {
            // this test messes with the topics in Embedded Social so it should not be run against a production service
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.EmbeddedSocialUri));

            // this test messes with the publish table so it should not be run on production accounts
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.AzureStorageConnectionString));

            string runId = RunId.GenerateTestRunId();

            // clean the publish table so that we are working with known state
            StorageManager publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId);
            await TestUtilities.CleanPublishStorage(publishStorage);

            // create diff table entries for a new route and a new stop
            StorageManager diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId);
            await diffStorage.CreateTables();
            RegionEntity region = TestUtilities.FakeRegionEntity();
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);
            RouteEntity newRoute = TestUtilities.FakeRouteEntity(region.Id, agency.Id);
            newRoute.RowState = DataRowState.Create.ToString();
            await diffStorage.RouteStore.Insert(newRoute);
            StopEntity newStop = TestUtilities.FakeStopEntity(region.Id);
            newStop.RowState = DataRowState.Create.ToString();
            await diffStorage.StopStore.Insert(newStop);

            // create diff metadata entries
            StorageManager diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
            await diffMetadataStorage.CreateTables();
            DiffMetadataEntity metadata1 = new DiffMetadataEntity
            {
                RunId = runId,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            };
            DiffMetadataEntity metadata2 = new DiffMetadataEntity
            {
                RunId = runId,
                RegionId = region.Id,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            };
            await diffMetadataStorage.DiffMetadataStore.Insert(new List<DiffMetadataEntity> { metadata1, metadata2 });

            // run the publish manager
            PublishManager publishManager = new PublishManager(TestConstants.AzureStorageConnectionString, runId, TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            await publishManager.InitializeStorage();
            await publishManager.PublishAndStore();

            // query the publish table
            IEnumerable<RouteEntity> publishedRoutes = await publishStorage.RouteStore.GetAllRoutes(region.Id);
            IEnumerable<StopEntity> publishedStops = await publishStorage.StopStore.GetAllStops(region.Id);

            // query the publish metadata table
            StorageManager publishMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.PublishMetadata, runId);
            IEnumerable<PublishMetadataEntity> publishMetadata = publishMetadataStorage.PublishMetadataStore.Get(runId);

            // query Embedded Social to get the topics
            EmbeddedSocial embeddedSocial = new EmbeddedSocial(TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            PrivateObject embeddedSocialPrivate = new PrivateObject(embeddedSocial);
            string routeName = (string)embeddedSocialPrivate.Invoke("TopicName", newRoute);
            string routeTitle = (string)embeddedSocialPrivate.Invoke("TopicTitle", newRoute);
            string routeText = (string)embeddedSocialPrivate.Invoke("TopicText", newRoute);
            TopicView routeTopic = await (Task<TopicView>)embeddedSocialPrivate.Invoke("GetTopic", routeName);
            string stopName = (string)embeddedSocialPrivate.Invoke("TopicName", newStop);
            string stopTitle = (string)embeddedSocialPrivate.Invoke("TopicTitle", newStop);
            string stopText = (string)embeddedSocialPrivate.Invoke("TopicText", newStop);
            TopicView stopTopic = await (Task<TopicView>)embeddedSocialPrivate.Invoke("GetTopic", stopName);

            // delete the diff table entries, publish table entries, diff metadata entries, and Embedded Social topics
            await (Task)embeddedSocialPrivate.Invoke("DeleteTopic", routeName);
            await (Task)embeddedSocialPrivate.Invoke("DeleteTopic", stopName);
            await publishMetadataStorage.PublishMetadataStore.Delete(runId);
            await publishStorage.DeleteTables();
            await diffMetadataStorage.DiffMetadataStore.Delete(runId);
            await diffStorage.DeleteDataTables();

            // check route
            Assert.IsNotNull(publishedRoutes);
            Assert.AreEqual(publishedRoutes.Count(), 1);
            Assert.AreEqual(publishedRoutes.First().Id, newRoute.Id);
            Assert.AreEqual(publishedRoutes.First().ShortName, newRoute.ShortName);
            Assert.AreEqual(publishedRoutes.First().LongName, newRoute.LongName);
            Assert.AreEqual(publishedRoutes.First().Description, newRoute.Description);
            Assert.AreEqual(publishedRoutes.First().Url, newRoute.Url);
            Assert.AreEqual(publishedRoutes.First().AgencyId, newRoute.AgencyId);
            Assert.AreEqual(publishedRoutes.First().RegionId, newRoute.RegionId);
            Assert.AreEqual(publishedRoutes.First().RecordType, newRoute.RecordType);
            Assert.AreEqual(publishedRoutes.First().RowState, DataRowState.Create.ToString());
            Assert.AreEqual(publishedRoutes.First().RawContent, newRoute.RawContent);

            // check stop
            Assert.IsNotNull(publishedStops);
            Assert.AreEqual(publishedStops.Count(), 1);
            Assert.AreEqual(publishedStops.First().Id, newStop.Id);
            Assert.AreEqual(publishedStops.First().Lat, newStop.Lat);
            Assert.AreEqual(publishedStops.First().Lon, newStop.Lon);
            Assert.AreEqual(publishedStops.First().Direction, newStop.Direction);
            Assert.AreEqual(publishedStops.First().Name, newStop.Name);
            Assert.AreEqual(publishedStops.First().Code, newStop.Code);
            Assert.AreEqual(publishedStops.First().RegionId, newStop.RegionId);
            Assert.AreEqual(publishedStops.First().RecordType, newStop.RecordType);
            Assert.AreEqual(publishedStops.First().RowState, DataRowState.Create.ToString());
            Assert.AreEqual(publishedStops.First().RawContent, newStop.RawContent);

            // check metadata
            Assert.IsNotNull(publishMetadata);
            List<PublishMetadataEntity> publishMetadataList = publishMetadata.ToList();
            Assert.AreEqual(publishMetadataList.Count, 2);
            int routeIndex = -1;
            int stopIndex = -1;
            if (publishMetadataList[0].RecordType == RecordType.Route.ToString())
            {
                routeIndex = 0;
                stopIndex = 1;
            }
            else
            {
                routeIndex = 1;
                stopIndex = 0;
            }

            Assert.AreEqual(publishMetadataList[routeIndex].AddedCount, 1);
            Assert.AreEqual(publishMetadataList[routeIndex].AgencyId, newRoute.AgencyId);
            Assert.AreEqual(publishMetadataList[routeIndex].DeletedCount, 0);
            Assert.AreEqual(publishMetadataList[routeIndex].RecordType, RecordType.Route.ToString());
            Assert.AreEqual(publishMetadataList[routeIndex].RegionId, newRoute.RegionId);
            Assert.AreEqual(publishMetadataList[routeIndex].ResurrectedCount, 0);
            Assert.AreEqual(publishMetadataList[routeIndex].RunId, runId);
            Assert.AreEqual(publishMetadataList[routeIndex].UpdatedCount, 0);

            Assert.AreEqual(publishMetadataList[stopIndex].AddedCount, 1);
            Assert.AreEqual(publishMetadataList[stopIndex].AgencyId, string.Empty);
            Assert.AreEqual(publishMetadataList[stopIndex].DeletedCount, 0);
            Assert.AreEqual(publishMetadataList[stopIndex].RecordType, RecordType.Stop.ToString());
            Assert.AreEqual(publishMetadataList[stopIndex].RegionId, newStop.RegionId);
            Assert.AreEqual(publishMetadataList[stopIndex].ResurrectedCount, 0);
            Assert.AreEqual(publishMetadataList[stopIndex].RunId, runId);
            Assert.AreEqual(publishMetadataList[stopIndex].UpdatedCount, 0);

            // check route topic view
            Assert.IsNotNull(routeTopic);
            Assert.AreEqual(routeTopic.FriendlyName, routeName);
            Assert.AreEqual(routeTopic.Text, routeText);
            Assert.AreEqual(routeTopic.Title, routeTitle);
            Assert.AreEqual(routeTopic.Categories, newRoute.RegionId);

            // check stop topic view
            Assert.IsNotNull(stopTopic);
            Assert.AreEqual(stopTopic.FriendlyName, stopName);
            Assert.AreEqual(stopTopic.Text, stopText);
            Assert.AreEqual(stopTopic.Title, stopTitle);
            Assert.AreEqual(stopTopic.Categories, newStop.RegionId);
        }

        /// <summary>
        /// Tests the publish manager with no changes
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task UnchangedEntities()
        {
            // this test messes with the topics in Embedded Social so it should not be run against a production service
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.EmbeddedSocialUri));

            // this test messes with the publish table so it should not be run on production accounts
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.AzureStorageConnectionString));

            string runId = RunId.GenerateTestRunId();

            // clean the publish table so that we are working with known state
            StorageManager publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId);
            await TestUtilities.CleanPublishStorage(publishStorage);

            // create diff table with no entries
            StorageManager diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId);
            await diffStorage.CreateTables();

            // create diff metadata table with no entries
            StorageManager diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
            await diffMetadataStorage.CreateTables();

            // run the publish manager
            PublishManager publishManager = new PublishManager(TestConstants.AzureStorageConnectionString, runId, TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            await publishManager.InitializeStorage();
            await publishManager.PublishAndStore();

            // query the publish metadata table
            StorageManager publishMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.PublishMetadata, runId);
            IEnumerable<PublishMetadataEntity> publishMetadata = publishMetadataStorage.PublishMetadataStore.Get(runId);

            // clean up
            await publishMetadataStorage.PublishMetadataStore.Delete(runId);
            await publishStorage.DeleteTables();
            await diffStorage.DeleteDataTables();

            // check metadata
            Assert.IsNull(publishMetadata);
        }

        /// <summary>
        /// Tests the publish manager with a changed route and stop
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task UpdateEntities()
        {
            // this test messes with the topics in Embedded Social so it should not be run against a production service
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.EmbeddedSocialUri));

            // this test messes with the publish table so it should not be run on production accounts
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.AzureStorageConnectionString));

            string runId1 = RunId.GenerateTestRunId();

            // clean the publish table so that we are working with known state
            StorageManager publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId1);
            await TestUtilities.CleanPublishStorage(publishStorage);

            // first create a route and a stop and publish it
            StorageManager diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId1);
            await diffStorage.CreateTables();
            RegionEntity region = TestUtilities.FakeRegionEntity();
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);
            RouteEntity existingRoute = TestUtilities.FakeRouteEntity(region.Id, agency.Id);
            existingRoute.RowState = DataRowState.Create.ToString();
            await diffStorage.RouteStore.Insert(existingRoute);
            StopEntity existingStop = TestUtilities.FakeStopEntity(region.Id);
            existingStop.RowState = DataRowState.Create.ToString();
            await diffStorage.StopStore.Insert(existingStop);
            StorageManager diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId1);
            await diffMetadataStorage.CreateTables();
            DiffMetadataEntity metadata1 = new DiffMetadataEntity
            {
                RunId = runId1,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            };
            DiffMetadataEntity metadata2 = new DiffMetadataEntity
            {
                RunId = runId1,
                RegionId = region.Id,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            };
            await diffMetadataStorage.DiffMetadataStore.Insert(new List<DiffMetadataEntity> { metadata1, metadata2 });
            PublishManager publishManager = new PublishManager(TestConstants.AzureStorageConnectionString, runId1, TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            await publishManager.InitializeStorage();
            await publishManager.PublishAndStore();

            // clean up the old diff storage & metadata
            await diffStorage.DeleteDataTables();
            await diffMetadataStorage.DiffMetadataStore.Delete(runId1);

            // now create a new runId
            string runId2 = RunId.GenerateTestRunId();
            publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId2);
            diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId2);
            diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId2);
            await diffStorage.CreateTables();
            publishManager = new PublishManager(TestConstants.AzureStorageConnectionString, runId2, TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            await publishManager.InitializeStorage();

            // create diff table entries for updated route & stop
            existingRoute.LongName = Guid.NewGuid().ToString();
            existingRoute.ShortName = Guid.NewGuid().ToString();
            existingRoute.RowState = DataRowState.Update.ToString();
            await diffStorage.RouteStore.Insert(existingRoute);
            existingStop.Name = Guid.NewGuid().ToString();
            existingStop.RowState = DataRowState.Update.ToString();
            await diffStorage.StopStore.Insert(existingStop);

            // add diff metadata entries
            DiffMetadataEntity metadata3 = new DiffMetadataEntity
            {
                RunId = runId2,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                AddedCount = 0,
                UpdatedCount = 1,
                DeletedCount = 0,
                ResurrectedCount = 0
            };
            DiffMetadataEntity metadata4 = new DiffMetadataEntity
            {
                RunId = runId2,
                RegionId = region.Id,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = 0,
                UpdatedCount = 1,
                DeletedCount = 0,
                ResurrectedCount = 0
            };
            await diffMetadataStorage.DiffMetadataStore.Insert(new List<DiffMetadataEntity> { metadata3, metadata4 });

            // run the publish manager
            await publishManager.PublishAndStore();

            // query the publish table
            IEnumerable<RouteEntity> publishedRoutes = await publishStorage.RouteStore.GetAllRoutes(region.Id);
            IEnumerable<StopEntity> publishedStops = await publishStorage.StopStore.GetAllStops(region.Id);

            // query the publish metadata table
            StorageManager publishMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.PublishMetadata, runId2);
            IEnumerable<PublishMetadataEntity> publishMetadata = publishMetadataStorage.PublishMetadataStore.Get(runId2);

            // query Embedded Social to get the topics
            EmbeddedSocial embeddedSocial = new EmbeddedSocial(TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            PrivateObject embeddedSocialPrivate = new PrivateObject(embeddedSocial);
            string routeName = (string)embeddedSocialPrivate.Invoke("TopicName", existingRoute);
            string routeTitle = (string)embeddedSocialPrivate.Invoke("TopicTitle", existingRoute);
            string routeText = (string)embeddedSocialPrivate.Invoke("TopicText", existingRoute);
            TopicView routeTopic = await (Task<TopicView>)embeddedSocialPrivate.Invoke("GetTopic", routeName);
            string stopName = (string)embeddedSocialPrivate.Invoke("TopicName", existingStop);
            string stopTitle = (string)embeddedSocialPrivate.Invoke("TopicTitle", existingStop);
            string stopText = (string)embeddedSocialPrivate.Invoke("TopicText", existingStop);
            TopicView stopTopic = await (Task<TopicView>)embeddedSocialPrivate.Invoke("GetTopic", stopName);

            // delete the diff table entries, publish table entries, diff metadata entries, and Embedded Social topics
            await (Task)embeddedSocialPrivate.Invoke("DeleteTopic", routeName);
            await (Task)embeddedSocialPrivate.Invoke("DeleteTopic", stopName);
            await publishMetadataStorage.PublishMetadataStore.Delete(runId1);
            await publishMetadataStorage.PublishMetadataStore.Delete(runId2);
            await publishStorage.DeleteTables();
            await diffMetadataStorage.DiffMetadataStore.Delete(runId2);
            await diffStorage.DeleteDataTables();

            // check route
            Assert.IsNotNull(publishedRoutes);
            Assert.AreEqual(publishedRoutes.Count(), 1);
            Assert.AreEqual(publishedRoutes.First().Id, existingRoute.Id);
            Assert.AreEqual(publishedRoutes.First().ShortName, existingRoute.ShortName);
            Assert.AreEqual(publishedRoutes.First().LongName, existingRoute.LongName);
            Assert.AreEqual(publishedRoutes.First().Description, existingRoute.Description);
            Assert.AreEqual(publishedRoutes.First().Url, existingRoute.Url);
            Assert.AreEqual(publishedRoutes.First().AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(publishedRoutes.First().RegionId, existingRoute.RegionId);
            Assert.AreEqual(publishedRoutes.First().RecordType, existingRoute.RecordType);
            Assert.AreEqual(publishedRoutes.First().RowState, DataRowState.Update.ToString());
            Assert.AreEqual(publishedRoutes.First().RawContent, existingRoute.RawContent);

            // check stop
            Assert.IsNotNull(publishedStops);
            Assert.AreEqual(publishedStops.Count(), 1);
            Assert.AreEqual(publishedStops.First().Id, existingStop.Id);
            Assert.AreEqual(publishedStops.First().Lat, existingStop.Lat);
            Assert.AreEqual(publishedStops.First().Lon, existingStop.Lon);
            Assert.AreEqual(publishedStops.First().Direction, existingStop.Direction);
            Assert.AreEqual(publishedStops.First().Name, existingStop.Name);
            Assert.AreEqual(publishedStops.First().Code, existingStop.Code);
            Assert.AreEqual(publishedStops.First().RegionId, existingStop.RegionId);
            Assert.AreEqual(publishedStops.First().RecordType, existingStop.RecordType);
            Assert.AreEqual(publishedStops.First().RowState, DataRowState.Update.ToString());
            Assert.AreEqual(publishedStops.First().RawContent, existingStop.RawContent);

            // check publish metadata
            Assert.IsNotNull(publishMetadata);
            List<PublishMetadataEntity> publishMetadataList = publishMetadata.ToList();
            Assert.AreEqual(publishMetadataList.Count, 2);
            int routeIndex = -1;
            int stopIndex = -1;
            if (publishMetadataList[0].RecordType == RecordType.Route.ToString())
            {
                routeIndex = 0;
                stopIndex = 1;
            }
            else
            {
                routeIndex = 1;
                stopIndex = 0;
            }

            Assert.AreEqual(publishMetadataList[routeIndex].AddedCount, 0);
            Assert.AreEqual(publishMetadataList[routeIndex].AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(publishMetadataList[routeIndex].DeletedCount, 0);
            Assert.AreEqual(publishMetadataList[routeIndex].RecordType, RecordType.Route.ToString());
            Assert.AreEqual(publishMetadataList[routeIndex].RegionId, existingRoute.RegionId);
            Assert.AreEqual(publishMetadataList[routeIndex].ResurrectedCount, 0);
            Assert.AreEqual(publishMetadataList[routeIndex].RunId, runId2);
            Assert.AreEqual(publishMetadataList[routeIndex].UpdatedCount, 1);

            Assert.AreEqual(publishMetadataList[stopIndex].AddedCount, 0);
            Assert.AreEqual(publishMetadataList[stopIndex].AgencyId, string.Empty);
            Assert.AreEqual(publishMetadataList[stopIndex].DeletedCount, 0);
            Assert.AreEqual(publishMetadataList[stopIndex].RecordType, RecordType.Stop.ToString());
            Assert.AreEqual(publishMetadataList[stopIndex].RegionId, existingStop.RegionId);
            Assert.AreEqual(publishMetadataList[stopIndex].ResurrectedCount, 0);
            Assert.AreEqual(publishMetadataList[stopIndex].RunId, runId2);
            Assert.AreEqual(publishMetadataList[stopIndex].UpdatedCount, 1);

            // check route topic view
            Assert.IsNotNull(routeTopic);
            Assert.AreEqual(routeTopic.FriendlyName, routeName);
            Assert.AreEqual(routeTopic.Text, routeText);
            Assert.AreEqual(routeTopic.Title, routeTitle);
            Assert.AreEqual(routeTopic.Categories, existingRoute.RegionId);

            // check stop topic view
            Assert.IsNotNull(stopTopic);
            Assert.AreEqual(stopTopic.FriendlyName, stopName);
            Assert.AreEqual(stopTopic.Text, stopText);
            Assert.AreEqual(stopTopic.Title, stopTitle);
            Assert.AreEqual(stopTopic.Categories, existingStop.RegionId);
        }

        /// <summary>
        /// Tests the publish manager with a deleted route and stop
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task DeleteEntities()
        {
            // this test messes with the topics in Embedded Social so it should not be run against a production service
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.EmbeddedSocialUri));

            // this test messes with the publish table so it should not be run on production accounts
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.AzureStorageConnectionString));

            string runId1 = RunId.GenerateTestRunId();

            // clean the publish table so that we are working with known state
            StorageManager publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId1);
            await TestUtilities.CleanPublishStorage(publishStorage);

            // first create a route and a stop and publish it
            StorageManager diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId1);
            await diffStorage.CreateTables();
            RegionEntity region = TestUtilities.FakeRegionEntity();
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);
            RouteEntity existingRoute = TestUtilities.FakeRouteEntity(region.Id, agency.Id);
            existingRoute.RowState = DataRowState.Create.ToString();
            await diffStorage.RouteStore.Insert(existingRoute);
            StopEntity existingStop = TestUtilities.FakeStopEntity(region.Id);
            existingStop.RowState = DataRowState.Create.ToString();
            await diffStorage.StopStore.Insert(existingStop);
            StorageManager diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId1);
            await diffMetadataStorage.CreateTables();
            DiffMetadataEntity metadata1 = new DiffMetadataEntity
            {
                RunId = runId1,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            };
            DiffMetadataEntity metadata2 = new DiffMetadataEntity
            {
                RunId = runId1,
                RegionId = region.Id,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            };
            await diffMetadataStorage.DiffMetadataStore.Insert(new List<DiffMetadataEntity> { metadata1, metadata2 });
            PublishManager publishManager = new PublishManager(TestConstants.AzureStorageConnectionString, runId1, TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            await publishManager.InitializeStorage();
            await publishManager.PublishAndStore();

            // clean up the old diff storage & metadata
            await diffStorage.DeleteDataTables();
            await diffMetadataStorage.DiffMetadataStore.Delete(runId1);

            // now create a new runId
            string runId2 = RunId.GenerateTestRunId();
            publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId2);
            diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId2);
            diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId2);
            await diffStorage.CreateTables();
            publishManager = new PublishManager(TestConstants.AzureStorageConnectionString, runId2, TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            await publishManager.InitializeStorage();

            // create diff table entries for deleted route & stop
            existingRoute.RowState = DataRowState.Delete.ToString();
            await diffStorage.RouteStore.Insert(existingRoute);
            existingStop.RowState = DataRowState.Delete.ToString();
            await diffStorage.StopStore.Insert(existingStop);

            // add diff metadata entries
            DiffMetadataEntity metadata3 = new DiffMetadataEntity
            {
                RunId = runId2,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                AddedCount = 0,
                UpdatedCount = 0,
                DeletedCount = 1,
                ResurrectedCount = 0
            };
            DiffMetadataEntity metadata4 = new DiffMetadataEntity
            {
                RunId = runId2,
                RegionId = region.Id,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = 0,
                UpdatedCount = 0,
                DeletedCount = 1,
                ResurrectedCount = 0
            };
            await diffMetadataStorage.DiffMetadataStore.Insert(new List<DiffMetadataEntity> { metadata3, metadata4 });

            // run the publish manager
            await publishManager.PublishAndStore();

            // query the publish table
            IEnumerable<RouteEntity> publishedRoutes = await publishStorage.RouteStore.GetAllRoutes(region.Id);
            IEnumerable<StopEntity> publishedStops = await publishStorage.StopStore.GetAllStops(region.Id);

            // query the publish metadata table
            StorageManager publishMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.PublishMetadata, runId2);
            IEnumerable<PublishMetadataEntity> publishMetadata = publishMetadataStorage.PublishMetadataStore.Get(runId2);

            // query Embedded Social to get the topics
            EmbeddedSocial embeddedSocial = new EmbeddedSocial(TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            PrivateObject embeddedSocialPrivate = new PrivateObject(embeddedSocial);
            string routeName = (string)embeddedSocialPrivate.Invoke("TopicName", existingRoute);
            TopicView routeTopic = await (Task<TopicView>)embeddedSocialPrivate.Invoke("GetTopic", routeName);
            string stopName = (string)embeddedSocialPrivate.Invoke("TopicName", existingStop);
            TopicView stopTopic = await (Task<TopicView>)embeddedSocialPrivate.Invoke("GetTopic", stopName);

            // delete the diff table entries, publish table entries, diff metadata entries, and Embedded Social topics
            await (Task)embeddedSocialPrivate.Invoke("DeleteTopic", routeName);
            await (Task)embeddedSocialPrivate.Invoke("DeleteTopic", stopName);
            await publishMetadataStorage.PublishMetadataStore.Delete(runId1);
            await publishMetadataStorage.PublishMetadataStore.Delete(runId2);
            await publishStorage.DeleteTables();
            await diffMetadataStorage.DiffMetadataStore.Delete(runId2);
            await diffStorage.DeleteDataTables();

            // check route
            Assert.IsNotNull(publishedRoutes);
            Assert.AreEqual(publishedRoutes.Count(), 1);
            Assert.AreEqual(publishedRoutes.First().Id, existingRoute.Id);
            Assert.AreEqual(publishedRoutes.First().ShortName, existingRoute.ShortName);
            Assert.AreEqual(publishedRoutes.First().LongName, existingRoute.LongName);
            Assert.AreEqual(publishedRoutes.First().Description, existingRoute.Description);
            Assert.AreEqual(publishedRoutes.First().Url, existingRoute.Url);
            Assert.AreEqual(publishedRoutes.First().AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(publishedRoutes.First().RegionId, existingRoute.RegionId);
            Assert.AreEqual(publishedRoutes.First().RecordType, existingRoute.RecordType);
            Assert.AreEqual(publishedRoutes.First().RowState, DataRowState.Delete.ToString());
            Assert.AreEqual(publishedRoutes.First().RawContent, existingRoute.RawContent);

            // check stop
            Assert.IsNotNull(publishedStops);
            Assert.AreEqual(publishedStops.Count(), 1);
            Assert.AreEqual(publishedStops.First().Id, existingStop.Id);
            Assert.AreEqual(publishedStops.First().Lat, existingStop.Lat);
            Assert.AreEqual(publishedStops.First().Lon, existingStop.Lon);
            Assert.AreEqual(publishedStops.First().Direction, existingStop.Direction);
            Assert.AreEqual(publishedStops.First().Name, existingStop.Name);
            Assert.AreEqual(publishedStops.First().Code, existingStop.Code);
            Assert.AreEqual(publishedStops.First().RegionId, existingStop.RegionId);
            Assert.AreEqual(publishedStops.First().RecordType, existingStop.RecordType);
            Assert.AreEqual(publishedStops.First().RowState, DataRowState.Delete.ToString());
            Assert.AreEqual(publishedStops.First().RawContent, existingStop.RawContent);

            // check publish metadata
            Assert.IsNotNull(publishMetadata);
            List<PublishMetadataEntity> publishMetadataList = publishMetadata.ToList();
            Assert.AreEqual(publishMetadataList.Count, 2);
            int routeIndex = -1;
            int stopIndex = -1;
            if (publishMetadataList[0].RecordType == RecordType.Route.ToString())
            {
                routeIndex = 0;
                stopIndex = 1;
            }
            else
            {
                routeIndex = 1;
                stopIndex = 0;
            }

            Assert.AreEqual(publishMetadataList[routeIndex].AddedCount, 0);
            Assert.AreEqual(publishMetadataList[routeIndex].AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(publishMetadataList[routeIndex].DeletedCount, 1);
            Assert.AreEqual(publishMetadataList[routeIndex].RecordType, RecordType.Route.ToString());
            Assert.AreEqual(publishMetadataList[routeIndex].RegionId, existingRoute.RegionId);
            Assert.AreEqual(publishMetadataList[routeIndex].ResurrectedCount, 0);
            Assert.AreEqual(publishMetadataList[routeIndex].RunId, runId2);
            Assert.AreEqual(publishMetadataList[routeIndex].UpdatedCount, 0);

            Assert.AreEqual(publishMetadataList[stopIndex].AddedCount, 0);
            Assert.AreEqual(publishMetadataList[stopIndex].AgencyId, string.Empty);
            Assert.AreEqual(publishMetadataList[stopIndex].DeletedCount, 1);
            Assert.AreEqual(publishMetadataList[stopIndex].RecordType, RecordType.Stop.ToString());
            Assert.AreEqual(publishMetadataList[stopIndex].RegionId, existingStop.RegionId);
            Assert.AreEqual(publishMetadataList[stopIndex].ResurrectedCount, 0);
            Assert.AreEqual(publishMetadataList[stopIndex].RunId, runId2);
            Assert.AreEqual(publishMetadataList[stopIndex].UpdatedCount, 0);

            // check route topic view
            Assert.IsNotNull(routeTopic);
            Assert.AreEqual(routeTopic.FriendlyName, routeName);
            Assert.IsTrue(routeTopic.Title.Contains("DELETED"));
            Assert.AreEqual(routeTopic.Categories, existingRoute.RegionId);

            // check stop topic view
            Assert.IsNotNull(stopTopic);
            Assert.AreEqual(stopTopic.FriendlyName, stopName);
            Assert.AreEqual(stopTopic.Categories, existingStop.RegionId);
            Assert.IsTrue(stopTopic.Title.Contains("DELETED"));
        }

        /// <summary>
        /// Tests the publish manager with a resurrected route and stop
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task ResurrectEntities()
        {
            // this test messes with the topics in Embedded Social so it should not be run against a production service
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.EmbeddedSocialUri));

            // this test messes with the publish table so it should not be run on production accounts
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.AzureStorageConnectionString));

            string runId1 = RunId.GenerateTestRunId();

            // clean the publish table so that we are working with known state
            StorageManager publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId1);
            await TestUtilities.CleanPublishStorage(publishStorage);

            // first create a route and a stop and publish it
            StorageManager diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId1);
            await diffStorage.CreateTables();
            RegionEntity region = TestUtilities.FakeRegionEntity();
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);
            RouteEntity existingRoute = TestUtilities.FakeRouteEntity(region.Id, agency.Id);
            existingRoute.RowState = DataRowState.Create.ToString();
            await diffStorage.RouteStore.Insert(existingRoute);
            StopEntity existingStop = TestUtilities.FakeStopEntity(region.Id);
            existingStop.RowState = DataRowState.Create.ToString();
            await diffStorage.StopStore.Insert(existingStop);
            StorageManager diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId1);
            await diffMetadataStorage.CreateTables();
            DiffMetadataEntity metadata1 = new DiffMetadataEntity
            {
                RunId = runId1,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            };
            DiffMetadataEntity metadata2 = new DiffMetadataEntity
            {
                RunId = runId1,
                RegionId = region.Id,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            };
            await diffMetadataStorage.DiffMetadataStore.Insert(new List<DiffMetadataEntity> { metadata1, metadata2 });
            PublishManager publishManager = new PublishManager(TestConstants.AzureStorageConnectionString, runId1, TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            await publishManager.InitializeStorage();
            await publishManager.PublishAndStore();

            // clean up the old diff storage & metadata
            await diffStorage.DeleteDataTables();
            await diffMetadataStorage.DiffMetadataStore.Delete(runId1);

            // now create a new runId
            string runId2 = RunId.GenerateTestRunId();
            publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId2);
            diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId2);
            await diffStorage.CreateTables();
            publishManager = new PublishManager(TestConstants.AzureStorageConnectionString, runId2, TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            await publishManager.InitializeStorage();

            // create diff table entries for deleted route & stop
            existingRoute.RowState = DataRowState.Delete.ToString();
            await diffStorage.RouteStore.Insert(existingRoute);
            existingStop.RowState = DataRowState.Delete.ToString();
            await diffStorage.StopStore.Insert(existingStop);

            // add diff metadata entries
            DiffMetadataEntity metadata3 = new DiffMetadataEntity
            {
                RunId = runId2,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                AddedCount = 0,
                UpdatedCount = 0,
                DeletedCount = 1,
                ResurrectedCount = 0
            };
            DiffMetadataEntity metadata4 = new DiffMetadataEntity
            {
                RunId = runId2,
                RegionId = region.Id,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = 0,
                UpdatedCount = 0,
                DeletedCount = 1,
                ResurrectedCount = 0
            };
            await diffMetadataStorage.DiffMetadataStore.Insert(new List<DiffMetadataEntity> { metadata3, metadata4 });

            // run the publish manager
            await publishManager.PublishAndStore();

            // clean up the old diff storage & metadata
            await diffStorage.DeleteDataTables();
            await diffMetadataStorage.DiffMetadataStore.Delete(runId2);

            // now create a new runId
            string runId3 = RunId.GenerateTestRunId();
            publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId3);
            diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId3);
            diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId3);
            await diffStorage.CreateTables();
            publishManager = new PublishManager(TestConstants.AzureStorageConnectionString, runId3, TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            await publishManager.InitializeStorage();

            // create diff table entries for resurrected route & stop
            existingRoute.RowState = DataRowState.Resurrect.ToString();
            await diffStorage.RouteStore.Insert(existingRoute);
            existingStop.RowState = DataRowState.Resurrect.ToString();
            await diffStorage.StopStore.Insert(existingStop);

            // add diff metadata entries
            DiffMetadataEntity metadata5 = new DiffMetadataEntity
            {
                RunId = runId3,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                AddedCount = 0,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 1
            };
            DiffMetadataEntity metadata6 = new DiffMetadataEntity
            {
                RunId = runId3,
                RegionId = region.Id,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = 0,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 1
            };
            await diffMetadataStorage.DiffMetadataStore.Insert(new List<DiffMetadataEntity> { metadata5, metadata6 });

            // run the publish manager
            await publishManager.PublishAndStore();

            // query the publish table
            IEnumerable<RouteEntity> publishedRoutes = await publishStorage.RouteStore.GetAllRoutes(region.Id);
            IEnumerable<StopEntity> publishedStops = await publishStorage.StopStore.GetAllStops(region.Id);

            // query the publish metadata table
            StorageManager publishMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.PublishMetadata, runId3);
            IEnumerable<PublishMetadataEntity> publishMetadata = publishMetadataStorage.PublishMetadataStore.Get(runId3);

            // query Embedded Social to get the topics
            EmbeddedSocial embeddedSocial = new EmbeddedSocial(TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            PrivateObject embeddedSocialPrivate = new PrivateObject(embeddedSocial);
            string routeName = (string)embeddedSocialPrivate.Invoke("TopicName", existingRoute);
            TopicView routeTopic = await (Task<TopicView>)embeddedSocialPrivate.Invoke("GetTopic", routeName);
            string stopName = (string)embeddedSocialPrivate.Invoke("TopicName", existingStop);
            TopicView stopTopic = await (Task<TopicView>)embeddedSocialPrivate.Invoke("GetTopic", stopName);

            // delete the diff table entries, publish table entries, diff metadata entries, and Embedded Social topics
            await (Task)embeddedSocialPrivate.Invoke("DeleteTopic", routeName);
            await (Task)embeddedSocialPrivate.Invoke("DeleteTopic", stopName);
            await publishMetadataStorage.PublishMetadataStore.Delete(runId1);
            await publishMetadataStorage.PublishMetadataStore.Delete(runId2);
            await publishMetadataStorage.PublishMetadataStore.Delete(runId3);
            await publishStorage.DeleteTables();
            await diffMetadataStorage.DiffMetadataStore.Delete(runId3);
            await diffStorage.DeleteDataTables();

            // check route
            Assert.IsNotNull(publishedRoutes);
            Assert.AreEqual(publishedRoutes.Count(), 1);
            Assert.AreEqual(publishedRoutes.First().Id, existingRoute.Id);
            Assert.AreEqual(publishedRoutes.First().ShortName, existingRoute.ShortName);
            Assert.AreEqual(publishedRoutes.First().LongName, existingRoute.LongName);
            Assert.AreEqual(publishedRoutes.First().Description, existingRoute.Description);
            Assert.AreEqual(publishedRoutes.First().Url, existingRoute.Url);
            Assert.AreEqual(publishedRoutes.First().AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(publishedRoutes.First().RegionId, existingRoute.RegionId);
            Assert.AreEqual(publishedRoutes.First().RecordType, existingRoute.RecordType);
            Assert.AreEqual(publishedRoutes.First().RowState, DataRowState.Resurrect.ToString());
            Assert.AreEqual(publishedRoutes.First().RawContent, existingRoute.RawContent);

            // check stop
            Assert.IsNotNull(publishedStops);
            Assert.AreEqual(publishedStops.Count(), 1);
            Assert.AreEqual(publishedStops.First().Id, existingStop.Id);
            Assert.AreEqual(publishedStops.First().Lat, existingStop.Lat);
            Assert.AreEqual(publishedStops.First().Lon, existingStop.Lon);
            Assert.AreEqual(publishedStops.First().Direction, existingStop.Direction);
            Assert.AreEqual(publishedStops.First().Name, existingStop.Name);
            Assert.AreEqual(publishedStops.First().Code, existingStop.Code);
            Assert.AreEqual(publishedStops.First().RegionId, existingStop.RegionId);
            Assert.AreEqual(publishedStops.First().RecordType, existingStop.RecordType);
            Assert.AreEqual(publishedStops.First().RowState, DataRowState.Resurrect.ToString());
            Assert.AreEqual(publishedStops.First().RawContent, existingStop.RawContent);

            // check publish metadata
            Assert.IsNotNull(publishMetadata);
            List<PublishMetadataEntity> publishMetadataList = publishMetadata.ToList();
            Assert.AreEqual(publishMetadataList.Count, 2);
            int routeIndex = -1;
            int stopIndex = -1;
            if (publishMetadataList[0].RecordType == RecordType.Route.ToString())
            {
                routeIndex = 0;
                stopIndex = 1;
            }
            else
            {
                routeIndex = 1;
                stopIndex = 0;
            }

            Assert.AreEqual(publishMetadataList[routeIndex].AddedCount, 0);
            Assert.AreEqual(publishMetadataList[routeIndex].AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(publishMetadataList[routeIndex].DeletedCount, 0);
            Assert.AreEqual(publishMetadataList[routeIndex].RecordType, RecordType.Route.ToString());
            Assert.AreEqual(publishMetadataList[routeIndex].RegionId, existingRoute.RegionId);
            Assert.AreEqual(publishMetadataList[routeIndex].ResurrectedCount, 1);
            Assert.AreEqual(publishMetadataList[routeIndex].RunId, runId3);
            Assert.AreEqual(publishMetadataList[routeIndex].UpdatedCount, 0);

            Assert.AreEqual(publishMetadataList[stopIndex].AddedCount, 0);
            Assert.AreEqual(publishMetadataList[stopIndex].AgencyId, string.Empty);
            Assert.AreEqual(publishMetadataList[stopIndex].DeletedCount, 0);
            Assert.AreEqual(publishMetadataList[stopIndex].RecordType, RecordType.Stop.ToString());
            Assert.AreEqual(publishMetadataList[stopIndex].RegionId, existingStop.RegionId);
            Assert.AreEqual(publishMetadataList[stopIndex].ResurrectedCount, 1);
            Assert.AreEqual(publishMetadataList[stopIndex].RunId, runId3);
            Assert.AreEqual(publishMetadataList[stopIndex].UpdatedCount, 0);

            // check route topic view
            Assert.IsNotNull(routeTopic);
            Assert.AreEqual(routeTopic.FriendlyName, routeName);
            Assert.IsFalse(routeTopic.Title.Contains("DELETED"));
            Assert.AreEqual(routeTopic.Categories, existingRoute.RegionId);

            // check stop topic view
            Assert.IsNotNull(stopTopic);
            Assert.AreEqual(stopTopic.FriendlyName, stopName);
            Assert.AreEqual(stopTopic.Categories, existingStop.RegionId);
            Assert.IsFalse(stopTopic.Title.Contains("DELETED"));
        }

        /// <summary>
        /// Tests hashtag removal
        /// </summary>
        [TestMethod]
        public void HashTagRemoval()
        {
            // create a route with hashes in the names
            RegionEntity region = TestUtilities.FakeRegionEntity();
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);
            RouteEntity newRoute = TestUtilities.FakeRouteEntity(region.Id, agency.Id);
            newRoute.ShortName = "#Hello # abc#def #123 ##";
            newRoute.LongName = "Testing hello #";

            // construct the topic title and text
            EmbeddedSocial embeddedSocial = new EmbeddedSocial(TestConstants.EmbeddedSocialUri, TestConstants.EmbeddedSocialAppKey, TestConstants.EmbeddedSocialAADToken, TestConstants.EmbeddedSocialAdminUserHandle);
            PrivateObject embeddedSocialPrivate = new PrivateObject(embeddedSocial);
            string routeTitle = (string)embeddedSocialPrivate.Invoke("TopicTitle", newRoute);
            string routeText = (string)embeddedSocialPrivate.Invoke("TopicText", newRoute);

            // check the title and texts
            Assert.IsTrue(routeTitle.Equals("# Hello # abc#def # 123 # # - Testing hello #"));
            Assert.IsTrue(routeText.Equals("Discuss the Testing hello # route"));
        }
    }
}