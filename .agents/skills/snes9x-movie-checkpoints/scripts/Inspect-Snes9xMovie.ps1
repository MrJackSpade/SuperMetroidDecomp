param(
    [Parameter(Mandatory = $true)]
    [string] $MoviePath,

    [string] $InputCsvPath
)

$ErrorActionPreference = 'Stop'

$movie = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $MoviePath))
if ($movie.Length -lt 32 -or [BitConverter]::ToUInt32($movie, 0) -ne 0x1a564d53) {
    throw 'The input is not an SMV movie.'
}

$version = [BitConverter]::ToUInt32($movie, 4)
$frameCount = [BitConverter]::ToUInt32($movie, 16)
$controllerCount = [int] $movie[20]
$snapshotOffset = [BitConverter]::ToUInt32($movie, 24)
$controllerOffset = [BitConverter]::ToUInt32($movie, 28)

if ($version -notin 1, 4, 5) {
    throw "Only SMV v1, v4 and v5 inspection is supported; this movie is v$version."
}
if ($controllerCount -ne 1) {
    throw "Only one-controller movies are supported; this movie records $controllerCount controllers."
}
if ($snapshotOffset -ge $movie.Length -or $controllerOffset -ge $movie.Length) {
    throw 'The SMV snapshot or controller-data offset is outside the file.'
}

$sampleCount = [int64] $frameCount + 1
$controllerEnd = [int64] $controllerOffset + 2 * $sampleCount
if ($controllerEnd -gt $movie.Length) {
    throw 'The SMV controller stream is truncated.'
}

$snapshotBytes = [byte[]]::new($controllerOffset - $snapshotOffset)
[Array]::Copy($movie, $snapshotOffset, $snapshotBytes, 0, $snapshotBytes.Length)
$snapshotInput = [IO.MemoryStream]::new($snapshotBytes)
$gzip = [IO.Compression.GZipStream]::new(
    $snapshotInput,
    [IO.Compression.CompressionMode]::Decompress)
$snapshotOutput = [IO.MemoryStream]::new()
try {
    $gzip.CopyTo($snapshotOutput)
    $snapshot = $snapshotOutput.ToArray()
}
finally {
    $gzip.Dispose()
    $snapshotInput.Dispose()
    $snapshotOutput.Dispose()
}

$recordedRomName = $null
$position = [Array]::IndexOf($snapshot, [byte] 10) + 1
while ($position + 11 -le $snapshot.Length) {
    $name = [Text.Encoding]::ASCII.GetString($snapshot, $position, 3)
    $size = 0
    $sizeText = [Text.Encoding]::ASCII.GetString($snapshot, $position + 4, 6)
    if (-not [int]::TryParse($sizeText, [ref] $size) -or
        $size -lt 0 -or
        $position + 11 + $size -gt $snapshot.Length) {
        throw "Invalid embedded snapshot block at offset $position."
    }
    if ($name -eq 'NAM') {
        $rawName = [Text.Encoding]::ASCII.GetString($snapshot, $position + 11, $size)
        $recordedRomName = $rawName.TrimEnd([char] 0)
        break
    }
    $position += 11 + $size
}

[pscustomobject]@{
    Version = $version
    FrameCount = $frameCount
    ControllerSamples = $sampleCount
    ControllerCount = $controllerCount
    SnapshotOffset = $snapshotOffset
    ControllerOffset = $controllerOffset
    RecordedRomName = $recordedRomName
    FileLength = $movie.Length
}

if ($InputCsvPath) {
    $rows = [Collections.Generic.List[string]]::new()
    $rows.Add('frame,input')
    for ($frame = 0; $frame -lt $sampleCount; $frame++) {
        $input = [BitConverter]::ToUInt16($movie, [int] ($controllerOffset + 2 * $frame))
        $rows.Add(('{0},{1:X4}' -f $frame, $input))
    }

    $parent = Split-Path -Parent $InputCsvPath
    if ($parent) {
        [IO.Directory]::CreateDirectory($parent) | Out-Null
    }
    [IO.File]::WriteAllLines($InputCsvPath, $rows)
}
