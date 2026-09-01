param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot 'SuperMetroid.AudioNative.vcxproj'
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswherePath)) {
    throw "Visual Studio Installer's vswhere.exe was not found at '$vswherePath'."
}

$installationPath = & $vswherePath `
    -latest `
    -products '*' `
    -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 `
    -property installationPath
if ([string]::IsNullOrWhiteSpace($installationPath)) {
    throw 'Visual Studio with the Desktop development with C++ workload is required for cartridge audio.'
}

$msbuildPath = Join-Path $installationPath 'MSBuild\Current\Bin\MSBuild.exe'
if (-not (Test-Path -LiteralPath $msbuildPath)) {
    throw "MSBuild was not found at '$msbuildPath'."
}

& $msbuildPath $projectPath "/p:Configuration=$Configuration" '/p:Platform=x64' '/m' '/nologo'
if ($LASTEXITCODE -ne 0) {
    throw "Native audio build failed with exit code $LASTEXITCODE."
}
