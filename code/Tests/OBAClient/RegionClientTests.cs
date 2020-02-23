// <copyright file="RegionClientTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.OBAClient;

    /// <summary>
    /// Tests the fetching of region data from OBA servers
    /// </summary>
    [TestClass]
    public class RegionClientTests
    {
        /// <summary>
        /// Tests the fetching of all regions
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task GetAllRegionsTest()
        {
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var regionsList = await obaClient.GetAllRegionsAsync();
            Assert.IsTrue(regionsList != null && regionsList.Regions.Count() > 0);
            foreach (var region in regionsList.Regions)
            {
                Assert.IsFalse(string.IsNullOrEmpty(region.Id));
                Assert.IsFalse(string.IsNullOrEmpty(region.ObaBaseUrl));
                Assert.IsFalse(string.IsNullOrEmpty(region.RegionName));
                Assert.IsFalse(string.IsNullOrEmpty(region.RawContent));
            }
        }
    }
}
