function Format-OBAManifest {
    <#
    .NOTES
        Name: Format-OBAManifest.ps1
        Requires: Azure Powershell with Azure subscription loaded
    .SYNOPSIS
        Prints the configuration data of an environment from the manifest
    .DESCRIPTION
        Prints out various forms of configuration data read from the manifest.
    .PARAMETER ManifestFileName
        Name of the OBA environment manifest
    .PARAMETER AppConfig
        This switch indicates that the script should print the environment configuration in an App-specific format.
        The format is easily ingestible by ConfigManager
    .PARAMETER AzureConfig
        This switch indicates that the script should print the environment configuration in an Azure-specific format.
        The format is easily ingestible by CloudConfigManager
    .PARAMETER Provision
        This switch indicates that the config should be provisioned into the KeyVault.
        Note that not all settings are put in the key vault, only the secret ones.
    #>

    param(
        [Parameter(Mandatory=$true,HelpMessage='File name of manifest')]
        [Alias("Name")]
        [string] $ManifestFileName,

        [switch] $AzureConfig,

        [switch] $AppConfig,

        [switch] $Provision
    )

    begin {
        $config = @{}

        # Read the json input file into an object
        $manifest = (Get-Content $ManifestFileName) -join "`n" | ConvertFrom-Json
        $EnvironmentName = $manifest.StorageAccount.ResourceGroupName
        $nameNoDashes = $manifest.StorageAccount.ResourceGroupName.Replace("-","")
    }

    process {
        # Construct AADOBAAppId
        $key = "AADOBAAppId"
        $value = $manifest.AADOBAAppId
        $config.Add($key, $value)
    
        # Construct AADOBAHomePage
        $key = "AADOBAHomePage"
        $value = $manifest.AADOBAHomePage
        $config.Add($key, $value)
        
        # Construct AADTenantId
        $key = "AADTenantId"
        $value = $manifest.AADTenantId
        $config.Add($key, $value)
    
        # Construct AzureStorageConnectionString
        $storageName = $manifest.StorageAccount.Name
        $storageKey = $manifest.StorageAccountPrimary
        $key = "AzureStorageConnectionString"
        $value = "DefaultEndpointsProtocol=https;AccountName=${storageName};AccountKey=${storageKey}"
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)

        # Construct EmbeddedSocialAdminUserHandle
        $key = "EmbeddedSocialAdminUserHandle"
        $value = $manifest.EmbeddedSocialAdminUserHandle
        $config.Add($key, $value)
        
        # Construct EmbeddedSocialAppKey
        $key = "EmbeddedSocialAppKey"
        $value = $manifest.EmbeddedSocialAppKey
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)
        
        # Construct EmbeddedSocialUri
        $key = "EmbeddedSocialUri"
        $value = $manifest.EmbeddedSocialUri
        $config.Add($key, $value)
        
        # Construct KeyVaultUri
        $key = "KeyVaultUri"
        $value = $manifest.KeyVault.VaultUri
        $config.Add($key, $value)
        
        # Construct Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString
        $key = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"
        $value = "UseDevelopmentStorage=true"
        $config.Add($key, $value)
        
        # Construct OBAApiKey
        $key = "OBAApiKey"
        $value = $manifest.OBAApiKey
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)

        # Construct OBACertThumbprint
        $key = "OBACertThumbprint"
        $value = $manifest.ServiceCertificate.Thumbprint
        $config.Add($key, $value)
        
        # Construct OBARegionsListUri
        $key = "OBARegionsListUri"
        $value = $manifest.OBARegionsListUri
        $config.Add($key, $value)
        
        # Construct SendGridEmailAddr
        $key = "SendGridEmailAddr"
        $value = $manifest.SendGridEmailAddr
        $config.Add($key, $value)
        
        # Construct SendGridKey
        $key = "SendGridKey"
        $value = $manifest.SendGridKey
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)
        
        # Construct ServiceBusConnectionString
        $key = "ServiceBusConnectionString"
        $value = $manifest.ServiceBusConnectionString
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)
        
        # Construct SigningKey
        $key = "SigningKey"
        $value = $manifest.SigningKey.Id
        $config.Add($key, $value)
    }

    end {
        if ($AzureConfig) {
            $config.Keys | Sort-Object | Foreach-Object { $key = $_; $key = $key -replace "MicrosoftWindowsAzurePluginsDiagnosticsConnectionString","Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"; "<Setting name=`"$key`" value=`"" + $config.Item($_) + "`" />" }
        }

        if ($AppConfig) {
            $config.Keys | Sort-Object | Foreach-Object { $key = $_; $key = $key -replace "MicrosoftWindowsAzurePluginsDiagnosticsConnectionString","Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"; "<add key=`"$key`" value=`"" + $config.Item($_) + "`" />" }
        }
    }
}

function Put-KV {
    param (
        [Parameter(Mandatory=$true)]
        [string] $VaultName,

        [Parameter(Mandatory=$true)]
        [string] $Name,

        [Parameter(Mandatory=$true)]
        [string] $Value
    )

    process {
        # Convert value from string to secure string
        $secretValue = ConvertTo-SecureString -String $Value -AsPlainText -Force

        # Insert the secure string in the KV
        $KVId = (Set-AzureKeyVaultSecret -VaultName $VaultName -Name $Name -SecretValue $secretValue).Id;

        return $KVId
    }
}

