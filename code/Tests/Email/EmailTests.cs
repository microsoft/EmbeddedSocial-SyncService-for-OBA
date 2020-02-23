// <copyright file="EmailTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests.Email
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OBAService.Email;
    using OBAService.Storage;
    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Tests the email class
    /// </summary>
    [TestClass]
    public class EmailTests
    {
        /// <summary>
        /// Tests sending an email with a fake exception
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task SendException1()
        {
            Email email = new Email();
            email.To = new List<string>() { TestConstants.SendGridEmailAddr };
            email.Add(RunId.GenerateTestRunId());

            // create a fake exception
            Exception e = new System.Exception("outer exception test message", new System.Exception("inner exception test message", new System.Exception("innermost exception test message")));
            email.Add(e);

            // send it
            await email.Send(TestConstants.SendGridKey);
        }

        /// <summary>
        /// Tests sending an email with a fake exception
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task SendException2()
        {
            Email email = new Email();
            email.To = new List<string>() { TestConstants.SendGridEmailAddr };
            email.Add(RunId.GenerateTestRunId());

            Exception[] exceptions =
            {
                new System.Exception("inner exception test message1", new System.Exception("innermost exception test message1")),
                new System.Exception("inner exception test message2", new System.Exception("innermost exception test message2")),
            };

            // create a fake exception
            Exception e = new System.Exception("outer exception test message", new AggregateException("inner aggregate exception test message", exceptions));
            email.Add(e);

            // send it
            await email.Send(TestConstants.SendGridKey);
        }

        /// <summary>
        /// Tests sending an email with a fake aggregate exception
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task SendAggregateException1()
        {
            Email email = new Email();
            email.To = new List<string>() { TestConstants.SendGridEmailAddr };
            email.Add(RunId.GenerateTestRunId());

            // create a fake aggregate exception
            AggregateException e = new AggregateException("outer aggregate exception test message", new System.Exception("inner exception test message", new System.Exception("innermost exception test message")));
            email.Add(e);

            // send it
            await email.Send(TestConstants.SendGridKey);
        }

        /// <summary>
        /// Tests sending an email with a fake aggregate exception
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task SendAggregateException2()
        {
            Email email = new Email();
            email.To = new List<string>() { TestConstants.SendGridEmailAddr };
            email.Add(RunId.GenerateTestRunId());

            // create a fake aggregate exception
            Exception[] exceptions =
            {
                new System.Exception("inner exception test message1", new System.Exception("innermost exception test message1")),
                new System.Exception("inner exception test message2", new System.Exception("innermost exception test message2")),
            };

            AggregateException e = new AggregateException("outer aggregate exception test message", exceptions);
            email.Add(e);

            // send it
            await email.Send(TestConstants.SendGridKey);
        }

        /// <summary>
        /// Tests sending an email with typical run results
        /// </summary>
        /// <returns>a test task</returns>
        [TestMethod]
        public async Task SendTypicalRun()
        {
            Email email = new Email();
            email.To = new List<string>() { TestConstants.SendGridEmailAddr };
            string runId = RunId.GenerateTestRunId();
            email.Add(runId);
            email.Add(TestConstants.EmbeddedSocialUri);

            // create fake region & agency for records
            RegionEntity region = TestUtilities.FakeRegionEntity();
            AgencyEntity agency = TestUtilities.FakeAgencyEntity(region.Id);

            // create fake download metadata
            List<DownloadMetadataEntity> downloadMetadata = new List<DownloadMetadataEntity>();
            downloadMetadata.Add(new DownloadMetadataEntity()
            {
                RunId = runId,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                Count = 10
            });
            downloadMetadata.Add(new DownloadMetadataEntity()
            {
                RunId = runId,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Stop.ToString(),
                Count = 2
            });
            email.Add(downloadMetadata);

            // create fake diff metadata entries
            List<DiffMetadataEntity> diffMetadata = new List<DiffMetadataEntity>();
            diffMetadata.Add(new DiffMetadataEntity()
            {
                RunId = runId,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            });
            diffMetadata.Add(new DiffMetadataEntity()
            {
                RunId = runId,
                RegionId = region.Id,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            });
            email.Add(diffMetadata);

            // create fake publish metadata entries
            List<PublishMetadataEntity> publishMetadata = new List<PublishMetadataEntity>();
            publishMetadata.Add(new PublishMetadataEntity()
            {
                RunId = runId,
                RegionId = region.Id,
                AgencyId = agency.Id,
                RecordType = RecordType.Route.ToString(),
                AddedCount = 0,
                UpdatedCount = 1,
                DeletedCount = 2,
                ResurrectedCount = 999
            });
            publishMetadata.Add(new PublishMetadataEntity()
            {
                RunId = runId,
                RegionId = region.Id,
                AgencyId = string.Empty,
                RecordType = RecordType.Stop.ToString(),
                AddedCount = 1,
                UpdatedCount = 0,
                DeletedCount = 0,
                ResurrectedCount = 0
            });
            email.Add(publishMetadata);

            // send it
            await email.Send(TestConstants.SendGridKey);
        }
    }
}