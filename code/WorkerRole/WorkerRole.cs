// <copyright file="WorkerRole.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.WorkerRole
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.ServiceRuntime;
    using OBAService.Email;
    using OBAService.Storage;
    using OBAService.Storage.Model;
    using OBAService.Utils;
    using SocialPlus;
    using SocialPlus.Logging;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Utils;

    /// <summary>
    /// This worker role executes all the key components of the OBA Service
    /// </summary>
    public class WorkerRole : RoleEntryPoint
    {
        /// <summary>
        /// Cancellation token source
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Run completed event
        /// </summary>
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        /// <summary>
        /// Default limit of outgoing connections per endpoint.
        /// See the .NET documentation of the ServicePointManager class for more detail.
        /// </summary>
        private readonly int defaultConnectionLimit = 500;

        /// <summary>
        /// Primary run routine
        /// </summary>
        public override void Run()
        {
            Alerts.Information("WorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            catch (Exception e)
            {
                Alerts.Exception(e);
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        /// <summary>
        /// Initializes the worker role
        /// </summary>
        /// <returns>some random undocumented bool</returns>
        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = this.defaultConnectionLimit;

            // Turn off Nagling, thereby reducing latency
            ServicePointManager.UseNagleAlgorithm = false;

            // Turn off Expect100, thereby reducing latency on successful operations
            ServicePointManager.Expect100Continue = false;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            bool result = base.OnStart();

            Alerts.Information("WorkerRole has started");

            return result;
        }

        /// <summary>
        /// Allows the worker role to gracefully shutdown
        /// </summary>
        public override void OnStop()
        {
            Alerts.Information("WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Alerts.Information("WorkerRole has stopped");
        }

        /// <summary>
        /// Actual OBAService code is invoked here
        /// </summary>
        /// <param name="cancellationToken">token that represents a cancel</param>
        /// <returns>task that runs OBAService</returns>
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            bool enableAzureSettingsReaderTracing = false;
            ISettingsReader settingsReader = new AzureSettingsReader(enableAzureSettingsReaderTracing);

            // use the normal azure settings reader to fetch the OBA app id, the OBA cert, and the key vault uri
            string aadAppId = settingsReader.ReadValue("AADOBAAppId");
            string obaCertThumbprint = settingsReader.ReadValue("OBACertThumbprint");
            string keyVaultUri = settingsReader.ReadValue("KeyVaultUri");

            // create a key vault settings reader to read secrets
            ICertificateHelper certHelper = new CertificateHelper(obaCertThumbprint, aadAppId, StoreLocation.LocalMachine);
            IKeyVaultClient kvClient = new AzureKeyVaultClient(certHelper);
            var log = new Log(LogDestination.Debug, Log.DefaultCategoryName);
            var kv = new KV(log, aadAppId, keyVaultUri, obaCertThumbprint, StoreLocation.LocalMachine, kvClient);
            var kvr = new KVSettingsReader(settingsReader, kv);

            // get all the settings
            string azureStorageConnectionString = await kvr.ReadValueAsync("AzureStorageConnectionString");
            string obaApiKey = await kvr.ReadValueAsync("OBAApiKey");
            string obaRegionsListUri = await kvr.ReadValueAsync("OBARegionsListUri");
            Uri embeddedSocialUri = new Uri(await kvr.ReadValueAsync("EmbeddedSocialUri"));
            string embeddedSocialAppKey = await kvr.ReadValueAsync("EmbeddedSocialAppKey");
            string embeddedSocialAdminUserHandle = await kvr.ReadValueAsync("EmbeddedSocialAdminUserHandle");
            string aadTenantId = await kvr.ReadValueAsync("AADTenantId");
            string aadAppHomePage = await kvr.ReadValueAsync("AADOBAHomePage");
            string sendGridEmailAddr = await kvr.ReadValueAsync("SendGridEmailAddr");
            string sendGridKey = await kvr.ReadValueAsync("SendGridKey");

            while (!cancellationToken.IsCancellationRequested)
            {
                // create a runId
                string runId = RunId.GenerateRunId();

                // setup email
                Email email = new Email();
                email.To = new List<string>() { sendGridEmailAddr };
                email.Add(runId);
                email.Add(embeddedSocialUri);

                try
                {
                    // obtain an AAD token using a cert from the local store for the current user
                    AADSettings aadSettings = new AADSettings(aadTenantId, aadAppId, aadAppHomePage, obaCertThumbprint);
                    string embeddedSocialAADToken = await certHelper.GetAccessToken(aadSettings.Authority, aadSettings.AppUri);

                    // create all the managers
                    OBADownload.DownloadManager downloadManager = new OBADownload.DownloadManager(azureStorageConnectionString, runId, obaApiKey, obaRegionsListUri);
                    Diff.DiffManager diffManager = new Diff.DiffManager(azureStorageConnectionString, runId);
                    PublishToEmbeddedSocial.PublishManager publishManager = new PublishToEmbeddedSocial.PublishManager(azureStorageConnectionString, runId, embeddedSocialUri, embeddedSocialAppKey, embeddedSocialAADToken, embeddedSocialAdminUserHandle);

                    // initialize storage
                    await downloadManager.InitializeStorage();
                    await diffManager.InitializeStorage();
                    await publishManager.InitializeStorage();

                    // download routes and stops from OBA servers
                    await downloadManager.DownloadAndStore();

                    // add download metadata to email
                    StorageManager downloadMetadataManager = new StorageManager(azureStorageConnectionString, TableNames.TableType.DownloadMetadata, runId);
                    IEnumerable<DownloadMetadataEntity> downloadMetadata = downloadMetadataManager.DownloadMetadataStore.Get(runId);
                    email.Add(downloadMetadata);

                    // compare downloaded data to previously published data
                    await diffManager.DiffAndStore();

                    // add diff metadata to email
                    StorageManager diffMetadataManager = new StorageManager(azureStorageConnectionString, TableNames.TableType.DiffMetadata, runId);
                    IEnumerable<DiffMetadataEntity> diffMetadata = diffMetadataManager.DiffMetadataStore.Get(runId);
                    email.Add(diffMetadata);

                    // publish changes to Embedded Social
                    await publishManager.PublishAndStore();

                    // add publish metadata to email
                    StorageManager publishMetadataManager = new StorageManager(azureStorageConnectionString, TableNames.TableType.PublishMetadata, runId);
                    IEnumerable<PublishMetadataEntity> publishMetadata = publishMetadataManager.PublishMetadataStore.Get(runId);
                    email.Add(publishMetadata);
                }
                catch (Exception e)
                {
                    // add the exception to email
                    email.Add(e);

                    // record it in diagnostic logs
                    Alerts.Error(e);
                }

                // remove the OBA key from the email
                email.RemoveString(obaApiKey);

                // send the email
                await email.Send(sendGridKey);

                // sleep for 24 hours
                await Task.Delay(1000 * 60 * 60 * 24);
            }
        }
    }
}
