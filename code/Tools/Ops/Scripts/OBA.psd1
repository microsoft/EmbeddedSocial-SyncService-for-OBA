#
# Module manifest for module 'OBA'
#

@{

# Script module or binary module file associated with this manifest.
# RootModule = ''

# Version number of this module.
ModuleVersion = '1.0'

# ID used to uniquely identify this module
GUID = '65d4c4c3-cab6-49e2-ac32-146c0433a7d0'

# Author of this module
Author = 'OBA Server developers'

# Company or vendor of this module
CompanyName = 'Microsoft'

# Copyright statement for this module
Copyright = '(c) 2016 Microsoft. All rights reserved.'

# Description of the functionality provided by this module
Description = 'Powershell scripts to manage OneBusAway Server Azure environments.'

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
NestedModules = @(
     'Format-OBAManifest.ps1',
     'New-OBAEnvironment.ps1',
     'Remove-OBAEnvironment.ps1',
     'Login-OBAAzure.ps1'
)

# Functions to export from this module
FunctionsToExport = '*'

# Cmdlets to export from this module
CmdletsToExport = '*'

# Variables to export from this module
VariablesToExport = '*'

# Aliases to export from this module
AliasesToExport = '*'

# HelpInfo URI of this module
# HelpInfoURI = ''

}

