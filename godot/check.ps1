# Builds the Godot project, then runs its checks headless through the one test-runner scene. The one way checks are run.
# Usage: pwsh -File godot/check.ps1 [name ...]
#   With no names, every check runs. With names, only checks whose class or method name contains one of them run.
# Godot runs with --headless: no window is opened and nothing is drawn.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

try {
    & pwsh -NoProfile -NonInteractive -File (Join-Path $PSScriptRoot 'build.ps1')
    if ($LASTEXITCODE -ne 0) { exit 1 }

    $godot = $env:GODOT4_BIN
    if (-not $godot) { $godot = [Environment]::GetEnvironmentVariable('GODOT4_BIN', 'User') }
    if (-not $godot) { throw 'GODOT4_BIN is not set. Set it, at user scope, to the Godot .NET executable.' }
    if (-not (Test-Path $godot)) { throw "GODOT4_BIN names a file that does not exist: $godot" }

    $out = Join-Path $PSScriptRoot '.godot\check.out.log'
    $err = Join-Path $PSScriptRoot '.godot\check.err.log'
    $arguments = @('--headless', '--path', "`"$PSScriptRoot`"", 'res://engine/tests/test_runner.tscn', '--') + $args
    $process = Start-Process -FilePath $godot -ArgumentList $arguments -NoNewWindow -Wait -PassThru `
        -RedirectStandardOutput $out -RedirectStandardError $err

    Get-Content $out | Write-Output
    $errors = Get-Content $err
    if ($errors) { $errors | ForEach-Object { [Console]::Error.WriteLine($_) } }
    exit $process.ExitCode
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 1
}
