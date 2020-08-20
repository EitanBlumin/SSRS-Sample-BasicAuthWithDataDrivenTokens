param (
[string] $SourceFolder = "SSRS",
[string] $TargetReportServerUri = "http://localhost:8081/ReportServer",
[string] $TargetFolder = "SSRS-Sample-DataDrivenTokens"
)

$ErrorActionPreference = "Stop"

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 
Install-Module PowerShellGet -RequiredVersion 2.2.4 -SkipPublisherCheck

if ($SourceFolder -eq "") {
    $SourceFolder = $(Get-Location).Path + "\"
}

if (!$SourceFolder.EndsWith("\"))
{
    $SourceFolder = $SourceFolder + "\"
}

Write-Output "====================================================================================="
Write-Output "                             Deploying SSRS Reports"
Write-Output "Source Folder: $SourceFolder"
Write-Output "Target Server: $TargetReportServerUri"
Write-Output "Target Folder: $TargetFolder"
Write-Output "====================================================================================="

Write-Output "Marking PSGallery as Trusted..."
Set-PSRepository -Name "PSGallery" -InstallationPolicy Trusted

Write-Output "Installing ReportingServicesTools Module..."
Install-Module -Name ReportingServicesTools | Out-Null

Write-Output "Requesting RSTools..."
Invoke-Expression (Invoke-WebRequest https://aka.ms/rstools)

#Get-Command -Module ReportingServicesTools

Write-Output "Creating Folder: $TargetFolder"
New-RsFolder -ReportServerUri $TargetReportServerUri -Path / -Name $TargetFolder -Verbose -ErrorAction SilentlyContinue

$TargetFolder = "/" + $TargetFolder

Write-Output "Deploying Data Source files from: $SourceFolder"
DIR $SourceFolder -Filter *.rds | % { $_.FullName } |
    Write-RsCatalogItem -ReportServerUri $TargetReportServerUri -Destination $TargetFolder -Verbose -Overwrite

Write-Output "Deploying Data Set files from: $SourceFolder"
DIR $SourceFolder -Filter *.rsd | % { $_.FullName } |
    Write-RsCatalogItem -ReportServerUri $TargetReportServerUri -Destination $TargetFolder -Verbose -Overwrite

Write-Output "Deploying Report Definition files from: $SourceFolder"
DIR $SourceFolder -Filter *.rdl | % { $_.FullName } |
    Write-RsCatalogItem -ReportServerUri $TargetReportServerUri -Destination $TargetFolder -Verbose -Overwrite