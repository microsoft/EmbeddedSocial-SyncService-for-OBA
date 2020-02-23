// <copyright file="StopClientTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.OBAClient;
    using OBAService.OBAClient.Model;

    /// <summary>
    /// Tests the fetching of stop data from OBA servers
    /// </summary>
    [TestClass]
    public class StopClientTests
    {
        /// <summary>
        /// Tests fetching stops Ids from one particular agency
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task GetStopIdsFromOneAgencyTest()
        {
            var region = new Region() { Id = "1", RegionName = "Puget Sound", ObaBaseUrl = "http://api.pugetsound.onebusaway.org/" };
            var agency = new Agency() { Id = "1", Name = "Metro Transit" };
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var stops = await obaClient.GetAllStopIdsAsync(region, agency);
            Assert.IsTrue(stops.Count() > 0);
            foreach (var stop in stops)
            {
                Assert.IsFalse(string.IsNullOrEmpty(stop));
            }
        }

        /// <summary>
        /// Tests fetching stop Ids from the first agency of every region
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task GetAllStopIdsFromOneAgencyInEveryRegionTest()
        {
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var regionsList = await obaClient.GetAllRegionsAsync();
            Assert.IsTrue(regionsList != null && regionsList.Regions.Count() > 0);

            foreach (var region in regionsList.Regions.Where(item => (item.Active && item.SupportsEmbeddedSocial)))
            {
                var agencies = await obaClient.GetAllAgenciesAsync(region);
                Assert.IsTrue(agencies.Count() > 0);
                var agency = agencies.ElementAt(0);
                Assert.IsFalse(string.IsNullOrEmpty(agency.Id));
                var stops = await obaClient.GetAllStopIdsAsync(region, agency);

                // Stop count could be 0 for some agencies
                if (stops.Count() > 0)
                {
                    foreach (var stop in stops)
                    {
                        Assert.IsFalse(string.IsNullOrEmpty(stop));
                    }
                }
            }
        }

        /// <summary>
        /// Tests fetching stops details for one particular stop
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task GetStopDetailsForOneStopIdTest()
        {
            var region = new Region() { Id = "1", RegionName = "Puget Sound", ObaBaseUrl = "http://api.pugetsound.onebusaway.org/" };
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var stopDetails = await obaClient.GetStopDetailsAsync(region, "1_10620");
            Assert.IsNotNull(stopDetails);
            Assert.IsFalse(string.IsNullOrEmpty(stopDetails.Name));
            Assert.IsFalse(string.IsNullOrEmpty(stopDetails.RawContent));
            Assert.AreEqual("1_10620", stopDetails.Id);
            Assert.AreEqual("10620", stopDetails.Code);
            Assert.AreEqual(0, stopDetails.LocationType);
        }

        /// <summary>
        /// Tests fetching stop details from the first stop of the first agency of every region
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task GetStopDetailsFromOneAgencyInEveryRegionTest()
        {
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var regionsList = await obaClient.GetAllRegionsAsync();
            Assert.IsTrue(regionsList != null && regionsList.Regions.Count() > 0);

            foreach (var region in regionsList.Regions.Where(item => (item.Active && item.SupportsEmbeddedSocial)))
            {
                var agencies = await obaClient.GetAllAgenciesAsync(region);
                Assert.IsTrue(agencies.Count() > 0);
                var agency = agencies.ElementAt(0);
                Assert.IsFalse(string.IsNullOrEmpty(agency.Id));
                var stops = await obaClient.GetAllStopIdsAsync(region, agency);

                // Stop count could be 0 for some agencies. For example : Boston
                if (stops.Count() > 0)
                {
                    var stop = stops.ElementAt(0);
                    Assert.IsFalse(string.IsNullOrEmpty(stop));
                    var stopDetails = await obaClient.GetStopDetailsAsync(region, stop);
                    Assert.IsNotNull(stopDetails);
                    Assert.IsFalse(string.IsNullOrEmpty(stopDetails.Name));
                    Assert.IsFalse(string.IsNullOrEmpty(stopDetails.RawContent));
                    Assert.AreEqual(stop, stopDetails.Id);
                    Assert.AreEqual(0, stopDetails.LocationType);
                }
            }
        }

        /// <summary>
        /// Tests fetching stops details for one particular route with a forward slash in the route id
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task GetStopsDetailsForForwardSlashRouteIdTest()
        {
            var region = new Region() { Id = "3", RegionName = "Atlanta", ObaBaseUrl = "http://atlanta.onebusaway.org/api/" };
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            Stop[] stopsDetails = await obaClient.GetStopsForRouteAsync(region, "GRTA_431 - BrandsMart/Stockbridge to Midtown");
            Assert.IsNotNull(stopsDetails);
            Assert.IsTrue(stopsDetails.Count() > 0);
            foreach (Stop stopDetails in stopsDetails)
            {
                Assert.IsFalse(string.IsNullOrEmpty(stopDetails.Name));
                Assert.IsFalse(string.IsNullOrEmpty(stopDetails.RawContent));
                Assert.IsFalse(string.IsNullOrEmpty(stopDetails.Id));
                Assert.IsFalse(string.IsNullOrEmpty(stopDetails.Code));
            }
        }

        /// <summary>
        /// Tests fetching stops details for one particular route
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task GetStopsDetailsForOneRouteIdTest()
        {
            var region = new Region() { Id = "0", RegionName = "Tampa Bay", ObaBaseUrl = "http://api.tampa.onebusaway.org/api" };
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            Stop[] stopsDetails = await obaClient.GetStopsForRouteAsync(region, "PSTA_5");
            Assert.IsNotNull(stopsDetails);
            Assert.IsTrue(stopsDetails.Count() > 0);
            foreach (Stop stopDetails in stopsDetails)
            {
                Assert.IsFalse(string.IsNullOrEmpty(stopDetails.Name));
                Assert.IsFalse(string.IsNullOrEmpty(stopDetails.RawContent));
                Assert.IsFalse(string.IsNullOrEmpty(stopDetails.Id));
                Assert.IsFalse(string.IsNullOrEmpty(stopDetails.Code));
            }
        }
    }
}
