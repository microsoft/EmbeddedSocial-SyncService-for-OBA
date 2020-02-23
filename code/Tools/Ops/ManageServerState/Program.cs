// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Tools.ManageServerState
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using OBAService.Utils;
    using SocialPlus.Logging;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Utils;

    /// <summary>
    /// This utility is used to clean and create Azure service state for the OBA service
    /// </summary>
    public class Program
    {
        /// <summary>
        /// names of WAD* tables
        /// </summary>
        public static readonly string[] WADTableNames = new string[6]
        {
            "WADCrashDump",
            "WADDiagnosticsInfrastructureLogsTable",
            "WADDirectoriesTable",
            "WADLogsTable",
            "WADPerformanceCountersTable",
            "WADWindowsEventLogsTable"
        };

        /// <summary>
        /// String used to name the setting of the Azure AD client
        /// </summary>
        private static readonly string OBAClientId = "AADOBAAppId";

        /// <summary>
        /// String used to name the setting of the SocialPlus's cert thumbprint
        /// </summary>
        private static readonly string OBACertThumbprint = "OBACertThumbprint";

        /// <summary>
        /// String used to name the setting of the URL to access keyvault
        /// </summary>
        private static readonly string OBAVaultUrl = "KeyVaultUri";

        /// <summary>
        /// flag that indicates whether the clean or create operation applies to all Azure services
        /// </summary>
        private static bool doAll = false;

        /// <summary>
        /// flag that indicates whether the clean or create operation is performed on Azure service bus queues
        /// </summary>
        private static bool doQueues = false;

        /// <summary>
        /// flag that indicates whether the clean or create operation is performed on Azure tables
        /// </summary>
        private static bool doTables = false;

        /// <summary>
        /// flag that indicates whether the clean or create operation is performed on the WAD* tables
        /// </summary>
        private static bool doLogs = false;

        /// <summary>
        /// flag that indicates whether to perform a create operation
        /// </summary>
        private static bool doCreate = false;

        /// <summary>
        /// flag that indicates whether to perform a clean operation
        /// </summary>
        private static bool doClean = false;

        /// <summary>
        /// flag that indicates whether to perform the requested action without interactive prompts
        /// </summary>
        private static bool forceOperation = false;

        /// <summary>
        /// parameter that provides the name of the environment to operate on
        /// </summary>
        private static string environmentName = null;

        /// <summary>
        /// object used to talk to the azure key vault
        /// </summary>
        private static KV kv = null;

        /// <summary>
        /// This program will create or delete Azure server state for the OBA service
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static void Main(string[] args)
        {
            AsyncMain(args).Wait();
        }

        /// <summary>
        /// Async version of the Main program
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>a task</returns>
        public static async Task AsyncMain(string[] args)
        {
            ParseArgs(args);

            var sr = new FileSettingsReader(ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + environmentName + ".config");
            var certThumbprint = sr.ReadValue(OBACertThumbprint);
            var clientID = sr.ReadValue(OBAClientId);
            var storeLocation = StoreLocation.CurrentUser;
            var vaultUrl = sr.ReadValue(OBAVaultUrl);
            ICertificateHelper cert = new CertificateHelper(certThumbprint, clientID, storeLocation);
            IKeyVaultClient client = new AzureKeyVaultClient(cert);
            var log = new Log(LogDestination.Debug, Log.DefaultCategoryName);
            kv = new KV(log, clientID, vaultUrl, certThumbprint, storeLocation, client);
            var kvReader = new KVSettingsReader(sr, kv);

            if (doClean)
            {
                DisplayWarning();
            }

            // display current configuration
            await ValidateAndPrintConfiguration(environmentName, kvReader);

            if (forceOperation == false)
            {
                // get user approval
                Console.Write("Are you sure you want to proceed? [y/n] : ");
                ConsoleKeyInfo keyInfo = Console.ReadKey(false);
                if (keyInfo.KeyChar != 'y')
                {
                    return;
                }

                Console.WriteLine();
            }

            if (doAll || doTables)
            {
                string azureTableStorageConnectionString = await kvReader.ReadValueAsync("AzureStorageConnectionString");
                if (doClean)
                {
                    // Delete tables
                    await Tables.Clean(azureTableStorageConnectionString);
                }

                if (doCreate)
                {
                    // Create tables
                    await Tables.Create(azureTableStorageConnectionString);
                }
            }

            if (doAll || doQueues)
            {
                if (doClean)
                {
                    // Delete queues
                    await Queues.Clean();
                }

                if (doCreate)
                {
                    // Create queues
                    await Queues.Create();
                }
            }

            if (doAll || doLogs)
            {
                string azureDiagnosticsConnectionString = await kvReader.ReadValueAsync("AzureStorageConnectionString");

                if (doClean)
                {
                    // Delete logs
                    await Logs.Clean(azureDiagnosticsConnectionString);
                }

                if (doCreate)
                {
                    // Create queues
                    await Logs.Create();
                }
            }

            Console.WriteLine("All done! Bye!");
        }

        /// <summary>
        /// Parse the command line arguments and perform some validation checks.
        /// </summary>
        /// <param name="args">command line arguments</param>
        private static void ParseArgs(string[] args)
        {
            int i = 0;
            while (i < args.Length)
            {
                if (args[i].Equals("-All", StringComparison.CurrentCultureIgnoreCase))
                {
                    doAll = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Clean", StringComparison.CurrentCultureIgnoreCase))
                {
                    doClean = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Create", StringComparison.CurrentCultureIgnoreCase))
                {
                    doCreate = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Force", StringComparison.CurrentCultureIgnoreCase))
                {
                    forceOperation = true;
                    i++;
                    continue;
                }
                else if (args[i].StartsWith("-Name=", StringComparison.CurrentCultureIgnoreCase))
                {
                    int prefixLen = "-Name=".Length;
                    environmentName = args[i].Substring(prefixLen);
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Queues", StringComparison.CurrentCultureIgnoreCase))
                {
                    doQueues = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Tables", StringComparison.CurrentCultureIgnoreCase))
                {
                    doTables = true;
                    i++;
                    continue;
                }
                else if (args[i].Equals("-Logs", StringComparison.CurrentCultureIgnoreCase))
                {
                    doLogs = true;
                    i++;
                    continue;
                }
                else
                {
                    // default case
                    Console.WriteLine("Unrecognized parameter: {0}", args[i]);
                    i++;
                }
            }

            if (string.IsNullOrWhiteSpace(environmentName))
            {
                Console.WriteLine("Usage error: must specify name of environment");
                Usage();
                Environment.Exit(0);
            }

            if (!(doClean || doCreate))
            {
                Console.WriteLine("Usage error: must specify an action of clean or create");
                Usage();
                Environment.Exit(0);
            }

            if (doClean && doCreate)
            {
                Console.WriteLine("Usage error: cannot perform both clean and create actions");
                Usage();
                Environment.Exit(0);
            }

            // if action is clean or create, then must specify target objects for the action
            if (!(doAll || doQueues || doTables || doLogs))
            {
                Console.WriteLine("Usage error: must specify which objects to clean or create");
                Usage();
                Environment.Exit(0);
            }

            return;
        }

        /// <summary>
        /// print usage error message
        /// </summary>
        private static void Usage()
        {
            Console.WriteLine("Usage: ManageServerState.exe -Name=<environment-name> [-Clean | -Create] [-Force] [-All | -Blobs | -Queues | -Tables | -Logs]");
        }

        /// <summary>
        /// Display a warning message for cleanup actions
        /// </summary>
        private static void DisplayWarning()
        {
            // display warning
            Console.WriteLine();
            if (doClean && doQueues)
            {
                Console.WriteLine("Warning!! this program will erase all OBA state from the Azure Service Bus queues.");
            }
            else if (doClean && doTables)
            {
                Console.WriteLine("Warning!! this program will erase all OBA state from the Azure Table storage.");
            }
            else if (doClean && doLogs)
            {
                Console.WriteLine("Warning!! this program will erase all OBA logs from the Azure WAD* tables.");
            }
            else if (doClean && doAll)
            {
                Console.WriteLine("Warning!! this program will erase all OBA state from Azure.  Everything!  Really!!");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// gets the current config, makes sure it is not a production environment, and prints the configuration
        /// </summary>
        /// <param name="environmentName">name of the environment that will be created or cleaned</param>
        /// <param name="kvr">key vault settings reader</param>
        /// <returns>validate and print task</returns>
        private static async Task ValidateAndPrintConfiguration(string environmentName, ISettingsReaderAsync kvr)
        {
            if (doAll || doTables)
            {
                if (string.IsNullOrWhiteSpace(await kvr.ReadValueAsync("AzureStorageConnectionString")))
                {
                    Console.WriteLine("Error! AzureStorageConnectionString in your configuration is null or whitespace. Aborting...");
                    System.Environment.Exit(-1);
                }
            }

            if (doAll || doLogs)
            {
                if (string.IsNullOrWhiteSpace(await kvr.ReadValueAsync("AzureStorageConnectionString")))
                {
                    Console.WriteLine("Error! AzureStorageConnectionString in your configuration is null or whitespace. Aborting...");
                    System.Environment.Exit(-1);
                }
            }

            if (doClean && ProdConfiguration.IsProduction(await kvr.ReadValueAsync("AzureStorageConnectionString")))
            {
                Console.WriteLine("Error! Your configuration includes a production service. Aborting...");
                System.Environment.Exit(-1);
            }

            Console.WriteLine();
            Console.Write("Environment name: ");
            Console.WriteLine(environmentName);
            Console.WriteLine();
            Console.WriteLine("Current configuration:");

            if (doAll || doTables)
            {
                Console.WriteLine("\tAzure table storage string: " + await kvr.ReadValueAsync("AzureStorageConnectionString"));
            }

            if (doAll || doLogs)
            {
                Console.WriteLine("\tAzure diagnostics table storage string: " + await kvr.ReadValueAsync("AzureStorageConnectionString"));
            }

            Console.WriteLine();
        }
    }
}
