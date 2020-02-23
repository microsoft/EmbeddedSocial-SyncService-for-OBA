
function New-OBAEnvironment {
    <#
    .NOTES
        Name: New-OBAEnvironment.ps1
        Requires: Azure Powershell version 2.1 or higher.
    .SYNOPSIS
        Creates a new instance of the OneBusAway server on Azure.
    .DESCRIPTION
        Implements all logic needed to create a new instance of the OneBusAway server on Azure.
    .PARAMETER AADOBAAppId
        This id comes from the AAD portal. Look under App registrations, and use the ApplicationID value for OneBusAway Server.
        Default value is "e7b11ff7-86e8-40c3-bbb2-fc0204965c3d"
    .PARAMETER AADOBAHomePage
        This value comes from the AAD portal. Look under App registrations, and use the Home Page value for OneBusAway Server.
        Default value is "http://oba-prod.cloudapp.net"
    .PARAMETER AADTenantId
        This id comes from the AAD portal.  Look under Properties for Microsoft, and use the value of the DirectoryID.
        Default value is "72f988bf-86f1-41af-91ab-2d7cd011db47"
    .PARAMETER EmbeddedSocialAdminUserHandle
        User handle for an admin user in Embedded Social.
    .PARAMETER EmbeddedSocialAppKey
        App key for Embedded Social.
    .PARAMETER EmbeddedSocialUri
        Uri for Embedded Social.
    .PARAMETER Name
        Name of the OneBusAway server instance to create.
    .PARAMETER Location
        Location specifies the Azure datacenter location where resources are created.
        Default value is "West US".
    .PARAMETER OBAApiKey
        This key is used to access OBA servers.
    .PARAMETER OBACertThumbprint
        Thumbprint of the client certificate for the OneBusAway server.
        Default value is "93DFB07CC4292C832F9E8BBDA21F878BD40EC1AC"
    .PARAMETER OBARegionsListUri
        Uri to fetch the list of OBA regions
        Default value is "http://regions.onebusaway.org/regions-v3.xml"
    .PARAMETER SendGridEmailAddr
        Email address destination for SendGrid messages.
    .PARAMETER SendGridKey
        Name of the instrumentation key from SendGrid.
    #>
    param (
        [Parameter(Mandatory=$false)]
        [string]$AADOBAAppId = "e7b11ff7-86e8-40c3-bbb2-fc0204965c3d",

        [Parameter(Mandatory=$false)]
        [string]$AADOBAHomePage = "http://oba-prod.cloudapp.net",
        
        [Parameter(Mandatory=$false)]
        [string]$AADTenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
        
        [Parameter(Mandatory=$true)]
        [string]$EmbeddedSocialAdminUserHandle,
        
        [Parameter(Mandatory=$true)]
        [string]$EmbeddedSocialAppKey,
        
        [Parameter(Mandatory=$true)]
        [string]$EmbeddedSocialUri,
        
        [Parameter(Mandatory=$true)]
        [string]$Name,

        [Parameter(Mandatory=$false)]
        [string]$Location = "West US",

        [Parameter(Mandatory=$true)]
        [string]$OBAApiKey,
        
        [Parameter(Mandatory=$false)]
        [string]$OBACertThumbprint = "93DFB07CC4292C832F9E8BBDA21F878BD40EC1AC",
        
        [Parameter(Mandatory=$false)]
        [string]$OBARegionsListUri = "http://regions.onebusaway.org/regions-v3.xml",

        [Parameter(Mandatory=$false)]
        [string]$SendGridEmailAddr = "socialplusops@microsoft.com",
        
        [Parameter(Mandatory=$true)]
        [string]$SendGridKey
    )

    begin {
        # Ask the user to press 'y' to continue
        $message  = 'This script creates Azure resources for a new SocialPlus environment.'
        $question = 'Are you sure you want to proceed?'

        $choices = New-Object Collections.ObjectModel.Collection[Management.Automation.Host.ChoiceDescription]
        $choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&Yes'))
        $choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&No'))

        $decision = $Host.UI.PromptForChoice($message, $question, $choices, 1)
        if ($decision -ne 0) {
            throw 'cancelled'
        }
    }

    process {
        $EnvironmentName = $Name
        $ResourceGroup = $EnvironmentName

        # Create the manifest as a hashtable
        $manifest = @{}

        # Add EnvironmentName to manifest
        $manifest.EnvironmentName = $EnvironmentName

        # Write the AAD OneBusAway ApplicationID to the manifest 
        $manifest.AADOBAAppId = $AADOBAAppId

        # Write the AAD OneBusAway home page to the manifest 
        $manifest.AADOBAHomePage = $AADOBAHomePage
        
        # Write the AAD tenantID to the manifest 
        $manifest.AADTenantId = $AADTenantId
        
        # Write the EmbeddedSocialAdminUserHandle to the manifest
        $manifest.EmbeddedSocialAdminUserHandle = $EmbeddedSocialAdminUserHandle

        # Write the EmbeddedSocialAppKey to the manifest
        $manifest.EmbeddedSocialAppKey = $EmbeddedSocialAppKey

        # Write the EmbeddedSocialUri to the manifest
        $manifest.EmbeddedSocialUri = $EmbeddedSocialUri
        
        # Write the OBAApiKey to the manifest
        $manifest.OBAApiKey = $OBAApiKey

        # Write the OBARegionsListUri to the manifest
        $manifest.OBARegionsListUri = $OBARegionsListUri
        
        # Write the SendGridKey to the manifest
        $manifest.SendGridKey = $SendGridKey
        
        # Write the SendGridEmailAddr to the manifest
        $manifest.SendGridEmailAddr = $SendGridEmailAddr

        # create the resource group and update the manifest
        $manifest.ResourceGroup = New-AzureRmResourceGroup -Name $ResourceGroup -Location $Location
        Write-Verbose ($manifest.ResourceGroup | Out-String)
        Write-Verbose "Created resource group $ResourceGroup."

        # create a key vault in the resource group 
        $manifest.KeyVault = New-AzureRmKeyVault -VaultName $EnvironmentName -ResourceGroupName $ResourceGroup -Location $Location
        Write-Verbose ($manifest.KeyVault | Out-String)
        Write-Verbose "Created key vault $EnvironmentName"
            
        # we postpone provisioning the key vault until later in this script because sometimes 
        # we notice errors when trying to provision it right away

        # create a storage account in the resource group
        $storageName =  $EnvironmentName.Replace("-","")

        # use LRS for a classic storage account
        $storageType = "Standard_LRS"
        Write-Verbose "Creating classic storage account $storageName..."
        $manifest.StorageAccount = New-AzureRmResource -ResourceGroupName $ResourceGroup -ResourceName $storageName `
          -ResourceType "Microsoft.ClassicStorage/storageAccounts" -ApiVersion "2015-12-01" `
          -Properties @{ "accountType" = "$storageType" } -Location $Location -Force
        Write-Verbose ($manifest.StorageAccount | Out-String)
        $manifest.StorageAccountPrimary = (Get-AzureStorageKey -StorageAccountName $storageName).Primary
        Write-Verbose ($manifest.StorageAccountPrimary | Out-String)
        $manifest.StorageAccountSecondary = (Get-AzureStorageKey -StorageAccountName $storageName).Secondary
        Write-Verbose ($manifest.torageAccountSecondary | Out-String)
        Write-Verbose "Created classic storage account $storageName"

        # create a cloud service in the resource group
        $manifest.CloudService = New-AzureRmResource -ResourceGroupName $ResourceGroup -ResourceName $EnvironmentName `
          -ResourceType "Microsoft.ClassicCompute/domainNames" `
          -Properties @{} -Location $Location -Force
        Write-Verbose ($manifest.CloudService | Out-String)
        Write-Verbose "Created cloud service $EnvironmentName"

        # Check the client certificate is present in local store
        $clientCertExists = Test-Path Cert:\LocalMachine\My\${OBACertThumbprint}
        if ([string]::IsNullOrEmpty($OBACertThumbprint) -or !$clientCertExists)
        {
            throw "Certificate with thumbprint ${OBACertThumbprint} not present in my certificate store."
        }
        $clientCert = Get-Item Cert:\LocalMachine\MY\${OBACertThumbprint}

        # Upload client certificate
        $manifest.ServiceCertificate = $clientCert
        Write-Verbose ($manifest.ServiceCertificate | Out-String)
        $manifest.ServiceCertificateAdded = Add-AzureCertificate -ServiceName $EnvironmentName -CertToDeploy $clientCert
        Write-Verbose ($manifest.ServiceCertificateAdded | Out-String)
        Write-Verbose "Uploaded certificate with thumbprint $OBACertThumbprint to $EnvironmentName service"

        # create and provision a signing key
        $manifest.SigningKey = Add-AzureKeyVaultKey -VaultName $EnvironmentName -Name SigningKey -Destination Software
        Write-Verbose ($manifest.SigningKey | Out-String)
        Write-Verbose "Signing key created and provisioned in the key vault $EnvironmentName"

        # set permissions on the keyvault for our OneBusAway service
        Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -ServicePrincipalName $AADOBAAppId `
          -PermissionsToSecrets get,list
        Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -ServicePrincipalName $AADOBAAppId `
          -PermissionsToKeys sign,verify,get
        Write-Verbose "Permissions to key vault $EnvironmentName for OneBusAway application granted"

        # give users access to the keyvault for our OneBusAway service
        Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -UserPrincipalName 'sagarwal@microsoft.com' `
          -PermissionsToKeys sign,verify,get,list,create,delete -PermissionsToSecrets all
        Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -UserPrincipalName 'alecw@microsoft.com' `
          -PermissionsToKeys sign,verify,get,list,create,delete -PermissionsToSecrets all
        Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -UserPrincipalName 'ssaroiu@microsoft.com' `
          -PermissionsToKeys sign,verify,get,list,create,delete -PermissionsToSecrets all
        Write-Verbose "Permissions to key vault $EnvironmentName for administrators granted"

        # create a service bus instance using resource manager
        $manifest.ServiceBus = New-AzureRmResource -ResourceGroupName $ResourceGroup -ResourceName $EnvironmentName `
          -ResourceType "Microsoft.ServiceBus/namespaces" -ApiVersion "2015-08-01" -Sku @{ Name = "Standard"; Tier = "Standard"; }`
          -Location $Location -Force
        Write-Verbose ($manifest.ServiceBus | Out-String)
        $manifest.ServiceBusConnectionString = (Get-AzureSbAuthorizationRule -Namespace $EnvironmentName).ConnectionString
        Write-Verbose ($manifest.ServiceBusConnectionString | Out-String)
        Write-Verbose "Created service bus namespace $EnvironmentName"
    }

    end {
        # Write the manifest to local files with a format of manifest.timestamp.txt and manifest.timestamp.json 
        $manifestObject = New-Object -TypeName PSObject -Property $manifest

        $timestamp = get-date -f yyyy_MM_dd_HH_MM_ss
        $filename = "manifest.$EnvironmentName.$timestamp"
        $txtFilename = $filename + ".txt"
        $jsonFilename = $filename + ".json"        
        $manifestObject | Out-File $txtFilename
        $manifestObject | ConvertTo-Json | Out-File $jsonFilename

        Write-Verbose "Finished."
    }
}
