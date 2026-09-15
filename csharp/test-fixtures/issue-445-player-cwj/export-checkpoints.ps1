param(
    [Parameter(Mandatory=$true)][string]$CaptureDirectory,
    [string]$Destination = $PSScriptRoot
)
$ErrorActionPreference = 'Stop'
# Read-only inspection of Snes9x 1.43's native gzip snapshot blocks. This does not
# convert our managed debugger state or infer missing native state.
function Read-Wram([string]$path) {
    $stream = [IO.File]::OpenRead($path)
    $gzip = [IO.Compression.GZipStream]::new($stream,[IO.Compression.CompressionMode]::Decompress)
    $output = [IO.MemoryStream]::new()
    try { $gzip.CopyTo($output); $bytes=$output.ToArray() }
    finally { $gzip.Dispose(); $stream.Dispose(); $output.Dispose() }
    $pos=[Array]::IndexOf($bytes,[byte]10)+1
    while($pos+11 -le $bytes.Length) {
        $name=[Text.Encoding]::ASCII.GetString($bytes,$pos,3)
        $size=[int][Text.Encoding]::ASCII.GetString($bytes,$pos+4,6)
        if($size -lt 0 -or $pos+11+$size -gt $bytes.Length) { throw 'Invalid snapshot block' }
        if($name -eq 'RAM') {
            if($size -ne 131072) { throw 'Invalid WRAM size' }
            $ram=[byte[]]::new($size)
            [Array]::Copy($bytes,$pos+11,$ram,0,$size)
            return ,$ram
        }
        $pos+=11+$size
    }
    throw 'Native snapshot has no RAM block'
}
New-Item -ItemType Directory -Path $Destination -Force | Out-Null
$movie=[IO.File]::ReadAllBytes((Join-Path $CaptureDirectory 'cwj.smv'))
if([BitConverter]::ToUInt32($movie,0) -ne 0x1a564d53 -or
   [BitConverter]::ToUInt32($movie,4) -ne 1 -or
   [BitConverter]::ToUInt32($movie,16) -ne 532 -or $movie[20] -ne 1) {
    throw 'Expected the 532-frame, single-controller SMV v1 supplied for #445'
}
$inputs=[Collections.Generic.List[string]]::new()
$inputs.Add('frame,input')
$offset=[BitConverter]::ToUInt32($movie,28)
for($frame=0;$frame -le 532;$frame++) {
    $inputs.Add(('{0},{1:X4}' -f $frame,[BitConverter]::ToUInt16($movie,$offset+2*$frame)))
}
$rows=[Collections.Generic.List[string]]::new()
$rows.Add('frame,x,y,pose,base,extra,animation,timer,vertical,direction,acceleration')
$transitionRows=[Collections.Generic.List[string]]::new()
$transitionRows.Add('frame,gameState,room,door,x,y,pose,base,extra,cameraX,cameraY')
foreach($frame in 100..269) {
    $ram=Read-Wram (Join-Path $CaptureDirectory "state$frame/rip_rollback.state")
    function Word([int]$address) { [BitConverter]::ToUInt16($ram,$address) }
    $transitionRows.Add(('{0},{1:X4},{2:X4},{3:X4},{4:X4}{5:X4},{6:X4}{7:X4},{8:X2},{9:X4}{10:X4},{11:X4}{12:X4},{13:X4},{14:X4}' -f
        $frame,(Word 0x0998),(Word 0x079b),(Word 0x078d),
        (Word 0xaf6),(Word 0xaf8),(Word 0xafa),(Word 0xafc),(Word 0xa1c),
        (Word 0xb46),(Word 0xb48),(Word 0xb42),(Word 0xb44),(Word 0x0911),(Word 0x0915)))
    if($frame -eq 102) { [IO.File]::WriteAllBytes((Join-Path $Destination 'frame-102.wram'),$ram) }
}
foreach($frame in 270..531) {
    $ram=Read-Wram (Join-Path $CaptureDirectory "state$frame/rip_rollback.state")
    function Word([int]$address) { [BitConverter]::ToUInt16($ram,$address) }
    # Field identities are also catalogued in MoatMovieMemory.cs. The native
    # snapshot is before this numbered emulator frame, not after its input.
    $rows.Add(('{0},{1:X4}{2:X4},{3:X4}{4:X4},{5:X2},{6:X4}{7:X4},{8:X4}{9:X4},{10:X4},{11:X4},{12:X4}{13:X4},{14:X4},{15:X4}' -f
        $frame,(Word 0xaf6),(Word 0xaf8),(Word 0xafa),(Word 0xafc),(Word 0xa1c),
        (Word 0xb46),(Word 0xb48),(Word 0xb42),(Word 0xb44),(Word 0xa96),(Word 0xa94),
        (Word 0xb2e),(Word 0xb2c),(Word 0xb36),(Word 0xb4a)))
    if($frame -eq 270) { [IO.File]::WriteAllBytes((Join-Path $Destination 'frame-270.wram'),$ram) }
}
[IO.File]::WriteAllLines((Join-Path $Destination 'inputs.csv'),$inputs)
[IO.File]::WriteAllLines((Join-Path $Destination 'native.csv'),$rows)
[IO.File]::WriteAllLines((Join-Path $Destination 'native-transition.csv'),$transitionRows)
Copy-Item -LiteralPath (Join-Path $CaptureDirectory 'cwj.smv') -Destination (Join-Path $Destination 'cwj.smv')
Write-Output 'Exported 170 transition checkpoints, 262 room-local checkpoints, and the original movie.'
