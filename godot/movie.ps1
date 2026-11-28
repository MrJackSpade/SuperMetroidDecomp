# Merges the other movie checkouts' committed work into this one (godot/sync.ps1), makes the assets again where the
# extractor's sources are no longer what they were made from (godot/assets.ps1 -IfStale), builds the Godot project, then plays one recorded movie through
# the game headless and checks the game against the cartridge on every update. The one way a movie is played.
# Usage: pwsh -File godot/movie.ps1 <movie> [-Until N] [-NoBuild]
#   <movie> is a folder under csharp/test-fixtures/replay-traces (low-13-percent, lowc, full-100-percent, ...), or a path.
#   -Until N stops, as agreeing, once the movie's update N has been played and found to agree.
#   -NoBuild plays the project as it was last built, merging nothing: for a second movie when no code has changed
#   since the first.
# It ends with 0 when the movie agrees as far as it was played, 1 at the first update that differs, 2 on an error,
# 3 when every update agrees but kinds of enemy the movie met are not compared yet, and 4 when another checkout's
# work did not merge by itself and nothing was played.
param(
    [Parameter(Mandatory)][string]$Movie,
    [int]$Until = 0,
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

try {
    if (-not $NoBuild) {
        & pwsh -NoProfile -NonInteractive -File (Join-Path $PSScriptRoot 'sync.ps1')
        if ($LASTEXITCODE -eq 1) { exit 4 }
        if ($LASTEXITCODE -ne 0) { exit 2 }

        & pwsh -NoProfile -NonInteractive -File (Join-Path $PSScriptRoot 'assets.ps1') -IfStale
        if ($LASTEXITCODE -ne 0) { exit 2 }

        & pwsh -NoProfile -NonInteractive -File (Join-Path $PSScriptRoot 'build.ps1')
        if ($LASTEXITCODE -ne 0) { exit 2 }
    }

    $godot = $env:GODOT4_BIN
    if (-not $godot) { $godot = [Environment]::GetEnvironmentVariable('GODOT4_BIN', 'User') }
    if (-not $godot) { throw 'GODOT4_BIN is not set. Set it, at user scope, to the Godot .NET executable.' }
    if (-not (Test-Path $godot)) { throw "GODOT4_BIN names a file that does not exist: $godot" }

    $trace = $Movie
    if (-not (Test-Path $trace)) { $trace = Join-Path $PSScriptRoot "..\csharp\test-fixtures\replay-traces\$Movie" }
    if (-not (Test-Path $trace)) { throw "No movie trace at $Movie or under csharp/test-fixtures/replay-traces." }
    $trace = (Resolve-Path $trace).Path

    # Each movie has logs of its own, so that two movies can be played at once.
    $name = Split-Path $trace -Leaf
    $out = Join-Path $PSScriptRoot ".godot\movie.$name.out.log"
    $err = Join-Path $PSScriptRoot ".godot\movie.$name.err.log"
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
