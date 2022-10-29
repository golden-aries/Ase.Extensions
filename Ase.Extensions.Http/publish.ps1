
$ErrorActionPreference = 'Stop'

dotnet pack --configuration Release

# defaultPushSource  key should be defined in Nuget.config for the following command to work
dotnet nuget push bin/Release/Ase.Extensions.Http.1.0.5.nupkg --api-key AzureDevOps