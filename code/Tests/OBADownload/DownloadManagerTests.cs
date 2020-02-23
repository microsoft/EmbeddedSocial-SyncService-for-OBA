// <copyright file="DownloadManagerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests.OBADownload
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.OBADownload;
    using OBAService.Storage;
    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Tests the download manager class
    /// </summary>
    [TestClass]
    public class DownloadManagerTests
    {
        /// <summary>
        /// Tests the download and store of OBA data
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task DownloadAndStore()
        {
            // create download manager & have it download & store data
            string runId = RunId.GenerateTestRunId();
            DownloadManager downloadManager = new DownloadManager(
                                                                  TestConstants.AzureStorageConnectionString,
                                                                  runId,
                                                                  TestConstants.OBAApiKey,
                                                                  TestConstants.OBARegionsListUri);
            await downloadManager.InitializeStorage();
            await downloadManager.DownloadAndStore();

            // fetch data for later verification
            StorageManager downloadStorage = new StorageManager(TestConstants.AzureStorageConnectionString,  TableNames.TableType.Download, runId);
            StorageManager downloadMetadataStorage = new StorageManager(TestConstants.AzureStorageConnectionString, TableNames.TableType.DownloadMetadata, runId);
            IEnumerable<DownloadMetadataEntity> metadataEntities = downloadMetadataStorage.DownloadMetadataStore.Get(runId);
            RegionsListEntity regionsListEntity = downloadStorage.RegionsListStore.Get();
            List<RegionEntity> regionEntities = downloadStorage.RegionStore.GetAllRegions().ToList();
            List<AgencyEntity> agencyEntities = null;
            List<RouteEntity> routeEntities = null;
            List<StopEntity> stopEntities = null;
            int regionEntityIndex = -1;
            if (regionEntities != null && regionEntities.Count > 0)
            {
                // pick a random region entity
                regionEntityIndex = new Random().Next(regionEntities.Count);

                // fetch all agency entities for that region
                agencyEntities = (await downloadStorage.AgencyStore.GetAllAgencies(regionEntities[regionEntityIndex].Id)).ToList();

                // fetch all stops for that region
                stopEntities = (await downloadStorage.StopStore.GetAllStops(regionEntities[regionEntityIndex].Id)).ToList();
            }

            int agencyEntityIndex = -1;
            if (agencyEntities != null && agencyEntities.Count > 0)
            {
                // pick a random agency entity
                agencyEntityIndex = new Random().Next(agencyEntities.Count);

                // fetch all routes for that agency
                routeEntities = (await downloadStorage.RouteStore.GetAllRoutes(regionEntities[regionEntityIndex].Id, agencyEntities[agencyEntityIndex].Id)).ToList();
            }

            // clean up
            await downloadManager.DeleteDownload();

            // check
            Assert.IsNotNull(metadataEntities);

            Assert.IsNotNull(regionsListEntity);
            Assert.AreEqual(regionsListEntity.RecordType, RecordType.RegionsList.ToString());
            Assert.AreEqual(regionsListEntity.RegionsServiceUri, TestConstants.OBARegionsListUri);
            IEnumerable<DownloadMetadataEntity> regionsListMetadata = from entity in metadataEntities
                                                                      where entity.RecordType == RecordType.RegionsList.ToString()
                                                                      select entity;
            Assert.IsNotNull(regionsListMetadata);
            Assert.AreEqual(regionsListMetadata.Count(), 1);
            Assert.AreEqual(regionsListMetadata.ElementAt(0).Count, 1);
            Assert.AreEqual(regionsListMetadata.ElementAt(0).RunId, runId);
            Assert.AreEqual(regionsListMetadata.ElementAt(0).AgencyId, string.Empty);
            Assert.AreEqual(regionsListMetadata.ElementAt(0).RegionId, string.Empty);

            Assert.IsNotNull(regionEntities);
            IEnumerable<DownloadMetadataEntity> regionMetadata = from entity in metadataEntities
                                                                 where entity.RecordType == RecordType.Region.ToString()
                                                                 select entity;
            Assert.IsNotNull(regionMetadata);
            Assert.AreEqual(regionMetadata.Count(), 1);
            Assert.AreEqual(regionMetadata.ElementAt(0).Count, regionEntities.Count);
            Assert.AreEqual(regionMetadata.ElementAt(0).RunId, runId);
            Assert.AreEqual(regionMetadata.ElementAt(0).AgencyId, string.Empty);
            Assert.AreEqual(regionMetadata.ElementAt(0).RegionId, string.Empty);

            Assert.IsNotNull(agencyEntities);
            IEnumerable<DownloadMetadataEntity> agencyMetadata = from entity in metadataEntities
                                                                 where entity.RecordType == RecordType.Agency.ToString() && entity.RegionId == regionEntities[regionEntityIndex].Id
                                                                 select entity;
            Assert.IsNotNull(agencyMetadata);
            Assert.AreEqual(agencyMetadata.Count(), 1);
            Assert.AreEqual(agencyMetadata.ElementAt(0).Count, agencyEntities.Count);
            Assert.AreEqual(agencyMetadata.ElementAt(0).RunId, runId);
            Assert.AreEqual(agencyMetadata.ElementAt(0).AgencyId, string.Empty);
            Assert.AreEqual(agencyMetadata.ElementAt(0).RegionId, regionEntities[regionEntityIndex].Id);

            Assert.IsNotNull(routeEntities);
            IEnumerable<DownloadMetadataEntity> routeMetadata = from entity in metadataEntities
                                                                where entity.RecordType == RecordType.Route.ToString() && entity.RegionId == regionEntities[regionEntityIndex].Id && entity.AgencyId == agencyEntities[agencyEntityIndex].Id
                                                                select entity;
            Assert.IsNotNull(routeMetadata);
            Assert.AreEqual(routeMetadata.Count(), 1);
            Assert.AreEqual(routeMetadata.ElementAt(0).Count, routeEntities.Count);
            Assert.AreEqual(routeMetadata.ElementAt(0).RunId, runId);
            Assert.AreEqual(routeMetadata.ElementAt(0).AgencyId, agencyEntities[agencyEntityIndex].Id);
            Assert.AreEqual(routeMetadata.ElementAt(0).RegionId, regionEntities[regionEntityIndex].Id);

            Assert.IsNotNull(stopEntities);
            IEnumerable<DownloadMetadataEntity> stopMetadata = from entity in metadataEntities
                                                               where entity.RecordType == RecordType.Stop.ToString() && entity.RegionId == regionEntities[regionEntityIndex].Id
                                                               select entity;
            Assert.IsNotNull(stopMetadata);
            Assert.AreEqual(stopMetadata.Count(), 1);
            Assert.AreEqual(stopMetadata.ElementAt(0).Count, stopEntities.Count);
            Assert.AreEqual(stopMetadata.ElementAt(0).RunId, runId);
            Assert.AreEqual(stopMetadata.ElementAt(0).AgencyId, string.Empty);
            Assert.AreEqual(stopMetadata.ElementAt(0).RegionId, regionEntities[regionEntityIndex].Id);
        }
    }
}
