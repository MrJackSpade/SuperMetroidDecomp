# Builds a verification project and only then runs it, so a run never uses a binary older than the source (owner rule,
# 2026-11-11): a failed build runs nothing and exits with the build's code. The run is at Idle priority so it does not
# starve other work.
# Usage: pwsh -File tools/run-verification.ps1 <Verification|RenderVerification|DesktopVerification|IntegrationVerification> [arguments...]
param(
    [Parameter(Mandatory)][ValidateSet('Verification', 'RenderVerification', 'DesktopVerification', 'IntegrationVerification')][string] $Project,
    [Parameter(ValueFromRemainingArguments)] [string[]] $Arguments = @())
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
try {
    $projectDirectory = Join-Path $repo "csharp/src/SuperMetroid.$Project"
    & pwsh -File (Join-Path $PSScriptRoot 'build.ps1') $projectDirectory
    if ($LASTEXITCODE -ne 0) {
        [Console]::Error.WriteLine("The build of SuperMetroid.$Project failed; nothing was run.")
        exit $LASTEXITCODE
    }
    $executables = @(Get-ChildItem -Path (Join-Path $projectDirectory 'bin/Debug/net10.0-windows') -Filter "SuperMetroid.$Project.exe")
    if ($executables.Count -ne 1) {
        throw "Expected exactly one SuperMetroid.$Project.exe under bin/Debug/net10.0-windows, found $($executables.Count)."
    }
    $executable = $executables[0]
    Write-Output "running $($executable.FullName) (built $($executable.LastWriteTime))"
    $start = @{ FilePath = $executable.FullName; WorkingDirectory = $repo; NoNewWindow = $true; PassThru = $true }
    if ($Arguments.Count -gt 0) { $start.ArgumentList = $Arguments }
    $process = Start-Process @start
    $process.PriorityClass = 'Idle'
    $process.WaitForExit()
    exit $process.ExitCode
}
catch {
    [Console]::Error.WriteLine(($_ | Out-String))
    exit 1
}
