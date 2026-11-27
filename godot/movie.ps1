# Builds the Godot project, then plays one recorded movie through the game headless and checks the game against the
# cartridge on every update. The one way a movie is played.
# Usage: pwsh -File godot/movie.ps1 <movie> [-Until N]
#   <movie> is a folder under csharp/test-fixtures/replay-traces (low-13-percent, lowc, full-100-percent, ...), or a path.
#   -Until N stops, as passed, once the movie's update N has been played and found to agree.
# It ends with 0 when the movie agrees as far as it was played, 1 at the first update that differs, 2 on an error.
param(
    [Parameter(Mandatory)][string]$Movie,
    [int]$Until = 0
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

try {
    & pwsh -NoProfile -NonInteractive -File (Join-Path $PSScriptRoot 'build.ps1')
    if ($LASTEXITCODE -ne 0) { exit 2 }

    $godot = $env:GODOT4_BIN
    if (-not $godot) { $godot = [Environment]::GetEnvironmentVariable('GODOT4_BIN', 'User') }
    if (-not $godot) { throw 'GODOT4_BIN is not set. Set it, at user scope, to the Godot .NET executable.' }
    if (-not (Test-Path $godot)) { throw "GODOT4_BIN names a file that does not exist: $godot" }

    $trace = $Movie
    if (-not (Test-Path $trace)) { $trace = Join-Path $PSScriptRoot "..\csharp\test-fixtures\replay-traces\$Movie" }
    if (-not (Test-Path $trace)) { throw "No movie trace at $Movie or under csharp/test-fixtures/replay-traces." }
    $trace = (Resolve-Path $trace).Path

    $out = Join-Path $PSScriptRoot '.godot\movie.out.log'
    $err = Join-Path $PSScriptRoot '.godot\movie.err.log'
    $arguments = @('--headless', '--path', "`"$PSScriptRoot`"", 'res://game/tests/movies/movie_runner.tscn', '--', "`"$trace`"")
    if ($Until -gt 0) { $arguments += @('--until', "$Until") }
    $process = Start-Process -FilePath $godot -ArgumentList $arguments -NoNewWindow -Wait -PassThru `
        -RedirectStandardOutput $out -RedirectStandardError $err

    Get-Content $out | Write-Output
    $errors = Get-Content $err
    if ($errors) { $errors | ForEach-Object { [Console]::Error.WriteLine($_) } }
    exit $process.ExitCode
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 2
}
