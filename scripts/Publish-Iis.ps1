[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $SiteName,

    [Parameter(Mandatory = $true)]
    [string] $PublishPath
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '..\LiveRateApi\LiveRateApi.csproj'
$publishPath = [System.IO.Path]::GetFullPath($PublishPath)

Write-Host "Publishing LiveRateApi to $publishPath"
$stagingPath = "$publishPath.staging"
if (Test-Path -LiteralPath $stagingPath) {
    Remove-Item -LiteralPath $stagingPath -Recurse -Force
}

dotnet publish $project -c Release -o $stagingPath
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$requiredFiles = @('LiveRateApi.dll', 'LiveRateApi.runtimeconfig.json', 'web.config')
foreach ($file in $requiredFiles) {
    $fullPath = Join-Path $stagingPath $file
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "Publish output is incomplete: $fullPath was not created."
    }
}

Import-Module WebAdministration
$sitePath = "IIS:\Sites\$SiteName"
if (-not (Test-Path $sitePath)) {
    throw "IIS site '$SiteName' does not exist. Create its HTTPS binding before running this script."
}

$module = Get-WebGlobalModule -Name AspNetCoreModuleV2 -ErrorAction SilentlyContinue
if ($null -eq $module) {
    throw 'AspNetCoreModuleV2 is not installed. Install the .NET 8 Hosting Bundle and run iisreset.'
}

$applicationPool = (Get-Item $sitePath).applicationPool
Stop-WebAppPool -Name $applicationPool

# Do not leave assemblies from a previous application in the IIS directory.
# Seeing LiveExchangeRatesAPI in logs after this deployment proves IIS is still
# using a different physical path/site or an old worker process.
if (Test-Path -LiteralPath $publishPath) {
    Remove-Item -LiteralPath $publishPath -Recurse -Force
}
Move-Item -LiteralPath $stagingPath -Destination $publishPath

Set-ItemProperty $sitePath -Name physicalPath -Value $publishPath
Set-ItemProperty "IIS:\AppPools\$applicationPool" -Name managedRuntimeVersion -Value ''

Set-WebConfigurationProperty `
    -PSPath 'MACHINE/WEBROOT/APPHOST' `
    -Location $SiteName `
    -Filter 'system.webServer/security/authentication/anonymousAuthentication' `
    -Name enabled `
    -Value $true

Restart-WebAppPool -Name $applicationPool

Write-Host "Deployment complete. IIS site '$SiteName' now points to $publishPath."
Write-Host "Test with: curl.exe -i http://localhost/health -H `"Host: <your-host-name>`""
