# Makes the Godot game's assets (godot/assets, which git does not hold) with the extractor as this checkout has it.
# Usage: pwsh -File godot/assets.ps1
#
# The extractor reads an installation of the old game. Every checkout reads the same one: the installation the old
# game's verification keeps under the main checkout (csharp/.cache/verification-installed-content), which this never
# changes. godot/movie.ps1 runs this when the checkout has no assets yet, or when godot/sync.ps1 has merged a change
# to the extractor's sources; a change made to the extractor by hand is followed by running it by hand.
# It ends with 0 when the assets are made, and with 2 on an error; the extractor's full output is in
# godot/.godot/assets.log.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

try {
    $root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    . (Join-Path $root 'tools\project-executable.ps1')

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
    & $runner assets engine-assets $installation (Join-Path $PSScriptRoot 'assets') *> $log
    if ($LASTEXITCODE -ne 0) {
        Get-Content $log | Write-Output
        throw "The extractor failed (exit $LASTEXITCODE). Full output: $log"
    }

    $gitFolder = (& git -C $root rev-parse --absolute-git-dir)
    if ($LASTEXITCODE -ne 0) { throw "No git folder for $root." }
    Remove-Item -Path (Join-Path $gitFolder 'movie-assets-stale') -ErrorAction Ignore
    Write-Output 'Assets made.'
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 2
}
