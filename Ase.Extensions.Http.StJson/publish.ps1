
$ErrorActionPreference = 'Stop'

dotnet pack --configuration Release

# defaultPushSource  key should be defined in Nuget.config for the following command to work
dotnet nuget push bin/Release/Ase.Extensions.Http.StJson.1.0.6.nupkg --api-key AzureDevOps