// <copyright file="AgencyClientTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.OBAClient;

    /// <summary>
    /// Tests the fetching of agency data from OBA servers
    /// </summary>
    [TestClass]
    public class AgencyClientTests
    {
        /// <summary>
        /// Tests the fetching of all agencies
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task GetAllAgenciesTest()
        {
            var obaClient = new Client(TestConstants.OBAApiKey, TestConstants.OBARegionsListUri);
            var regionsList = await obaClient.GetAllRegionsAsync();
            Assert.IsTrue(regionsList != null && regionsList.Regions.Count() > 0);

            foreach (var region in regionsList.Regions.Where(item => (item.Active && item.SupportsEmbeddedSocial)))
            {
                var agencies = await obaClient.GetAllAgenciesAsync(region);
                Assert.IsTrue(agencies.Count() > 0);
                foreach (var agency in agencies)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(agency.Id));
                    Assert.IsFalse(string.IsNullOrEmpty(agency.Name));
                    Assert.IsFalse(string.IsNullOrEmpty(agency.RawContent));
                }
            }
        }
    }
}
