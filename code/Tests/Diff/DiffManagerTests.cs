// <copyright file="DiffManagerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests.Diff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.Diff;
    using OBAService.Storage;
    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Tests the diff manager class
    /// </summary>
    [TestClass]
    public class DiffManagerTests
    {
        /// <summary>
        /// Tests the diff manager with a new route and stop
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task CreateEntities()
        {
            // this test messes with the publish table so it should not be run on production accounts
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.AzureStorageConnectionString));

            string runId = RunId.GenerateTestRunId();

            // clean the publish table so that we are working with known state
            StorageManager publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId);
            await TestUtilities.CleanPublishStorage(publishStorage);

            // setup download storage
            StorageManager downloadStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, runId);
            await downloadStorage.CreateTables();

            // create region
            RegionEntity region = TestUtilities.FakeRegionEntity();
            await downloadStorage.RegionStore.Insert(new List<RegionEntity>() { region });

            // create agency
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);
            await downloadStorage.AgencyStore.Insert(new List<AgencyEntity>() { agency });

            // create 1 new route and 1 new stop in download table that do not exist in the publish table
            RouteEntity newRoute = TestUtilities.FakeRouteEntity(region.Id, agency.Id);
            await downloadStorage.RouteStore.Insert(newRoute);
            StopEntity newStop = TestUtilities.FakeStopEntity(region.Id);
            await downloadStorage.StopStore.Insert(newStop);

            // setup diff manager
            DiffManager diffManager = new DiffManager(TestConstants.AzureStorageConnectionString, runId);
            await diffManager.InitializeStorage();

            // execute diff manager
            await diffManager.DiffAndStore();

            // sleep between the insert and the query otherwise Azure sometimes returns no records
            Thread.Sleep(TestConstants.AzureTableDelay);

            // query results
            StorageManager diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId);
            IEnumerable<RouteEntity> diffRoutes = await diffStorage.RouteStore.GetAllRoutes(newRoute.RegionId);
            IEnumerable<StopEntity> diffStops = await diffStorage.StopStore.GetAllStops(newStop.RegionId);
            StorageManager diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
            List<DiffMetadataEntity> diffMetadata = diffMetadataStorage.DiffMetadataStore.Get(runId).ToList();

            // clean up
            await diffManager.DeleteDiff();
            await downloadStorage.DeleteDataTables();
            await publishStorage.DeleteTables();

            // check route
            Assert.IsNotNull(diffRoutes);
            Assert.AreEqual(diffRoutes.Count(), 1);
            Assert.AreEqual(diffRoutes.First().Id, newRoute.Id);
            Assert.AreEqual(diffRoutes.First().ShortName, newRoute.ShortName);
            Assert.AreEqual(diffRoutes.First().LongName, newRoute.LongName);
            Assert.AreEqual(diffRoutes.First().Description, newRoute.Description);
            Assert.AreEqual(diffRoutes.First().Url, newRoute.Url);
            Assert.AreEqual(diffRoutes.First().AgencyId, newRoute.AgencyId);
            Assert.AreEqual(diffRoutes.First().RegionId, newRoute.RegionId);
            Assert.AreEqual(diffRoutes.First().RecordType, newRoute.RecordType);
            Assert.AreEqual(diffRoutes.First().RowState, DataRowState.Create.ToString());
            Assert.AreEqual(diffRoutes.First().RawContent, newRoute.RawContent);

            // check stop
            Assert.IsNotNull(diffStops);
            Assert.AreEqual(diffStops.Count(), 1);
            Assert.AreEqual(diffStops.First().Id, newStop.Id);
            Assert.AreEqual(diffStops.First().Lat, newStop.Lat);
            Assert.AreEqual(diffStops.First().Lon, newStop.Lon);
            Assert.AreEqual(diffStops.First().Direction, newStop.Direction);
            Assert.AreEqual(diffStops.First().Name, newStop.Name);
            Assert.AreEqual(diffStops.First().Code, newStop.Code);
            Assert.AreEqual(diffStops.First().RegionId, newStop.RegionId);
            Assert.AreEqual(diffStops.First().RecordType, newStop.RecordType);
            Assert.AreEqual(diffStops.First().RowState, DataRowState.Create.ToString());
            Assert.AreEqual(diffStops.First().RawContent, newStop.RawContent);

            // check metadata
            Assert.IsNotNull(diffMetadata);
            Assert.AreEqual(diffMetadata.Count, 2);
            int routeIndex = -1;
            int stopIndex = -1;
            if (diffMetadata[0].RecordType == RecordType.Route.ToString())
            {
                routeIndex = 0;
                stopIndex = 1;
            }
            else
            {
                routeIndex = 1;
                stopIndex = 0;
            }

            Assert.AreEqual(diffMetadata[routeIndex].AddedCount, 1);
            Assert.AreEqual(diffMetadata[routeIndex].AgencyId, newRoute.AgencyId);
            Assert.AreEqual(diffMetadata[routeIndex].DeletedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].RecordType, RecordType.Route.ToString());
            Assert.AreEqual(diffMetadata[routeIndex].RegionId, newRoute.RegionId);
            Assert.AreEqual(diffMetadata[routeIndex].ResurrectedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].RunId, runId);
            Assert.AreEqual(diffMetadata[routeIndex].UpdatedCount, 0);

            Assert.AreEqual(diffMetadata[stopIndex].AddedCount, 1);
            Assert.AreEqual(diffMetadata[stopIndex].AgencyId, string.Empty);
            Assert.AreEqual(diffMetadata[stopIndex].DeletedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].RecordType, RecordType.Stop.ToString());
            Assert.AreEqual(diffMetadata[stopIndex].RegionId, newStop.RegionId);
            Assert.AreEqual(diffMetadata[stopIndex].ResurrectedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].RunId, runId);
            Assert.AreEqual(diffMetadata[stopIndex].UpdatedCount, 0);
        }

        /// <summary>
        /// Tests the diff manager with an unchanged route and stop
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task UnchangedEntities()
        {
            // this test messes with the publish table so it should not be run on production accounts
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.AzureStorageConnectionString));

            string runId = RunId.GenerateTestRunId();

            // clean the publish table so that we are working with known state
            StorageManager publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId);
            await TestUtilities.CleanPublishStorage(publishStorage);

            // setup download storage
            StorageManager downloadStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, runId);
            await downloadStorage.CreateTables();

            // create region
            RegionEntity region = TestUtilities.FakeRegionEntity();
            await downloadStorage.RegionStore.Insert(new List<RegionEntity>() { region });

            // create agency
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);
            await downloadStorage.AgencyStore.Insert(new List<AgencyEntity>() { agency });

            // create 1 route and 1 stop in download table that also exist in the publish table
            RouteEntity existingRoute = TestUtilities.FakeRouteEntity(region.Id, agency.Id);
            await downloadStorage.RouteStore.Insert(existingRoute);
            existingRoute.RowState = DataRowState.Create.ToString();
            await publishStorage.RouteStore.Insert(existingRoute);
            StopEntity existingStop = TestUtilities.FakeStopEntity(region.Id);
            await downloadStorage.StopStore.Insert(existingStop);
            existingStop.RowState = DataRowState.Create.ToString();
            await publishStorage.StopStore.Insert(existingStop);

            // setup diff manager
            DiffManager diffManager = new DiffManager(TestConstants.AzureStorageConnectionString, runId);
            await diffManager.InitializeStorage();

            // execute diff manager
            await diffManager.DiffAndStore();

            // sleep between the insert and the query otherwise Azure sometimes returns no records
            Thread.Sleep(TestConstants.AzureTableDelay);

            // query results
            StorageManager diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId);
            IEnumerable<RouteEntity> diffRoutes = await diffStorage.RouteStore.GetAllRoutes(existingRoute.RegionId);
            IEnumerable<StopEntity> diffStops = await diffStorage.StopStore.GetAllStops(existingStop.RegionId);
            StorageManager diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
            List<DiffMetadataEntity> diffMetadata = diffMetadataStorage.DiffMetadataStore.Get(runId).ToList();

            // clean up
            await diffManager.DeleteDiff();
            await downloadStorage.DeleteDataTables();
            await publishStorage.DeleteTables();

            // check results
            Assert.AreEqual(diffRoutes.Count(), 0);
            Assert.AreEqual(diffStops.Count(), 0);

            // check metadata
            Assert.IsNotNull(diffMetadata);
            Assert.AreEqual(diffMetadata.Count, 2);
            int routeIndex = -1;
            int stopIndex = -1;
            if (diffMetadata[0].RecordType == RecordType.Route.ToString())
            {
                routeIndex = 0;
                stopIndex = 1;
            }
            else
            {
                routeIndex = 1;
                stopIndex = 0;
            }

            Assert.AreEqual(diffMetadata[routeIndex].AddedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(diffMetadata[routeIndex].DeletedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].RecordType, RecordType.Route.ToString());
            Assert.AreEqual(diffMetadata[routeIndex].RegionId, existingRoute.RegionId);
            Assert.AreEqual(diffMetadata[routeIndex].ResurrectedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].RunId, runId);
            Assert.AreEqual(diffMetadata[routeIndex].UpdatedCount, 0);

            Assert.AreEqual(diffMetadata[stopIndex].AddedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].AgencyId, string.Empty);
            Assert.AreEqual(diffMetadata[stopIndex].DeletedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].RecordType, RecordType.Stop.ToString());
            Assert.AreEqual(diffMetadata[stopIndex].RegionId, existingStop.RegionId);
            Assert.AreEqual(diffMetadata[stopIndex].ResurrectedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].RunId, runId);
            Assert.AreEqual(diffMetadata[stopIndex].UpdatedCount, 0);
        }

        /// <summary>
        /// Tests the diff manager with a changed route and stop
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task UpdateEntities()
        {
            // this test messes with the publish table so it should not be run on production accounts
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.AzureStorageConnectionString));

            string runId = RunId.GenerateTestRunId();

            // clean the publish table so that we are working with known state
            StorageManager publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId);
            await TestUtilities.CleanPublishStorage(publishStorage);

            // setup download storage
            StorageManager downloadStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, runId);
            await downloadStorage.CreateTables();

            // create region
            RegionEntity region = TestUtilities.FakeRegionEntity();
            await downloadStorage.RegionStore.Insert(new List<RegionEntity>() { region });

            // create agency
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);
            await downloadStorage.AgencyStore.Insert(new List<AgencyEntity>() { agency });

            // create 1 route and 1 stop in download table that also exist in the publish table with slightly different values
            RouteEntity existingRoute = TestUtilities.FakeRouteEntity(region.Id, agency.Id);
            await downloadStorage.RouteStore.Insert(existingRoute);
            existingRoute.ShortName = Guid.NewGuid().ToString();
            existingRoute.RowState = DataRowState.Create.ToString();
            await publishStorage.RouteStore.Insert(existingRoute);
            StopEntity existingStop = TestUtilities.FakeStopEntity(region.Id);
            await downloadStorage.StopStore.Insert(existingStop);
            existingStop.Name = Guid.NewGuid().ToString();
            existingStop.RowState = DataRowState.Create.ToString();
            await publishStorage.StopStore.Insert(existingStop);

            // setup diff manager
            DiffManager diffManager = new DiffManager(TestConstants.AzureStorageConnectionString, runId);
            await diffManager.InitializeStorage();

            // execute diff manager
            await diffManager.DiffAndStore();

            // sleep between the insert and the query otherwise Azure sometimes returns no records
            Thread.Sleep(TestConstants.AzureTableDelay);

            // query results
            StorageManager diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId);
            IEnumerable<RouteEntity> diffRoutes = await diffStorage.RouteStore.GetAllRoutes(existingRoute.RegionId);
            IEnumerable<StopEntity> diffStops = await diffStorage.StopStore.GetAllStops(existingStop.RegionId);
            StorageManager diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
            List<DiffMetadataEntity> diffMetadata = diffMetadataStorage.DiffMetadataStore.Get(runId).ToList();

            // clean up
            await diffManager.DeleteDiff();
            await downloadStorage.DeleteDataTables();
            await publishStorage.DeleteTables();

            // check route
            Assert.IsNotNull(diffRoutes);
            Assert.AreEqual(diffRoutes.Count(), 1);
            Assert.AreEqual(diffRoutes.First().Id, existingRoute.Id);
            Assert.AreNotEqual(diffRoutes.First().ShortName, existingRoute.ShortName);
            Assert.AreEqual(diffRoutes.First().LongName, existingRoute.LongName);
            Assert.AreEqual(diffRoutes.First().Description, existingRoute.Description);
            Assert.AreEqual(diffRoutes.First().Url, existingRoute.Url);
            Assert.AreEqual(diffRoutes.First().AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(diffRoutes.First().RegionId, existingRoute.RegionId);
            Assert.AreEqual(diffRoutes.First().RecordType, existingRoute.RecordType);
            Assert.AreEqual(diffRoutes.First().RowState, DataRowState.Update.ToString());
            Assert.AreEqual(diffRoutes.First().RawContent, existingRoute.RawContent);

            // check stop
            Assert.IsNotNull(diffStops);
            Assert.AreEqual(diffStops.Count(), 1);
            Assert.AreEqual(diffStops.First().Id, existingStop.Id);
            Assert.AreEqual(diffStops.First().Lat, existingStop.Lat);
            Assert.AreEqual(diffStops.First().Lon, existingStop.Lon);
            Assert.AreEqual(diffStops.First().Direction, existingStop.Direction);
            Assert.AreNotEqual(diffStops.First().Name, existingStop.Name);
            Assert.AreEqual(diffStops.First().Code, existingStop.Code);
            Assert.AreEqual(diffStops.First().RegionId, existingStop.RegionId);
            Assert.AreEqual(diffStops.First().RecordType, existingStop.RecordType);
            Assert.AreEqual(diffStops.First().RowState, DataRowState.Update.ToString());
            Assert.AreEqual(diffStops.First().RawContent, existingStop.RawContent);

            // check metadata
            Assert.IsNotNull(diffMetadata);
            Assert.AreEqual(diffMetadata.Count, 2);
            int routeIndex = -1;
            int stopIndex = -1;
            if (diffMetadata[0].RecordType == RecordType.Route.ToString())
            {
                routeIndex = 0;
                stopIndex = 1;
            }
            else
            {
                routeIndex = 1;
                stopIndex = 0;
            }

            Assert.AreEqual(diffMetadata[routeIndex].AddedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(diffMetadata[routeIndex].DeletedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].RecordType, RecordType.Route.ToString());
            Assert.AreEqual(diffMetadata[routeIndex].RegionId, existingRoute.RegionId);
            Assert.AreEqual(diffMetadata[routeIndex].ResurrectedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].RunId, runId);
            Assert.AreEqual(diffMetadata[routeIndex].UpdatedCount, 1);

            Assert.AreEqual(diffMetadata[stopIndex].AddedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].AgencyId, string.Empty);
            Assert.AreEqual(diffMetadata[stopIndex].DeletedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].RecordType, RecordType.Stop.ToString());
            Assert.AreEqual(diffMetadata[stopIndex].RegionId, existingStop.RegionId);
            Assert.AreEqual(diffMetadata[stopIndex].ResurrectedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].RunId, runId);
            Assert.AreEqual(diffMetadata[stopIndex].UpdatedCount, 1);
        }

        /// <summary>
        /// Tests the diff manager with a deleted route and stop
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task DeleteEntities()
        {
            // this test messes with the publish table so it should not be run on production accounts
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.AzureStorageConnectionString));

            string runId = RunId.GenerateTestRunId();

            // clean the publish table so that we are working with known state
            StorageManager publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId);
            await TestUtilities.CleanPublishStorage(publishStorage);

            // setup download storage
            StorageManager downloadStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, runId);
            await downloadStorage.CreateTables();

            // create region
            RegionEntity region = TestUtilities.FakeRegionEntity();
            await downloadStorage.RegionStore.Insert(new List<RegionEntity>() { region });

            // create agency
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);
            await downloadStorage.AgencyStore.Insert(new List<AgencyEntity>() { agency });

            // create 1 route and 1 stop in the publish table
            RouteEntity existingRoute = TestUtilities.FakeRouteEntity(region.Id, agency.Id);
            existingRoute.RowState = DataRowState.Create.ToString();
            await publishStorage.RouteStore.Insert(existingRoute);
            StopEntity existingStop = TestUtilities.FakeStopEntity(region.Id);
            existingStop.RowState = DataRowState.Create.ToString();
            await publishStorage.StopStore.Insert(existingStop);

            // setup diff manager
            DiffManager diffManager = new DiffManager(TestConstants.AzureStorageConnectionString, runId);
            await diffManager.InitializeStorage();

            // execute diff manager
            await diffManager.DiffAndStore();

            // sleep between the insert and the query otherwise Azure sometimes returns no records
            Thread.Sleep(TestConstants.AzureTableDelay);

            // query results
            StorageManager diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId);
            IEnumerable<RouteEntity> diffRoutes = await diffStorage.RouteStore.GetAllRoutes(existingRoute.RegionId);
            IEnumerable<StopEntity> diffStops = await diffStorage.StopStore.GetAllStops(existingStop.RegionId);
            StorageManager diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
            List<DiffMetadataEntity> diffMetadata = diffMetadataStorage.DiffMetadataStore.Get(runId).ToList();

            // clean up
            await diffManager.DeleteDiff();
            await downloadStorage.DeleteDataTables();
            await publishStorage.DeleteTables();

            // check route
            Assert.IsNotNull(diffRoutes);
            Assert.AreEqual(diffRoutes.Count(), 1);
            Assert.AreEqual(diffRoutes.First().Id, existingRoute.Id);
            Assert.AreEqual(diffRoutes.First().ShortName, existingRoute.ShortName);
            Assert.AreEqual(diffRoutes.First().LongName, existingRoute.LongName);
            Assert.AreEqual(diffRoutes.First().Description, existingRoute.Description);
            Assert.AreEqual(diffRoutes.First().Url, existingRoute.Url);
            Assert.AreEqual(diffRoutes.First().AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(diffRoutes.First().RegionId, existingRoute.RegionId);
            Assert.AreEqual(diffRoutes.First().RecordType, existingRoute.RecordType);
            Assert.AreEqual(diffRoutes.First().RowState, DataRowState.Delete.ToString());
            Assert.AreEqual(diffRoutes.First().RawContent, existingRoute.RawContent);

            // check stop
            Assert.IsNotNull(diffStops);
            Assert.AreEqual(diffStops.Count(), 1);
            Assert.AreEqual(diffStops.First().Id, existingStop.Id);
            Assert.AreEqual(diffStops.First().Lat, existingStop.Lat);
            Assert.AreEqual(diffStops.First().Lon, existingStop.Lon);
            Assert.AreEqual(diffStops.First().Direction, existingStop.Direction);
            Assert.AreEqual(diffStops.First().Name, existingStop.Name);
            Assert.AreEqual(diffStops.First().Code, existingStop.Code);
            Assert.AreEqual(diffStops.First().RegionId, existingStop.RegionId);
            Assert.AreEqual(diffStops.First().RecordType, existingStop.RecordType);
            Assert.AreEqual(diffStops.First().RowState, DataRowState.Delete.ToString());
            Assert.AreEqual(diffStops.First().RawContent, existingStop.RawContent);

            // check metadata
            Assert.IsNotNull(diffMetadata);
            Assert.AreEqual(diffMetadata.Count, 2);
            int routeIndex = -1;
            int stopIndex = -1;
            if (diffMetadata[0].RecordType == RecordType.Route.ToString())
            {
                routeIndex = 0;
                stopIndex = 1;
            }
            else
            {
                routeIndex = 1;
                stopIndex = 0;
            }

            Assert.AreEqual(diffMetadata[routeIndex].AddedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(diffMetadata[routeIndex].DeletedCount, 1);
            Assert.AreEqual(diffMetadata[routeIndex].RecordType, RecordType.Route.ToString());
            Assert.AreEqual(diffMetadata[routeIndex].RegionId, existingRoute.RegionId);
            Assert.AreEqual(diffMetadata[routeIndex].ResurrectedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].RunId, runId);
            Assert.AreEqual(diffMetadata[routeIndex].UpdatedCount, 0);

            Assert.AreEqual(diffMetadata[stopIndex].AddedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].AgencyId, string.Empty);
            Assert.AreEqual(diffMetadata[stopIndex].DeletedCount, 1);
            Assert.AreEqual(diffMetadata[stopIndex].RecordType, RecordType.Stop.ToString());
            Assert.AreEqual(diffMetadata[stopIndex].RegionId, existingStop.RegionId);
            Assert.AreEqual(diffMetadata[stopIndex].ResurrectedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].RunId, runId);
            Assert.AreEqual(diffMetadata[stopIndex].UpdatedCount, 0);
        }

        /// <summary>
        /// Tests the diff manager with a resurrected route and stop
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task ResurrectEntities()
        {
            // this test messes with the publish table so it should not be run on production accounts
            Assert.IsFalse(Utils.ProdConfiguration.IsProduction(TestConstants.AzureStorageConnectionString));

            string runId = RunId.GenerateTestRunId();

            // clean the publish table so that we are working with known state
            StorageManager publishStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Publish, runId);
            await TestUtilities.CleanPublishStorage(publishStorage);

            // setup download storage
            StorageManager downloadStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Download, runId);
            await downloadStorage.CreateTables();

            // create region
            RegionEntity region = TestUtilities.FakeRegionEntity();
            await downloadStorage.RegionStore.Insert(new List<RegionEntity>() { region });

            // create agency
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);
            await downloadStorage.AgencyStore.Insert(new List<AgencyEntity>() { agency });

            // create 1 route and 1 stop in download table that also exist in the publish table as deleted entities
            RouteEntity existingRoute = TestUtilities.FakeRouteEntity(region.Id, agency.Id);
            await downloadStorage.RouteStore.Insert(existingRoute);
            existingRoute.RowState = DataRowState.Delete.ToString();
            await publishStorage.RouteStore.Insert(existingRoute);
            StopEntity existingStop = TestUtilities.FakeStopEntity(region.Id);
            await downloadStorage.StopStore.Insert(existingStop);
            existingStop.RowState = DataRowState.Delete.ToString();
            await publishStorage.StopStore.Insert(existingStop);

            // setup diff manager
            DiffManager diffManager = new DiffManager(TestConstants.AzureStorageConnectionString, runId);
            await diffManager.InitializeStorage();

            // execute diff manager
            await diffManager.DiffAndStore();

            // sleep between the insert and the query otherwise Azure sometimes returns no records
            Thread.Sleep(TestConstants.AzureTableDelay);

            // query results
            StorageManager diffStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.Diff, runId);
            IEnumerable<RouteEntity> diffRoutes = await diffStorage.RouteStore.GetAllRoutes(existingRoute.RegionId);
            IEnumerable<StopEntity> diffStops = await diffStorage.StopStore.GetAllStops(existingStop.RegionId);
            StorageManager diffMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
            List<DiffMetadataEntity> diffMetadata = diffMetadataStorage.DiffMetadataStore.Get(runId).ToList();

            // clean up
            await diffManager.DeleteDiff();
            await downloadStorage.DeleteDataTables();
            await publishStorage.DeleteTables();

            // check route
            Assert.IsNotNull(diffRoutes);
            Assert.AreEqual(diffRoutes.Count(), 1);
            Assert.AreEqual(diffRoutes.First().Id, existingRoute.Id);
            Assert.AreEqual(diffRoutes.First().ShortName, existingRoute.ShortName);
            Assert.AreEqual(diffRoutes.First().LongName, existingRoute.LongName);
            Assert.AreEqual(diffRoutes.First().Description, existingRoute.Description);
            Assert.AreEqual(diffRoutes.First().Url, existingRoute.Url);
            Assert.AreEqual(diffRoutes.First().AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(diffRoutes.First().RegionId, existingRoute.RegionId);
            Assert.AreEqual(diffRoutes.First().RecordType, existingRoute.RecordType);
            Assert.AreEqual(diffRoutes.First().RowState, DataRowState.Resurrect.ToString());
            Assert.AreEqual(diffRoutes.First().RawContent, existingRoute.RawContent);

            // check stop
            Assert.IsNotNull(diffStops);
            Assert.AreEqual(diffStops.Count(), 1);
            Assert.AreEqual(diffStops.First().Id, existingStop.Id);
            Assert.AreEqual(diffStops.First().Lat, existingStop.Lat);
            Assert.AreEqual(diffStops.First().Lon, existingStop.Lon);
            Assert.AreEqual(diffStops.First().Direction, existingStop.Direction);
            Assert.AreEqual(diffStops.First().Name, existingStop.Name);
            Assert.AreEqual(diffStops.First().Code, existingStop.Code);
            Assert.AreEqual(diffStops.First().RegionId, existingStop.RegionId);
            Assert.AreEqual(diffStops.First().RecordType, existingStop.RecordType);
            Assert.AreEqual(diffStops.First().RowState, DataRowState.Resurrect.ToString());
            Assert.AreEqual(diffStops.First().RawContent, existingStop.RawContent);

            // check metadata
            Assert.IsNotNull(diffMetadata);
            Assert.AreEqual(diffMetadata.Count, 2);
            int routeIndex = -1;
            int stopIndex = -1;
            if (diffMetadata[0].RecordType == RecordType.Route.ToString())
            {
                routeIndex = 0;
                stopIndex = 1;
            }
            else
            {
                routeIndex = 1;
                stopIndex = 0;
            }

            Assert.AreEqual(diffMetadata[routeIndex].AddedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].AgencyId, existingRoute.AgencyId);
            Assert.AreEqual(diffMetadata[routeIndex].DeletedCount, 0);
            Assert.AreEqual(diffMetadata[routeIndex].RecordType, RecordType.Route.ToString());
            Assert.AreEqual(diffMetadata[routeIndex].RegionId, existingRoute.RegionId);
            Assert.AreEqual(diffMetadata[routeIndex].ResurrectedCount, 1);
            Assert.AreEqual(diffMetadata[routeIndex].RunId, runId);
            Assert.AreEqual(diffMetadata[routeIndex].UpdatedCount, 0);

            Assert.AreEqual(diffMetadata[stopIndex].AddedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].AgencyId, string.Empty);
            Assert.AreEqual(diffMetadata[stopIndex].DeletedCount, 0);
            Assert.AreEqual(diffMetadata[stopIndex].RecordType, RecordType.Stop.ToString());
            Assert.AreEqual(diffMetadata[stopIndex].RegionId, existingStop.RegionId);
            Assert.AreEqual(diffMetadata[stopIndex].ResurrectedCount, 1);
            Assert.AreEqual(diffMetadata[stopIndex].RunId, runId);
            Assert.AreEqual(diffMetadata[stopIndex].UpdatedCount, 0);
        }
    }
}