// <copyright file="TestConstants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tests
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;

    using OBAService.Utils;
    using SocialPlus.Logging;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Utils;

    /// <summary>
    /// Configuration for tests
    /// </summary>
    public static class TestConstants
    {
        /// <summary>
        /// Name of the config file
        /// </summary>
        public static readonly string ConfigFileName;

        /// <summary>
        /// Azure storage connection string to use for tests
        /// </summary>
        public static readonly string AzureStorageConnectionString;

        /// <summary>
        /// OBA API key to use for tests
        /// </summary>
        public static readonly string OBAApiKey;

        /// <summary>
        /// OBA Regions List URI
        /// </summary>
        public static readonly string OBARegionsListUri;

        /// <summary>
        /// Embedded Social service URI
        /// </summary>
        public static readonly Uri EmbeddedSocialUri;

        /// <summary>
        /// Embedded Social app key for OneBusAway
        /// </summary>
        public static readonly string EmbeddedSocialAppKey;

        /// <summary>
        /// Embedded Social AAD token to use for authentication
        /// </summary>
        public static readonly string EmbeddedSocialAADToken;

        /// <summary>
        /// Embedded Social user handle to use for creating or updating topics for OneBusAway
        /// </summary>
        public static readonly string EmbeddedSocialAdminUserHandle;

        /// <summary>
        /// Embedded Social expects an AAD token to acccess its API. Obtaining a token from AAD is done on behalf of an application.
        /// This is the application's tenant id.
        /// </summary>
        public static readonly string AADTenantId;

        /// <summary>
        /// Embedded Social expects an AAD token to acccess its API. Obtaining a token from AAD is done on behalf of an application.
        /// This is the OBA Service application id.
        /// </summary>
        public static readonly string AADOBAAppId;

        /// <summary>
        /// Embedded Social expects an AAD token to acccess its API. Obtaining a token from AAD is done on behalf of an application.
        /// This is the OBA Server home page.
        /// </summary>
        public static readonly string AADOBAHomePage;

        /// <summary>
        /// Embedded Social expects an AAD token to acccess its API. Obtaining a token from AAD requers authenticating to the AAD first.
        /// This is the client-side certificate needed to authenticate to AAD.
        /// </summary>
        public static readonly string OBACertThumbprint;

        /// <summary>
        /// Uri of the key vault
        /// </summary>
        public static readonly string KeyVaultUri;

        /// <summary>
        /// Email address to send emails to
        /// </summary>
        public static readonly string SendGridEmailAddr;

        /// <summary>
        /// Key to use SendGrid
        /// </summary>
        public static readonly string SendGridKey;

        /// <summary>
        /// Service bus connection string
        /// </summary>
        public static readonly string ServiceBusConnectionString;

        /// <summary>
        /// Number of milliseconds to sleep between certain Azure Table operations,
        /// such as between deleting a table and creating a table with the same name.
        /// This prevents Azure Table throwing 409 error codes, or not returning table entries
        /// that were recently created.
        /// </summary>
        public static readonly int AzureTableDelay = 1000;

        /// <summary>
        /// How many times to try creating a table despite a 409 conflict error message, before giving up.
        /// </summary>
        public static readonly int AzureTableCreateTries = 50;

        /// <summary>
        /// Initializes static members of the <see cref="TestConstants"/> class.
        /// </summary>
        static TestConstants()
        {
#if OBA_DEV_ALEC
            string environmentName = "oba-dev-alec";
            ConfigFileName = environmentName + ".config";
#endif
#if OBA_DEV_SHARAD
            string environmentName = "oba-dev-sharad";
            ConfigFileName = environmentName + ".config";
#endif
#if OBA_PPE
            string environmentName = "oba-ppe";
            ConfigFileName = environmentName + ".config";
#endif
#if OBA_PROD
            string environmentName = "oba-prod";
            ConfigFileName = environmentName + ".config";
#endif

            // use the fsr to read values that are not secrets
            var fsr = new FileSettingsReader(ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + ConfigFileName);

            EmbeddedSocialAdminUserHandle = fsr.ReadValue("EmbeddedSocialAdminUserHandle");
            EmbeddedSocialAppKey = fsr.ReadValue("EmbeddedSocialAppKey");
            EmbeddedSocialUri = new Uri(fsr.ReadValue("EmbeddedSocialUri"));

            AADOBAAppId = fsr.ReadValue("AADOBAAppId");
            AADOBAHomePage = fsr.ReadValue("AADOBAHomePage");
            AADTenantId = fsr.ReadValue("AADTenantId");

            KeyVaultUri = fsr.ReadValue("KeyVaultUri");
            OBACertThumbprint = fsr.ReadValue("OBACertThumbprint");
            OBARegionsListUri = fsr.ReadValue("OBARegionsListUri");

            // use the kvr to read secrets
            ICertificateHelper cert = new CertificateHelper(OBACertThumbprint, AADOBAAppId, StoreLocation.CurrentUser);
            IKeyVaultClient kvClient = new AzureKeyVaultClient(cert);
            var log = new Log(LogDestination.Debug, Log.DefaultCategoryName);
            var kv = new KV(log, AADOBAAppId, KeyVaultUri, OBACertThumbprint, StoreLocation.CurrentUser, kvClient);
            var kvr = new KVSettingsReader(fsr, kv);

            AzureStorageConnectionString = kvr.ReadValueAsync("AzureStorageConnectionString").Result;
            OBAApiKey = kvr.ReadValueAsync("OBAApiKey").Result;
            SendGridEmailAddr = kvr.ReadValueAsync("SendGridEmailAddr").Result;
            SendGridKey = kvr.ReadValueAsync("SendGridKey").Result;
            ServiceBusConnectionString = kvr.ReadValueAsync("ServiceBusConnectionString").Result;

            // Obtain an AAD token using a cert from the local store for the current user
            AADSettings aadSettings = new AADSettings(TestConstants.AADTenantId, TestConstants.AADOBAAppId, TestConstants.AADOBAHomePage, TestConstants.OBACertThumbprint);
            CertificateHelper certHelper = new CertificateHelper(TestConstants.OBACertThumbprint, TestConstants.AADOBAAppId, StoreLocation.CurrentUser);
            EmbeddedSocialAADToken = certHelper.GetAccessToken(aadSettings.Authority, aadSettings.AppUri).Result;
        }
    }
}
