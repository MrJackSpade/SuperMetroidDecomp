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
    # What compiles can still fail in the editor: a class made there ([Tool]) that holds a script type that is not is
    # handed a placeholder it cannot take. The sources are checked for that, and the build fails on it.
    & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'check-editor-types.ps1')
    if ($LASTEXITCODE -ne 0) {
        [Console]::Error.WriteLine("Build failed: the editor could not load what is listed above.")
        exit 1
    }

    Write-Output "Build succeeded."
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 1
}
