# Makes the Godot game's assets (godot/assets, which git does not hold) with the extractor as this checkout has it.
# Usage: pwsh -File godot/assets.ps1 [-IfStale]
#
# The extractor reads an installation of the old game. Every checkout reads the same one: the installation the old
# game's verification keeps under the main checkout (csharp/.cache/verification-installed-content), which this never
# changes.
#
# Each time it makes the assets it records, in the checkout's own git folder, what the extractor's sources were
# (every file under csharp/src, committed or not). With -IfStale it makes them only when the checkout has no assets or
# those sources are no longer what was recorded: godot/movie.ps1 runs it so before every movie, so the assets follow
# the extractor however a change to it arrives, by a merge made here or elsewhere, by a commit or by hand.
# It ends with 0 when the assets are made or were already as the extractor makes them, and with 2 on an error; the
# extractor's full output is in godot/.godot/assets.log.
param(
    [switch]$IfStale
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# One hash standing for every file under csharp/src as the checkout has it now, tracked or new, committed or not.
function Get-ExtractorSources([string] $Root) {
    $paths = @(& git -C $Root ls-files --cached --others --exclude-standard -- csharp/src)
    if ($LASTEXITCODE -ne 0) { throw 'git could not list the extractor''s sources.' }
    $present = @($paths | Where-Object { Test-Path -LiteralPath (Join-Path $Root $_) })
    $hashes = @($present | & git -C $Root hash-object --stdin-paths)
    if ($LASTEXITCODE -ne 0 -or $hashes.Count -ne $present.Count) { throw 'git could not hash the extractor''s sources.' }
    $lines = for ($index = 0; $index -lt $present.Count; $index++) { "$($hashes[$index]) $($present[$index])" }
    $bytes = [System.Text.Encoding]::UTF8.GetBytes(($lines -join "`n"))
    return [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes))
}

try {
    $root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    . (Join-Path $root 'tools\project-executable.ps1')

    $gitFolder = (& git -C $root rev-parse --absolute-git-dir)
    if ($LASTEXITCODE -ne 0) { throw "No git folder for $root." }
    $record = Join-Path $gitFolder 'movie-assets-made-from'
    $sources = Get-ExtractorSources $root
    $assets = Join-Path $PSScriptRoot 'assets'
    if ($IfStale -and (Test-Path $assets) -and (Test-Path $record) -and ((Get-Content -Raw $record).Trim() -eq $sources)) {
        exit 0
    }

    $shared = (& git -C $root rev-parse --path-format=absolute --git-common-dir)
    if ($LASTEXITCODE -ne 0 -or -not $shared) { throw "No git repository at $root." }
    $installation = Join-Path (Split-Path -Parent $shared) 'csharp\.cache\verification-installed-content'
    if (-not (Test-Path $installation)) {
        throw "No installation at $installation. The old game's verification makes it in the main checkout."
    }

    $project = Join-Path $root 'csharp\src\SuperMetroid.DebugRunner\SuperMetroid.DebugRunner.csproj'
    & pwsh -NoProfile -NonInteractive -File (Join-Path $root 'tools\build.ps1') $project
    if ($LASTEXITCODE -ne 0) { throw 'The extractor did not build.' }
    $runner = Resolve-ProjectExecutable $project
    if (-not (Test-Path $runner)) { throw "The build left no extractor at $runner." }

    $log = Join-Path $PSScriptRoot '.godot\assets.log'
    New-Item -ItemType Directory -Force -Path (Split-Path $log) | Out-Null
    & $runner assets engine-assets $installation $assets *> $log
    if ($LASTEXITCODE -ne 0) {
        Get-Content $log | Write-Output
        throw "The extractor failed (exit $LASTEXITCODE). Full output: $log"
    }

    Set-Content -Path $record -Value $sources
    Write-Output 'Assets made.'
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 2
}
