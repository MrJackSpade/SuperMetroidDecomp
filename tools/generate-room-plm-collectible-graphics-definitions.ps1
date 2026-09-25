param(
    [string] $RomPath = 'Super Metroid.smc'
)

$ErrorActionPreference = 'Stop'
try {
    $repoRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
    $romFile = [IO.Path]::GetFullPath((Join-Path $repoRoot $RomPath))
    $outputFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.Generated.cs'
    $expectedHash = '12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72'
    $actualHash = (Get-FileHash -LiteralPath $romFile -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actualHash -cne $expectedHash) {
        throw "Collectible-graphics generation requires unheadered Japan/USA NTSC v1.0 ($expectedHash), got $actualHash."
    }

    $rom = [IO.File]::ReadAllBytes($romFile)
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('namespace SuperMetroid.Core.Rooms;')
    $lines.Add('')
    $lines.Add('/// <summary>Pinned bank-$89 item tiles and bank-$84 palette offsets; regenerate with tools/generate-room-plm-collectible-graphics-definitions.ps1.</summary>')
    $lines.Add('internal static partial class RoomPlmDynamicCollectibleGraphicsDefinitions')
    $lines.Add('{')
    $lines.Add('    private static readonly (byte Kind, ushort GraphicsPointer, string PaletteHex, string TilesHex)[] Sources =')
    $lines.Add('    [')
    $uniqueGraphics = [Collections.Generic.HashSet[int]]::new()
    for ($kind = 4; $kind -lt 21; $kind++) {
        $graphicPointer = -1
        $paletteHex = $null
        foreach ($baseHeader in @(0xEED7, 0xEF2B, 0xEF7F)) {
            $header = $baseHeader + $kind * 4
            $headerOffset = 0x20000 + ($header - 0x8000)
            $instruction = [int]$rom[$headerOffset + 2] -bor ([int]$rom[$headerOffset + 3] -shl 8)
            if ($instruction -lt 0x8000) {
                throw ('Collectible header $84:{0:X4} has an invalid initial list.' -f $header)
            }
            $instructionOffset = 0x20000 + ($instruction - 0x8000)
            $opcode = [int]$rom[$instructionOffset] -bor ([int]$rom[$instructionOffset + 1] -shl 8)
            if ($opcode -ne 0x8764) {
                throw ('Collectible header $84:{0:X4} lacks the native $8764 graphics upload.' -f $header)
            }
            $candidatePointer = [int]$rom[$instructionOffset + 2] -bor ([int]$rom[$instructionOffset + 3] -shl 8)
            $candidatePalette = [Convert]::ToHexString($rom[($instructionOffset + 4)..($instructionOffset + 11)])
            if ($graphicPointer -ge 0 -and ($candidatePointer -ne $graphicPointer -or $candidatePalette -cne $paletteHex)) {
                throw ('Collectible kind {0} has different graphics or palette offsets across presentations.' -f $kind)
            }
            $graphicPointer = $candidatePointer
            $paletteHex = $candidatePalette
        }
        if ($graphicPointer -lt 0x8000 -or $graphicPointer -gt 0xFF00 -or !$uniqueGraphics.Add($graphicPointer)) {
            throw ('Collectible kind {0} has an invalid or duplicate bank-$89 graphics pointer.' -f $kind)
        }
        $sourceOffset = 0x48000 + ($graphicPointer - 0x8000)
        $tilesHex = [Convert]::ToHexString($rom[$sourceOffset..($sourceOffset + 0xFF)])
        $lines.Add(('        ({0}, 0x{1:X4}, "{2}", "{3}"),' -f $kind, $graphicPointer, $paletteHex, $tilesHex))
    }
    $lines.Add('    ];')
    $lines.Add('}')
    [IO.File]::WriteAllLines($outputFile, $lines, [Text.UTF8Encoding]::new($false))
    Write-Output "Generated $($uniqueGraphics.Count) shared permanent-item graphics uploads."
}
catch {
    [Console]::Error.WriteLine($_.Exception.ToString())
    exit 1
}
