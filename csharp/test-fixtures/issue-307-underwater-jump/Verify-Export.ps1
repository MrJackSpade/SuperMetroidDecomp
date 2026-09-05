param([string]$RepositoryRoot = (Resolve-Path "$PSScriptRoot/../../..").Path)
$ErrorActionPreference = 'Stop'
$runner = Join-Path $RepositoryRoot 'csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0/SuperMetroid.DebugRunner.dll'
$recording = Join-Path $RepositoryRoot 'input-recordings/SuperMetroid-input-20260905-174826-322.smrec'
$rom = Join-Path $RepositoryRoot 'Super Metroid.smc'
$output = Join-Path $RepositoryRoot ('csharp/test-temp/export-regression-' + [Guid]::NewGuid().ToString('N') + '.srm')
& dotnet $runner --export-replay-sram $recording $rom $output 2
if ($LASTEXITCODE -ne 0) { throw 'SRAM export failed.' }
$actual = [IO.File]::ReadAllBytes($output)
$expected = [IO.File]::ReadAllBytes((Join-Path $PSScriptRoot 'Maridia Jump Test.srm'))
$original = [IO.File]::ReadAllBytes((Join-Path $PSScriptRoot 'Super Metroid.srm'))
if ($actual.Length -ne $expected.Length) { throw 'Export length changed.' }
# FILE C entry-point word and redundant checksum/complement directory words.
# These independently specified fixture offsets deliberately do not use production catalogs.
$allowed = @(0x0E1C, 0x0E1D, 0x0004, 0x0005, 0x000C, 0x000D, 0x1FF4, 0x1FF5, 0x1FFC, 0x1FFD)
for ($index = 0; $index -lt $actual.Length; $index++) {
    if ($actual[$index] -ne $expected[$index]) { throw "Cartridge-verified fixture mismatch at $index." }
    if ($actual[$index] -ne $original[$index] -and $index -notin $allowed) {
        throw "Unrelated save data changed at $index."
    }
}
if ([BitConverter]::ToUInt16($original, 0x0E1C) -ne 0) { throw 'Missing failing intro-entry fixture.' }
if ([BitConverter]::ToUInt16($actual, 0x0E1C) -ne 5) { throw 'Export still selects intro instead of main game.' }
Write-Output 'PASS: exact cartridge-verified export; only entry point/checksums changed.'
