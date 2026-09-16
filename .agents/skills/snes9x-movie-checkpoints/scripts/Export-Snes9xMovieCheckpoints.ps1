param(
    [Parameter(Mandatory = $true)]
    [string] $MoviePath,

    [Parameter(Mandatory = $true)]
    [string] $RomPath,

    [Parameter(Mandatory = $true)]
    [string] $Snes9xPath,

    [Parameter(Mandatory = $true)]
    [string] $OutputDirectory,

    [Parameter(Mandatory = $true)]
    [int[]] $Frames,

    [switch] $KeepNativeSnapshots
)

$ErrorActionPreference = 'Stop'

function Read-SnapshotWram([string] $Path) {
    $stream = [IO.File]::OpenRead($Path)
    $gzip = [IO.Compression.GZipStream]::new(
        $stream,
        [IO.Compression.CompressionMode]::Decompress)
    $output = [IO.MemoryStream]::new()
    try {
        $gzip.CopyTo($output)
        $snapshot = $output.ToArray()
    }
    finally {
        $gzip.Dispose()
        $stream.Dispose()
        $output.Dispose()
    }

    $position = [Array]::IndexOf($snapshot, [byte] 10) + 1
    while ($position + 11 -le $snapshot.Length) {
        $name = [Text.Encoding]::ASCII.GetString($snapshot, $position, 3)
        $sizeText = [Text.Encoding]::ASCII.GetString($snapshot, $position + 4, 6)
        $size = 0
        if (-not [int]::TryParse($sizeText, [ref] $size) -or
            $size -lt 0 -or
            $position + 11 + $size -gt $snapshot.Length) {
            throw "Invalid native snapshot block at offset $position in $Path."
        }

        if ($name -eq 'RAM') {
            if ($size -ne 131072) {
                throw "Expected 128 KiB WRAM in $Path, found $size bytes."
            }
            $wram = [byte[]]::new($size)
            [Array]::Copy($snapshot, $position + 11, $wram, 0, $size)
            return ,$wram
        }
        $position += 11 + $size
    }
    throw "Native snapshot $Path has no RAM block."
}

$resolvedMovie = (Resolve-Path -LiteralPath $MoviePath).Path
$resolvedRom = (Resolve-Path -LiteralPath $RomPath).Path
$resolvedSnes9x = (Resolve-Path -LiteralPath $Snes9xPath).Path
$movie = [IO.File]::ReadAllBytes($resolvedMovie)

if ($movie.Length -lt 32 -or [BitConverter]::ToUInt32($movie, 0) -ne 0x1a564d53) {
    throw 'The input is not an SMV movie.'
}
$version = [BitConverter]::ToUInt32($movie, 4)
$frameCount = [BitConverter]::ToUInt32($movie, 16)
$controllerCount = [int] $movie[20]
$controllerOffset = [BitConverter]::ToUInt32($movie, 28)
if ($version -ne 1 -or $controllerCount -ne 1) {
    throw "Expected an SMV v1 one-controller movie; found v$version with $controllerCount controllers."
}
if ([int64] $controllerOffset + 2 * ([int64] $frameCount + 1) -gt $movie.Length) {
    throw 'The SMV controller stream is truncated.'
}

$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)
[IO.Directory]::CreateDirectory($outputRoot) | Out-Null

$inputs = [Collections.Generic.List[string]]::new()
$inputs.Add('frame,input')
for ($frame = 0; $frame -le $frameCount; $frame++) {
    $input = [BitConverter]::ToUInt16($movie, [int] ($controllerOffset + 2 * $frame))
    $inputs.Add(('{0},{1:X4}' -f $frame, $input))
}
[IO.File]::WriteAllLines((Join-Path $outputRoot 'inputs.csv'), $inputs)

$manifest = [Collections.Generic.List[string]]::new()
$manifest.Add('frame,wram')
foreach ($frame in ($Frames | Sort-Object -Unique)) {
    if ($frame -lt 0 -or $frame -gt $frameCount) {
        throw "Checkpoint frame $frame is outside 0..$frameCount."
    }

    $shortMovie = [byte[]] $movie.Clone()
    [BitConverter]::GetBytes([uint32] ($frame + 1)).CopyTo($shortMovie, 16)
    $shortMoviePath = Join-Path $outputRoot ("checkpoint-{0}.smv" -f $frame)
    [IO.File]::WriteAllBytes($shortMoviePath, $shortMovie)

    $captureDirectory = Join-Path $outputRoot ("state-{0}" -f $frame)
    [IO.Directory]::CreateDirectory($captureDirectory) | Out-Null
    $startInfo = [Diagnostics.ProcessStartInfo]::new($resolvedSnes9x)
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.WorkingDirectory = Split-Path -Parent $resolvedSnes9x
    $startInfo.ArgumentList.Add('--rip')
    $startInfo.ArgumentList.Add($resolvedRom)
    $startInfo.ArgumentList.Add($shortMoviePath)
    $startInfo.ArgumentList.Add($captureDirectory)
    $startInfo.ArgumentList.Add([string] $frame)
    $process = [Diagnostics.Process]::Start($startInfo)
    $process.WaitForExit()
    if ($process.ExitCode -ne 0) {
        throw "Snes9x checkpoint capture failed at frame $frame with exit code $($process.ExitCode)."
    }

    $snapshotPath = Join-Path $captureDirectory 'rip_rollback.state'
    if (-not [IO.File]::Exists($snapshotPath)) {
        throw "Snes9x did not produce $snapshotPath."
    }
    $wramPath = Join-Path $outputRoot ("frame-{0}.wram" -f $frame)
    [IO.File]::WriteAllBytes($wramPath, (Read-SnapshotWram $snapshotPath))
    $manifest.Add(('{0},{1}' -f $frame, [IO.Path]::GetFileName($wramPath)))

    [IO.File]::Delete($shortMoviePath)
    if (-not $KeepNativeSnapshots) {
        [IO.Directory]::Delete($captureDirectory, $true)
    }
}

[IO.File]::WriteAllLines((Join-Path $outputRoot 'manifest.csv'), $manifest)
Write-Output ("Exported {0} independent native pre-frame WRAM checkpoints." -f ($manifest.Count - 1))
