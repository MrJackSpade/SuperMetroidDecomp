param(
    [string] $RomPath = 'Super Metroid.smc',
    [switch] $HeadersOnly
)

$ErrorActionPreference = 'Stop'
try {
    if ($IsWindows) {
        Add-Type -TypeDefinition @"
using System.Runtime.InteropServices;
public static class PlmHeaderGenerationProcessPolicy {
    [DllImport("kernel32.dll")] public static extern uint SetErrorMode(uint mode);
}
"@
        [void][PlmHeaderGenerationProcessPolicy]::SetErrorMode(0x8003)
    }
    $repoRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
    $romFile = [IO.Path]::GetFullPath((Join-Path $repoRoot $RomPath))
    $stateFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs'
    $outputFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomPlmPopulationDefinitions.Generated.cs'
    $headerOutputFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomPlmHeaderDefinitions.Generated.cs'
    $scrollOutputFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomPlmScrollProgramDefinitions.Generated.cs'
    $expectedHash = '12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72'
    $actualHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([IO.File]::ReadAllBytes($romFile))).ToLowerInvariant()
    if ($actualHash -cne $expectedHash) {
        throw "PLM-population generation requires unheadered Japan/USA NTSC v1.0 ($expectedHash), got $actualHash."
    }

    $pointers = [Collections.Generic.HashSet[int]]::new()
    foreach ($line in [IO.File]::ReadLines($stateFile)) {
        if ($line -notmatch '^\s*new\(') { continue }
        $words = [regex]::Matches($line, '0x([0-9A-F]+)')
        if ($words.Count -ne 16) { continue }
        [void] $pointers.Add([Convert]::ToInt32($words[13].Groups[1].Value, 16))
    }
    $ordered = @($pointers | Sort-Object)
    if ($ordered.Count -ne 284) {
        throw "Expected 284 distinct retail PLM populations, found $($ordered.Count)."
    }

    $rom = [IO.File]::ReadAllBytes($romFile)
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('namespace SuperMetroid.Core.Rooms;')
    $lines.Add('')
    $lines.Add('/// <summary>Pinned cartridge placement records; regenerate with tools/generate-room-plm-population-definitions.ps1.</summary>')
    $lines.Add('internal static partial class RoomPlmPopulationDefinitions')
    $lines.Add('{')
    $lines.Add('    private static readonly (ushort Pointer, string Hex)[] Sources =')
    $lines.Add('    [')
    $recordTotal = 0
    $headers = [Collections.Generic.HashSet[int]]::new()
    $scrollProgramPointers = [Collections.Generic.HashSet[int]]::new()
    foreach ($pointer in $ordered) {
        $offset = 0x78000 + ($pointer - 0x8000)
        $bytes = [Collections.Generic.List[byte]]::new()
        $count = 0
        while ($true) {
            if ($offset -ge $rom.Length - 1) {
                throw ('Population $8F:{0:X4} exceeds the cartridge.' -f $pointer)
            }
            $header = [int]$rom[$offset] -bor ([int]$rom[$offset + 1] -shl 8)
            if ($header -eq 0) {
                $bytes.Add(0)
                $bytes.Add(0)
                break
            }
            if (++$count -gt 256 || $offset -ge $rom.Length - 5) {
                throw ('Population $8F:{0:X4} has no bounded terminator.' -f $pointer)
            }
            [void] $headers.Add($header)
            if ($header -eq 0xB703) {
                $scrollProgramPointer = [int]$rom[$offset + 4] -bor ([int]$rom[$offset + 5] -shl 8)
                [void] $scrollProgramPointers.Add($scrollProgramPointer)
            }
            for ($index = 0; $index -lt 6; $index++) {
                $bytes.Add($rom[$offset + $index])
            }
            $offset += 6
        }
        $recordTotal += $count
        $hex = [Convert]::ToHexString($bytes.ToArray())
        $lines.Add(('        (0x{0:X4}, "{1}"),' -f $pointer, $hex))
    }
    if ($recordTotal -ne 941) {
        throw "Expected 941 retail PLM records, found $recordTotal."
    }
    $lines.Add('    ];')
    $lines.Add('}')
    if (-not $HeadersOnly) { [IO.File]::WriteAllLines($outputFile, $lines, [Text.UTF8Encoding]::new($false)) }

    $orderedHeaders = @($headers | Sort-Object)
    if ($orderedHeaders.Count -ne 70) {
        throw "Expected 70 distinct retail PLM headers, found $($orderedHeaders.Count)."
    }
    $headerNames = @{}
    $headerCatalog = [IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomPlmHeaders.cs'))
    foreach ($match in [regex]::Matches($headerCatalog, 'const ushort (\w+) = 0x([0-9a-fA-F]+);')) {
        $headerNames[[Convert]::ToInt32($match.Groups[2].Value, 16)] = $match.Groups[1].Value
    }
    $headerLines = [Collections.Generic.List[string]]::new()
    $headerLines.Add('namespace SuperMetroid.Core.Rooms;')
    $headerLines.Add('')
    $headerLines.Add('/// <summary>Named retail header dispatch; regenerate with tools/generate-room-plm-population-definitions.ps1 -HeadersOnly.</summary>')
    $headerLines.Add('internal static partial class RoomPlmHeaderDefinitions')
    $headerLines.Add('{')
    $headerLines.Add('    private static bool TrySelect(ushort header, out RoomPlmHeaderDefinition value)')
    $headerLines.Add('    {')
    $headerLines.Add('        value = header switch')
    $headerLines.Add('        {')
    foreach ($header in $orderedHeaders) {
        if (-not $headerNames.ContainsKey($header)) { throw ('Retail header {0:X4} lacks a domain name.' -f $header) }
        $headerOffset = 0x20000 + ($header - 0x8000)
        $setup = [int]$rom[$headerOffset] -bor ([int]$rom[$headerOffset + 1] -shl 8)
        $initial = [int]$rom[$headerOffset + 2] -bor ([int]$rom[$headerOffset + 3] -shl 8)
        $headerLines.Add(('            RoomPlmHeaders.{0} => new(header, 0x{1:X4}, 0x{2:X4}),' -f $headerNames[$header], $setup, $initial))
    }
    $headerLines.Add('            _ => default,')
    $headerLines.Add('        };')
    $headerLines.Add('        return value.Header != 0;')
    $headerLines.Add('    }')
    $headerLines.Add('')
    $headerLines.Add('    private static IEnumerable<RoomPlmHeaderDefinition> Enumerate()')
    $headerLines.Add('    {')
    foreach ($header in $orderedHeaders) {
        $headerLines.Add(('        yield return Get(RoomPlmHeaders.{0});' -f $headerNames[$header]))
    }
    $headerLines.Add('    }')
    $headerLines.Add('}')
    [IO.File]::WriteAllLines($headerOutputFile, $headerLines, [Text.UTF8Encoding]::new($false))
    if ($HeadersOnly) { Write-Output "Generated $($orderedHeaders.Count) named retail header cases."; exit 0 }

    $orderedScrollPrograms = @($scrollProgramPointers | Sort-Object)
    if ($orderedScrollPrograms.Count -ne 173) {
        throw "Expected 173 distinct retail scroll programs, found $($orderedScrollPrograms.Count)."
    }
    $scrollLines = [Collections.Generic.List[string]]::new()
    $scrollLines.Add('#nullable enable')
    $scrollLines.Add('using SuperMetroid.Core.Game;')
    $scrollLines.Add('')
    $scrollLines.Add('namespace SuperMetroid.Core.Rooms;')
    $scrollLines.Add('')
    $scrollLines.Add('/// <summary>Explicit ordered room-scroll state writes; regenerate with tools/generate-room-plm-population-definitions.ps1.</summary>')
    $scrollLines.Add('internal static partial class RoomPlmScrollProgramDefinitions')
    $scrollLines.Add('{')
    $scrollLines.Add('    internal static bool TryApply(ushort pointer, Action<int, RoomScrollState>? write)')
    $scrollLines.Add('    {')
    $scrollLines.Add('        switch (pointer)')
    $scrollLines.Add('        {')
    $scrollByteTotal = 0
    $scrollPairTotal = 0
    foreach ($pointer in $orderedScrollPrograms) {
        if ($pointer -lt 0x8000) {
            throw ('Scroll program $8F:{0:X4} is outside LoROM bank data.' -f $pointer)
        }
        $offset = 0x78000 + ($pointer - 0x8000)
        $bytes = [Collections.Generic.List[byte]]::new()
        for ($pairIndex = 0; $pairIndex -lt 50; $pairIndex++) {
            if ($offset -ge $rom.Length) {
                throw ('Scroll program $8F:{0:X4} exceeds the cartridge.' -f $pointer)
            }
            $scrollIndex = $rom[$offset++]
            $bytes.Add($scrollIndex)
            if (($scrollIndex -band 0x80) -ne 0) { break }
            if ($scrollIndex -ge 50 -or $offset -ge $rom.Length) {
                throw ('Scroll program $8F:{0:X4} has an invalid storage index.' -f $pointer)
            }
            $scrollValue = $rom[$offset++]
            if ($scrollValue -gt 2) {
                throw ('Scroll program $8F:{0:X4} has an invalid scroll state.' -f $pointer)
            }
            $bytes.Add($scrollValue)
            $scrollPairTotal++
        }
        if ($bytes.Count -eq 0 -or ($bytes[$bytes.Count - 1] -band 0x80) -eq 0) {
            throw ('Scroll program $8F:{0:X4} has no bounded terminator.' -f $pointer)
        }
        $scrollByteTotal += $bytes.Count
        if ($bytes[$bytes.Count - 1] -ne 0x80) { throw 'Scroll terminator differs from canonical negative end marker.' }
        $scrollLines.Add(('            case 0x{0:X4}:' -f $pointer))
        for ($pair = 0; $pair -lt $bytes.Count - 1; $pair += 2) {
            $stateName = switch ($bytes[$pair + 1]) { 0 { 'RedBoundary' } 1 { 'Blue' } 2 { 'Green' } }
            $scrollLines.Add(('                write?.Invoke({0}, RoomScrollState.{1});' -f $bytes[$pair], $stateName))
        }
        $scrollLines.Add('                return true;')
    }
    if ($scrollByteTotal -ne 743 -or $scrollPairTotal -ne 285) {
        throw "Expected 743 scroll-program bytes/285 pairs, found $scrollByteTotal/$scrollPairTotal."
    }
    $scrollLines.Add('            default: return false;')
    $scrollLines.Add('        }')
    $scrollLines.Add('    }')
    $scrollLines.Add('}')
    [IO.File]::WriteAllLines($scrollOutputFile, $scrollLines, [Text.UTF8Encoding]::new($false))
    Write-Output "Generated $($ordered.Count) PLM populations, $recordTotal records, $($orderedHeaders.Count) headers and $($orderedScrollPrograms.Count) scroll programs."
}
catch {
    [Console]::Error.WriteLine($_.Exception.ToString())
    exit 1
}
