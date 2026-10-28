#requires -Version 7
<#
.SYNOPSIS
Regenerates one committed compact replay trace from its movie and the pinned ROM (#1442).

.DESCRIPTION
Runs the native capture, the update converter and the compactor into a temporary work directory, then replaces
csharp/test-fixtures/replay-traces/NAME with the compact trace. Full captures are never kept: once the compactor has
verified the compact trace record by record against the capture, the work directory and every capture stream in it
are deleted. If any step fails, the script stops with that step's error and exit code, and the work directory is kept
for diagnosis; nothing is deleted on failure. No step is given a time limit.

.PARAMETER Movie
The supplied movie: a Snes9x .smv or an lsnes .lsmv. It is read, never modified.

.PARAMETER Rom
The pinned J/U ROM; the converter checks its hash.

.PARAMETER Name
The committed trace's directory name under csharp/test-fixtures/replay-traces.

.PARAMETER Capture
The built native capture adapter for the movie's format: tools/smv-native-capture for .smv, tools/lsmv-native-capture
for .lsmv (see their READMEs).

.PARAMETER Verifier
The built SuperMetroid.Verification executable, which runs --compact-replay-trace.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Movie,
    [Parameter(Mandatory)][string]$Rom,
    [Parameter(Mandatory)][string]$Name,
    [Parameter(Mandatory)][string]$Capture,
    [string]$Verifier = (Join-Path $PSScriptRoot '..\csharp\src\SuperMetroid.Verification\bin\Release\net10.0-windows\SuperMetroid.Verification.exe')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repository = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$movie = (Resolve-Path $Movie).Path
$rom = (Resolve-Path $Rom).Path
$captureTool = (Resolve-Path $Capture).Path
$verifierTool = (Resolve-Path $Verifier).Path
$trace = Join-Path $repository "csharp\test-fixtures\replay-traces\$Name"
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("replay-trace-$Name-" + [Guid]::NewGuid().ToString('N'))
$captured = Join-Path $work 'capture'
$compact = Join-Path $work 'compact'
New-Item -ItemType Directory -Path $captured | Out-Null

# Runs one native step and stops the regeneration with its name and exit code when it fails; the work directory stays.
function Invoke-Step([string]$step, [string]$tool, [string[]]$arguments) {
    Write-Host "== $step"
    & $tool @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Step '$step' failed with exit code $LASTEXITCODE. The work directory is kept for diagnosis: $work"
    }
}

switch ([System.IO.Path]::GetExtension($movie).ToLowerInvariant()) {
    '.smv' {
        # The snapshot list names source frames whose WRAM is also written whole; the terminal frame always is.
        Invoke-Step 'native capture' $captureTool @($rom, $movie, $captured, '0')
    }
    '.lsmv' {
        $frames = Join-Path $work 'frames.bin'
        Invoke-Step 'lsmv frame extraction' 'python' @((Join-Path $repository 'tools\lsmv-native-capture\lsmv-extract.py'), $movie, '--rom', $rom, '--output', $frames)
        Invoke-Step 'native capture' $captureTool @($rom, $frames, $captured)
    }
    default { throw "Movie $movie is neither .smv nor .lsmv." }
}
Invoke-Step 'update conversion' 'python' @((Join-Path $repository 'tools\convert-smv-updates.py'), $movie, $captured, '--rom', $rom, '--output', (Join-Path $captured 'updates.json'))
# The compactor writes into an empty directory and verifies every record against the capture before it returns.
Invoke-Step 'compaction' $verifierTool @('--compact-replay-trace', $captured, $compact)

Write-Host "== replace $trace"
if (Test-Path $trace) {
    Get-ChildItem -LiteralPath $trace -Force | Remove-Item -Recurse -Force
}
else {
    New-Item -ItemType Directory -Path $trace | Out-Null
}
Get-ChildItem -LiteralPath $compact -Force | Copy-Item -Destination $trace

# The compact trace is verified and committed in place; the full capture is never kept.
Remove-Item -LiteralPath $work -Recurse -Force
Write-Host "Regenerated $trace; the full capture was removed."
