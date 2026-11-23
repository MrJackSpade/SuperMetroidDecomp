# Builds the Godot project's C# assembly. The one way the project is built from the command line.
# Usage: pwsh -File godot/build.ps1
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

try {
    $project = Join-Path $PSScriptRoot 'Engine.csproj'
    $log = Join-Path $PSScriptRoot '.godot\build.log'
    New-Item -ItemType Directory -Force -Path (Split-Path $log) | Out-Null

    & dotnet build $project --configuration Debug --nologo -consoleloggerparameters:NoSummary *> $log
    $exit = $LASTEXITCODE

    $problems = Select-String -Path $log -Pattern ': (error|warning) ' | ForEach-Object { $_.Line.Trim() } | Sort-Object -Unique
    if ($problems) { $problems | ForEach-Object { Write-Output $_ } }
    if ($exit -ne 0) {
        if (-not $problems) { Get-Content $log | Write-Output }
        [Console]::Error.WriteLine("Build failed (exit $exit). Full output: $log")
        exit 1
    }
    Write-Output "Build succeeded."
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 1
}
