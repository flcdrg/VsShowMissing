$PersonalAccessToken = $args[0]

$ErrorActionPreference = "Stop"

# TODO: Replace the path with the path to your VSIX file
$VsixPath = "$PSScriptRoot\..\VS2019\bin\Release\Gardiner.VsShowMissing.VS2019.vsix"
$ManifestPath = "$PSScriptRoot\extension-manifest.json"

# Find the location of VsixPublisher
$Installation = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -prerelease -format json | ConvertFrom-Json
$Path = $Installation.installationPath

Write-Host $Path
$VsixPublisher = Join-Path -Path $Path -ChildPath "VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe" -Resolve

Write-Host $VsixPublisher

# Publish to VSIX to the marketplace
& $VsixPublisher publish -payload $VsixPath -publishManifest $ManifestPath -personalAccessToken $PersonalAccessToken -ignoreWarnings "VSIXValidatorWarning01,VSIXValidatorWarning02,VSIXValidatorWarning08"