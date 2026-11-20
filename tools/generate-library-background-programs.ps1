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
    if ($pointers.Count -ne 67) { throw "Expected 67 library-background programs, found $($pointers.Count)." }

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
    # Core defines doors per area (CrateriaDoorDefinitions, ...) and in DoorDefinitions, the unlisted doors among them.
    foreach ($match in [regex]::Matches($doorFile, '\[0x([0-9a-f]{4})\] = (\w*DoorDefinitions)\.(\w+),')) {
        $doorNames[[Convert]::ToInt32($match.Groups[1].Value, 16)] = $match.Groups[2].Value + '.' + $match.Groups[3].Value
    }
    if ($doorNames.Count -ne 594) { throw "Expected 594 door names, found $($doorNames.Count)." }
    function Format-Door([int]$door) {
        if ($door -eq 0) { throw 'A door transfer names no door.' }
        if (-not $doorNames.ContainsKey($door)) { throw ('Library transfer names unknown door ${0:X4}.' -f $door) }
        $qualified = $doorNames[$door]
        if ($qualified.StartsWith('DoorDefinitions.UnlistedDoorTo')) {
            throw ('Library transfer names door ${0:X4}, which no room data reaches ({1}).' -f $door, $qualified)
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
    if ($tilemapNames.Count -ne 57) { throw "Expected 57 background tilemaps, found $($tilemapNames.Count)." }
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
    # Each background's summary names the rooms that show it (#2546): the room-state choices name each state's room, and the
    # room states name the background each loads.
    $roomStates = [IO.Path]::Combine($workspace, 'csharp', 'src', 'SuperMetroid.Core', 'World', 'RoomStates')
    $stateRooms = @{}
    $roomOrder = [Collections.Generic.List[string]]::new()
    $room = $null
    foreach ($line in [IO.File]::ReadAllLines([IO.Path]::Combine($roomStates, 'RoomStateSelectionDefinitions.cs'))) {
        $roomMatch = [regex]::Match($line, '\[RoomDefinitions\.(\w+)\] = ')
        if ($roomMatch.Success) {
            $room = $roomMatch.Groups[1].Value
            $roomOrder.Add($room)
        }
        foreach ($stateMatch in [regex]::Matches($line, '\w+RoomStateDefinitions\.(\w+)')) {
            if ($null -eq $room) { throw "Room state $($stateMatch.Groups[1].Value) is chosen before any room." }
            if (-not $stateRooms.ContainsKey($stateMatch.Groups[1].Value)) { $stateRooms[$stateMatch.Groups[1].Value] = $room }
        }
    }
    $backgroundRooms = @{}
    foreach ($stateFile in [IO.Directory]::GetFiles($roomStates, '*RoomStateDefinitions.cs')) {
        # A state is named by its declared member, which the selections name; its installed key may keep an older name.
        $declared = $null
        foreach ($line in [IO.File]::ReadAllLines($stateFile)) {
            $declarationMatch = [regex]::Match($line, 'RoomStateSettings (\w+) \{ get; \} =')
            if ($declarationMatch.Success) { $declared = $declarationMatch.Groups[1].Value }
            $stateMatch = [regex]::Match($line, 'new\("(\w+)",')
            $backgroundMatch = [regex]::Match($line, 'LibraryBackgroundDefinitions\.(\w+)')
            if (-not $stateMatch.Success -or -not $backgroundMatch.Success) { continue }
            if ($null -eq $declared) { throw "Room state $($stateMatch.Groups[1].Value) in $stateFile follows no declaration." }
            $state = $declared
            $declared = $null
            if (-not $stateRooms.ContainsKey($state)) { throw "Room state $state loads a library background but no room chooses it." }
            $background = $backgroundMatch.Groups[1].Value
            if (-not $backgroundRooms.ContainsKey($background)) { $backgroundRooms[$background] = [Collections.Generic.HashSet[string]]::new() }
            [void]$backgroundRooms[$background].Add($stateRooms[$state])
        }
    }
    function Format-Rooms([string]$background) {
        if (-not $backgroundRooms.ContainsKey($background)) { throw "No room state loads library background $background." }
        $shown = @($roomOrder | Where-Object { $backgroundRooms[$background].Contains($_) } |
            ForEach-Object { '<see cref="RoomDefinitions.{0}"/>' -f $_ })
        if ($shown.Count -eq 1) { return $shown[0] }
        return ($shown[0..($shown.Count - 2)] -join ', ') + ' and ' + $shown[-1]
    }

    # The backgrounds the simulation cannot tell apart are one Core identity each (#2546): every list that clears the layer
    # leaves nothing, and every list that copies the enemies' staging leaves the boss's body, whichever room loads it. The
    # import keeps each list as its own program, naming the shared background.
    $sharedNames = @{ Nothing = 'Cleared'; BossBody = 'BossBody' }
    $declared = [Collections.Generic.HashSet[string]]::new()
    $instructionCount = 0
    foreach ($pointer in $pointers) {
        $cursor = $pointer
        $programName = $programNames[$pointer]
        $pictureDoors = [Collections.Generic.List[string]]::new()
        $instructionLines = [Collections.Generic.List[string]]::new()
        $terminated = $false
        # What the player sees on the background of a room that loads the list (#1649): the tilemap it stages, the sky's
        # rows (which the sky draws from its own image), the boss's body (the enemies' staging holds the boss's own layer,
        # which its body sheet pictures), or nothing (the layer is cleared to its empty tile, or not shown at all).
        $views = [Collections.Generic.HashSet[string]]::new()
        $pages = @('LibraryBackgroundDestination.BackgroundFirstPage', 'LibraryBackgroundDestination.BackgroundSecondPage',
            'LibraryBackgroundDestination.KraidBackgroundPages')
        for ($guard = 0; $guard -lt 128; $guard++) {
            $command = Read-Word $cursor
            $cursor += 2
            if ($command -eq 0) { $terminated = $true; break }
            $source = ''
            $destination = ''
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
            # Each command kind is its own instruction carrying only its operands (#2558).
            $instruction = switch ($name) {
                'Transfer' { 'new LibraryBackgroundInstruction.Transfer({0}, {1}, {2})' -f $source, $destination, $byteCount }
                'TransferForKraid' { 'new LibraryBackgroundInstruction.TransferForKraid({0}, {1}, {2})' -f $source, $destination, $byteCount }
                'TransferForDoor' { 'new LibraryBackgroundInstruction.TransferForDoor({0}, {1}, {2}, {3})' -f (Format-Door $door), $source, $destination, $byteCount }
                'Decompress' { 'new LibraryBackgroundInstruction.Decompress(new {0})' -f ($source -replace '^LibraryBackgroundSource\.Compressed\(', 'LibraryBackgroundSource.CompressedTilemap(') }
                default { 'new LibraryBackgroundInstruction.{0}()' -f $name }
            }
            $instructionLines.Add(('        {0},' -f $instruction))
            if ($command -eq 10 -or $command -eq 12) { [void]$views.Add('Nothing') }
            elseif ($pages -contains $destination) {
                if ($source -eq 'LibraryBackgroundSource.TilemapStaging') { [void]$views.Add('Picture') }
                elseif ($source -eq 'LibraryBackgroundSource.EnemyTilemapStaging') { [void]$views.Add('BossBody') }
                elseif ($source.StartsWith('LibraryBackgroundSource.Sky(')) { [void]$views.Add('SkyRows') }
                else { throw ('Library list ${0:X4} copies {1} to a background page.' -f $pointer, $source) }
            }
            if ($command -eq 14 -and -not $pictureDoors.Contains((Format-Door $door))) {
                $pictureDoors.Add((Format-Door $door))
            }
            $instructionCount++
        }
        if (-not $terminated) { throw ('Unterminated library list ${0:X4}.' -f $pointer) }
        if ($views.Count -ne 1) {
            throw ('Library list ${0:X4} leaves the background showing [{1}], not one view.' -f $pointer, ($views -join ', '))
        }
        $view = @($views)[0]
        $backgroundName = if ($sharedNames.ContainsKey($view)) { $sharedNames[$view] } else { $programName }
        if ($sharedNames.ContainsKey($view) -and $pictureDoors.Count -ne 0) {
            throw ('Library list ${0:X4} shows {1} but gives entry doors pictures of their own.' -f $pointer, $view)
        }
        $lines.Add(('    /// <summary>The {0} library-background program.</summary>' -f $programName))
        # Each program is reached through All; checks name one through its generated private access (#2570).
$lines.Add(('    private static LibraryBackgroundProgram {0} {{ get; }} = new(LibraryBackgroundDefinitions.{1}, [' -f
            $programName, $backgroundName))
        $lines.AddRange($instructionLines)
        $lines.Add('    ]);')
        $lines.Add('')
        if (-not $declared.Add($backgroundName)) { continue }
        # Each background's summary says what the player sees of it, behind which rooms' blocks, and how many entry doors
        # show a picture of their own (#2482, #2546), which neither its name nor its view does.
        $rooms = Format-Rooms $backgroundName
        $doorPhrase = switch ($pictureDoors.Count) {
            0 { '' }
            1 { '; one of its entry doors shows a picture of its own' }
            default { '; {0} of its entry doors each show a picture of their own' -f $pictureDoors.Count }
        }
        $summary = switch ($view) {
            Picture { 'The picture shown behind the blocks of {0}' -f $rooms }
            SkyRows { "The scrolling sky's rows, shown behind the blocks of {0}" -f $rooms }
            BossBody { "The body of the room's boss, which the boss draws behind the blocks of {0}" -f $rooms }
            Nothing { 'No picture: nothing is shown behind the blocks of {0}' -f $rooms }
            default { throw "Library list $programName leaves the undefined view $view." }
        }
        $coreLines.Add(('    /// <summary>{0}{1}.</summary>' -f $summary, $doorPhrase))
        $coreLines.Add(('    public static LibraryBackground {0} {{ get; }} = new("{0}", LibraryBackgroundView.{1}, [{2}]);' -f
            $backgroundName, $view, ($pictureDoors -join ', ')))
        $coreLines.Add('')
    }
    $lines.Add('    /// <summary>')
    $lines.Add('    /// Every library-background program, in command-list order: one for each background the player sees as a picture or')
    $lines.Add('    /// as the sky''s rows, and several for the cleared background and for the boss''s body.')
    $lines.Add('    /// </summary>')
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
