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

if ($version -ne 1) {
    throw "Only SMV v1 is supported; this movie is v$version."
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

[pscustomobject]@{
    Version = $version
    FrameCount = $frameCount
    ControllerSamples = $sampleCount
    ControllerCount = $controllerCount
    SnapshotOffset = $snapshotOffset
    ControllerOffset = $controllerOffset
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

