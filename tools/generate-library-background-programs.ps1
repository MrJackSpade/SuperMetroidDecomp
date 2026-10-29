param(
    [Parameter(Mandatory = $true)]
    [string]$RomPath
)

$ErrorActionPreference = 'Stop'
try {
    $workspace = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, '..'))
    # The import pipeline owns the command lists; Core names only the backgrounds and the doors that show their own pictures (#1466).
    $target = [IO.Path]::GetFullPath([IO.Path]::Combine($workspace, 'csharp', 'src',
        'SuperMetroid.AssetExtraction', 'Rooms', 'LibraryBackgroundProgramDefinitions.Generated.cs'))
    $coreTarget = [IO.Path]::GetFullPath([IO.Path]::Combine($workspace, 'csharp', 'src',
        'SuperMetroid.Core', 'Rooms', 'Backgrounds', 'LibraryBackgroundDefinitions.Generated.cs'))
    foreach ($path in @($target, $coreTarget)) {
        if (-not $path.StartsWith($workspace + [IO.Path]::DirectorySeparatorChar,
            [StringComparison]::OrdinalIgnoreCase)) {
            throw "Generated catalog path escapes the repository: $path"
        }
    }

    $rom = [IO.File]::ReadAllBytes([IO.Path]::GetFullPath($RomPath))
    if ($rom.Length -ne 3145728) {
        throw "Expected the unheadered 3 MiB pinned ROM, got $($rom.Length) bytes."
    }
    $identityFile = [IO.File]::ReadAllText([IO.Path]::Combine($workspace, 'csharp', 'src',
        'SuperMetroid.AssetExtraction', 'Cartridge', 'SupportedCartridge.cs'))
    $identity = [regex]::Match($identityFile, 'public const string Sha256 = "([0-9a-f]{64})"')
    if (-not $identity.Success -or
        -not [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($rom)).Equals(
            $identity.Groups[1].Value, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The ROM does not match the pinned Japan/USA NTSC v1.0 cartridge.'
    }

    $programFile = [IO.File]::ReadAllText([IO.Path]::Combine($workspace, 'csharp', 'test-support',
        'VerificationAccess.Core', 'AssetExtraction', 'Rooms', 'LibraryBackgroundSourceDefinitions.cs'))
    $programNames = @{}
    foreach ($match in [regex]::Matches($programFile, '\[0x([0-9a-f]{4})\] = LibraryBackgroundProgramDefinitions\.(\w+),')) {
        $programNames[[Convert]::ToInt32($match.Groups[1].Value, 16)] = $match.Groups[2].Value
    }
    $pointers = @($programNames.Keys | Sort-Object)
    if ($pointers.Count -ne 68) { throw "Expected 68 library-background programs, found $($pointers.Count)." }

    function Read-Word([int]$pointer) {
        if ($pointer -lt 0x8000 -or $pointer -gt 0xFFFE) {
            throw ('Bank-$8F word read out of range at ${0:X4}.' -f $pointer)
        }
        $offset = 0x78000 + ($pointer - 0x8000)
        return [int]$rom[$offset] -bor ([int]$rom[$offset + 1] -shl 8)
    }
    function Read-Long([int]$pointer) {
        if ($pointer -lt 0x8000 -or $pointer -gt 0xFFFD) {
            throw ('Bank-$8F long read out of range at ${0:X4}.' -f $pointer)
        }
        $offset = 0x78000 + ($pointer - 0x8000)
        return [int]$rom[$offset] -bor ([int]$rom[$offset + 1] -shl 8) -bor
            ([int]$rom[$offset + 2] -shl 16)
    }

    $doorFile = [IO.File]::ReadAllText([IO.Path]::Combine($workspace, 'csharp', 'src',
        'SuperMetroid.Tooling', 'AssetExtraction', 'Rooms', 'DoorHeaderSourceDefinitions.cs'))
    $doorNames = @{}
    # Core defines doors per area (CrateriaDoorDefinitions, ...) and in DoorDefinitions; DoorToolingDefinitions is tooling's.
    foreach ($match in [regex]::Matches($doorFile, '\[0x([0-9a-f]{4})\] = (\w*DoorDefinitions|DoorToolingDefinitions)\.(\w+),')) {
        $doorNames[[Convert]::ToInt32($match.Groups[1].Value, 16)] = $match.Groups[2].Value + '.' + $match.Groups[3].Value
    }
    if ($doorNames.Count -ne 597) { throw "Expected 597 door names, found $($doorNames.Count)." }
    function Format-Door([int]$door) {
        if ($door -eq 0) { return 'null' }
        if (-not $doorNames.ContainsKey($door)) { throw ('Library transfer names unknown door ${0:X4}.' -f $door) }
        $qualified = $doorNames[$door]
        if ($qualified.StartsWith('DoorToolingDefinitions.')) {
            throw ('Library transfer names door ${0:X4}, which only tooling defines ({1}).' -f $door, $qualified)
        }
        return $qualified
    }

    # Extraction names each compressed background tilemap; the programs name tilemaps, sources and destinations, never addresses (#1414).
    $tilemapFile = [IO.File]::ReadAllText([IO.Path]::Combine($workspace, 'csharp', 'src',
        'SuperMetroid.AssetExtraction', 'CartridgeTables', 'RoomBackgroundSourceDefinitions.cs'))
    $tilemapNames = @{}
    foreach ($match in [regex]::Matches($tilemapFile, '\[RoomBackgroundTilemap\.(\w+)\] = 0x([0-9a-f]{6}),')) {
        $tilemapNames[[Convert]::ToInt32($match.Groups[2].Value, 16)] = $match.Groups[1].Value
    }
    if ($tilemapNames.Count -ne 58) { throw "Expected 58 background tilemaps, found $($tilemapNames.Count)." }
    function Format-Tilemap([int]$source) {
        if (-not $tilemapNames.ContainsKey($source)) { throw ('Library decompression names unknown tilemap ${0:X6}.' -f $source) }
        return 'LibraryBackgroundSource.Compressed(RoomBackgroundTilemap.' + $tilemapNames[$source] + ')'
    }
    function Format-TransferSource([int]$source) {
        switch ($source) {
            0x7E4000 { return 'LibraryBackgroundSource.TilemapStaging' }
            0x7E2000 { return 'LibraryBackgroundSource.EnemyTilemapStaging' }
            0x87AD64 { return 'LibraryBackgroundSource.TourianStatueGhostCharacters' }
            0x9AB200 { return 'LibraryBackgroundSource.HudCharacters' }
        }
        $skyOffset = $source - 0x8AB180
        if ($skyOffset -ge 0 -and ($skyOffset % 0x800) -eq 0 -and ($skyOffset / 0x800) -lt 7) {
            return ('LibraryBackgroundSource.Sky({0})' -f ($skyOffset / 0x800))
        }
        throw ('Library transfer reads unknown source ${0:X6}.' -f $source)
    }
    function Format-Destination([int]$word) {
        switch ($word) {
            0x4800 { return 'LibraryBackgroundDestination.BackgroundFirstPage' }
            0x4C00 { return 'LibraryBackgroundDestination.BackgroundSecondPage' }
            0x4000 { return 'LibraryBackgroundDestination.KraidBackgroundPages' }
            0x6D00 { return 'LibraryBackgroundDestination.StatueGhostSpriteCharacters' }
            0x2000 { return 'LibraryBackgroundDestination.KraidLayer3Characters' }
        }
        throw ('Library transfer writes unknown VRAM word ${0:X4}.' -f $word)
    }

    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('// <auto-generated/>')
    $lines.Add('#nullable enable')
    $lines.Add('using SuperMetroid.Core.Rooms.Backgrounds;')
    $lines.Add('using SuperMetroid.Core.Rooms.Doors;')
    $lines.Add('')
    $lines.Add('namespace SuperMetroid.AssetExtraction.Rooms;')
    $lines.Add('')
    $lines.Add('public static partial class LibraryBackgroundProgramDefinitions')
    $lines.Add('{')
    $coreLines = [Collections.Generic.List[string]]::new()
    $coreLines.Add('// <auto-generated/>')
    $coreLines.Add('#nullable enable')
    $coreLines.Add('using SuperMetroid.Core.Rooms.Doors;')
    $coreLines.Add('')
    $coreLines.Add('namespace SuperMetroid.Core.Rooms.Backgrounds;')
    $coreLines.Add('')
    $coreLines.Add('public static partial class LibraryBackgroundDefinitions')
    $coreLines.Add('{')
    $instructionCount = 0
    foreach ($pointer in $pointers) {
        $cursor = $pointer
        $programName = $programNames[$pointer]
        $pictureDoors = [Collections.Generic.List[string]]::new()
        $lines.Add(('    /// <summary>The {0} library-background program.</summary>' -f $programName))
        $lines.Add(('    public static LibraryBackgroundProgram {0} {{ get; }} = new(LibraryBackgroundDefinitions.{0}, [' -f $programName))
        $terminated = $false
        for ($guard = 0; $guard -lt 128; $guard++) {
            $command = Read-Word $cursor
            $cursor += 2
            if ($command -eq 0) { $terminated = $true; break }
            $source = 'LibraryBackgroundSource.None'
            $destination = 'LibraryBackgroundDestination.None'
            $byteCount = 0
            $door = 0
            switch ($command) {
                2 { $name = 'Transfer'; $source = Format-TransferSource (Read-Long $cursor);
                    $destination = Format-Destination (Read-Word ($cursor + 3)); $byteCount = Read-Word ($cursor + 5);
                    $cursor += 7 }
                4 { $name = 'Decompress'; $source = Format-Tilemap (Read-Long $cursor);
                    $stagingOffset = Read-Word ($cursor + 3)
                    if ($stagingOffset -ne 0x4000) { throw ('Library decompression stages at $7E:{0:X4}, not the tilemap staging.' -f $stagingOffset) }
                    $destination = 'LibraryBackgroundDestination.TilemapStaging'; $cursor += 5 }
                6 { $name = 'ClearFxTilemap' }
                8 { $name = 'TransferForKraid'; $source = Format-TransferSource (Read-Long $cursor);
                    $destination = Format-Destination (Read-Word ($cursor + 3)); $byteCount = Read-Word ($cursor + 5);
                    $cursor += 7 }
                10 { $name = 'ClearBg2' }
                12 { $name = 'ClearBg2ForKraid' }
                14 { $name = 'TransferForDoor'; $door = Read-Word $cursor;
                    $source = Format-TransferSource (Read-Long ($cursor + 2));
                    $destination = Format-Destination (Read-Word ($cursor + 5)); $byteCount = Read-Word ($cursor + 7);
                    $cursor += 9 }
                default { throw ('Unknown bank-$8F library command ${0:X4} at ${1:X4}.' -f
                        $command, ($cursor - 2)) }
            }
            $lines.Add(('        new(LibraryBackgroundCommand.{0}, {1}, {2}, {3}, {4}),' -f
                $name, $source, $destination, $byteCount, (Format-Door $door)))
            if ($command -eq 14 -and -not $pictureDoors.Contains((Format-Door $door))) {
                $pictureDoors.Add((Format-Door $door))
            }
            $instructionCount++
        }
        if (-not $terminated) { throw ('Unterminated library list ${0:X4}.' -f $pointer) }
        $lines.Add('    ]);')
        $lines.Add('')
        $coreLines.Add(('    /// <summary>The {0} library background.</summary>' -f $programName))
        $coreLines.Add(('    public static LibraryBackground {0} {{ get; }} = new(nameof({0}), [{1}]);' -f
            $programName, ($pictureDoors -join ', ')))
        $coreLines.Add('')
    }
    $lines.Add('    /// <summary>Every library-background program, one for each library background, in command-list order.</summary>')
    $lines.Add('    public static IReadOnlyList<LibraryBackgroundProgram> All { get; } =')
    $lines.Add('    [')
    foreach ($pointer in $pointers) {
        $lines.Add(('        {0},' -f $programNames[$pointer]))
    }
    $lines.Add('    ];')
    $lines.Add('}')
    $coreLines.Add('}')
    [IO.File]::WriteAllLines($target, $lines, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllLines($coreTarget, $coreLines, [Text.UTF8Encoding]::new($false))
    Write-Output "Generated $($pointers.Count) lists and $instructionCount typed instructions at $target, and their backgrounds at $coreTarget"
}
catch {
    [Console]::Error.WriteLine($_.Exception.ToString())
    exit 1
}
