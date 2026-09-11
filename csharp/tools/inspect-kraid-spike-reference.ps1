param(
    [string]$StatePath = "$PSScriptRoot/../test-fixtures/issue-269-kraid-spike-destruction/SuperMetroid.001"
)
$ErrorActionPreference = 'Stop'
# Read-only inspection of Snes9x's tagged freeze blocks. This is not a replay,
# revision detector, or converter into the port's debugger format.
$inputStream = [IO.File]::OpenRead((Resolve-Path -LiteralPath $StatePath))
$gzip = [IO.Compression.GZipStream]::new($inputStream, [IO.Compression.CompressionMode]::Decompress)
$decoded = [IO.MemoryStream]::new()
try { $gzip.CopyTo($decoded); $bytes = $decoded.ToArray() }
finally { $decoded.Dispose(); $gzip.Dispose(); $inputStream.Dispose() }
if ([Text.Encoding]::ASCII.GetString($bytes, 0, 14) -ne "#!snes9x:1510`n") {
    throw 'This diagnostic expects Snes9x freeze version 1510.'
}
$ram = $null
for ($offset = 14; $offset -lt $bytes.Length;) {
    if ($offset + 11 -gt $bytes.Length) { throw 'Truncated freeze block header.' }
    $header = [Text.Encoding]::ASCII.GetString($bytes, $offset, 11)
    if ($header -notmatch '^[A-Z]{3}:\d{6}:$') { throw 'Invalid freeze block header.' }
    $length = [int]$header.Substring(4, 6)
    $offset += 11
    if ($offset + $length -gt $bytes.Length) { throw 'Truncated freeze block payload.' }
    if ($header.StartsWith('RAM:')) {
        if ($length -ne 131072 -or $null -ne $ram) { throw 'Invalid or duplicate WRAM block.' }
        $ram = $bytes[$offset..($offset + $length - 1)]
    }
    $offset += $length
}
if ($null -eq $ram) { throw 'No WRAM block found.' }
function Read-Word([int]$Address) { [BitConverter]::ToUInt16([byte[]]$ram, $Address) }
# These are cartridge WRAM symbols, independent of Snes9x's CPU/PPU struct layout.
$addresses = @{ Room = 0x079b; Width = 0x07a5; Headers = 0x1c37;
    Blocks = 0x1c87; Instructions = 0x1d27; Timers = 0x1d77; Level = 0x10002 }
$width = Read-Word $addresses.Width
if ((Read-Word $addresses.Room) -ne 0xa59f -or $width -ne 32) { throw 'Not the retail Kraid room.' }
'SHA256: ' + (Get-FileHash -LiteralPath $StatePath -Algorithm SHA256).Hash
for ($slot = 0; $slot -lt 40; $slot++) {
    if ((Read-Word ($addresses.Headers + 2*$slot)) -eq 0xb7bf) {
        'Active crumble PLM slot={0}, blockByteIndex={1:X4}, instruction={2:X4}, loopTimer={3}' -f $slot,
            (Read-Word ($addresses.Blocks + 2*$slot)),
            (Read-Word ($addresses.Instructions + 2*$slot)),
            (Read-Word ($addresses.Timers + 2*$slot))
    }
}
for ($x = 5; $x -le 26; $x++) {
    'Floor ({0},27)={1:X4}' -f $x, (Read-Word ($addresses.Level + 2*(27*$width + $x)))
}
