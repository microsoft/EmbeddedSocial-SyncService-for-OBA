// <copyright file="RouteClientTests.cs" company="Microsoft">
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
    /// Tests the fetching of route data from OBA servers
    /// </summary>
    [TestClass]
    public class RouteClientTests
    {
        /// <summary>
        /// Tests fetching routes from one particular agency
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task GetRoutesFromOneAgencyTest()
        {
            var region = new Region() { Id = "1", RegionName = "Puget Sound", ObaBaseUrl = "http://api.pugetsound.onebusaway.org/" };
            var agency = new Agency() { Id = "1", Name = "Metro Transit" };
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var routes = await obaClient.GetAllRoutesAsync(region, agency);
            Assert.IsTrue(routes.Count() > 0);
            foreach (var route in routes)
            {
                Assert.IsFalse(string.IsNullOrEmpty(route.Id));
                Assert.IsFalse(string.IsNullOrEmpty(route.ShortName));
                Assert.IsFalse(string.IsNullOrEmpty(route.RawContent));
            }
        }

        /// <summary>
        /// Tests fetching routes from the first agency of every region
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task GetAllRoutesFromOneAgencyInEveryRegionTest()
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
                var routes = await obaClient.GetAllRoutesAsync(region, agency);

                // Route count could be 0 for some agencies
                if (routes.Count() > 0)
                {
                    foreach (var route in routes)
                    {
                        Assert.IsFalse(string.IsNullOrEmpty(route.Id));
                        Assert.IsFalse(string.IsNullOrEmpty(route.ShortName) && string.IsNullOrEmpty(route.LongName));
                        Assert.IsFalse(string.IsNullOrEmpty(route.RawContent));
                    }
                }
            }
        }
    }
}
