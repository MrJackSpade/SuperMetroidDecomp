param(
    [string] $RomPath = 'Super Metroid.smc',
    [switch] $KindsOnly
)

$ErrorActionPreference = 'Stop'
try {
    if ($IsWindows) {
        Add-Type -TypeDefinition @"
using System.Runtime.InteropServices;
public static class RoomObjectPopulationGenerationProcessPolicy {
    [DllImport("kernel32.dll")] public static extern uint SetErrorMode(uint mode);
}
"@
        [void][RoomObjectPopulationGenerationProcessPolicy]::SetErrorMode(0x8003)
    }
    $repoRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
    $romFile = [IO.Path]::GetFullPath((Join-Path $repoRoot $RomPath))
    $populationFile = Join-Path $repoRoot 'csharp/test-support/VerificationAccess.Tooling/Rooms/RoomObjectPopulationSourceDefinitions.cs'
    $outputFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectPopulationDefinitions.cs'
    $kindOutputFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectKindDefinitions.Generated.cs'
    $scrollOutputFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectScrollChangeDefinitions.Generated.cs'
    $scrollIdentityFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomScrollChange.cs'
    $scrollNativeFile = Join-Path $repoRoot 'csharp/test-support/VerificationAccess.Core/Rooms/RoomObjects/RoomScrollProgramNative.cs'
    $doorCatalogFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Progression/LockedDoorDefinitions.cs'
    $itemCatalogFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Progression/ItemLocationDefinitions.cs'
    $placementNumbersFile = Join-Path $repoRoot 'csharp/src/SuperMetroid.AssetExtraction/Progression/RoomPlacementNumbers.Generated.cs'
    $expectedHash = '12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72'
    $actualHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([IO.File]::ReadAllBytes($romFile))).ToLowerInvariant()
    if ($actualHash -cne $expectedHash) {
        throw "PLM-population generation requires unheadered Japan/USA NTSC v1.0 ($expectedHash), got $actualHash."
    }

    $populationNames = @{}
    foreach ($match in [regex]::Matches([IO.File]::ReadAllText($populationFile), '\[0x([0-9a-f]{4})\] = RoomObjectPopulationDefinitions\.(\w+),')) {
        $populationNames[[Convert]::ToInt32($match.Groups[1].Value, 16)] = $match.Groups[2].Value
    }
    $ordered = @($populationNames.Keys | Sort-Object)
    if ($ordered.Count -ne 283) {
        throw "Expected 283 distinct retail PLM populations, found $($ordered.Count)."
    }

    $kindNames = @{}
    # RoomObjectKind's values are arbitrary (#1414); verification's native-word table names the kind of each header word.
    $kindCatalog = [IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/test-support/VerificationAccess.Core/Rooms/RoomObjects/RoomObjectNativeWords.cs'))
    foreach ($match in [regex]::Matches($kindCatalog, '(?m)^        \[RoomObjectKind\.(\w+)\] = 0x([0-9a-fA-F]+),\r?$')) {
        $word = [Convert]::ToInt32($match.Groups[2].Value, 16)
        if ($word -ne 0) { $kindNames[$word] = $match.Groups[1].Value }
    }
    # The import decodes each placement's room argument into what the PLM's setup reads (#1452): a collectible's item
    # location, a door, a grey door's condition and door, a save station's load station, a Metroid-cleared entry or a gate trigger. Doors and
    # item locations are named identities (#1553): each is named for the first population that places it and its kind,
    # with its block where two would share a name; the room-data number stays in the import pipeline's numbering. Scroll
    # triggers carry their program and Draygon cannons their control word instead; filler and fixed unread words become
    # the absence of an argument.
    $fixedArgumentWords = @{ 'NoobTube' = 0x0080; 'BombTorizoHand' = 0x0000 }
    function Format-RoomArgument([string] $name, [int] $word) {
        if ($name -eq 'ScrollTrigger' -or $word -eq 0x8000) { return 'null' }
        if ($fixedArgumentWords.ContainsKey($name)) {
            if ($word -ne $fixedArgumentWords[$name]) { throw ('{0} argument {1:X4} differs from its fixed word.' -f $name, $word) }
            return 'null'
        }
        if ($name -match '^(Chozo|Exposed|ShotBlock)') { return 'new CollectedItemArgument(@@ITEM:{0}@@)' -f $word }
        if ($name -match '^(GreyDoor|BombTorizoGreyDoor)') {
            $persistence = if (($word -band 0x8000) -eq 0) { 'Persistent' } else { 'Transient' }
            return 'new GreyDoorArgument({0}, new DoorBitArgument(@@DOOR:{1}@@, DoorOpeningPersistence.{2}))' -f (($word -shr 10) -band 0x1f), ($word -band 0x3ff), $persistence
        }
        if ($name -match '^((Red|Green|Yellow)Door|EyeDoor)') {
            if (($word -band 0x8000) -ne 0) { throw ('Door {0} argument {1:X4} never persists.' -f $name, $word) }
            return 'new DoorBitArgument(@@DOOR:{0}@@, DoorOpeningPersistence.Persistent)' -f $word
        }
        # A refill station's word numbers it in room data, but neither its setup nor its refill reads the number (#1623):
        # the station is placed without an argument.
        if ($name -in @('EnergyStation', 'MissileStation')) { return 'null' }
        # A save station's argument numbers its load station in its area's station list; the population's area resolves it.
        if ($name -eq 'SaveStation') { return '@@STATION:{0}@@' -f $word }
        if ($name -eq 'MetroidRoomClearance' -or $name -eq 'DownwardGateShotBlock') {
            if (($word -band 1) -ne 0) { throw ('{0} argument {1:X4} is odd.' -f $name, $word) }
            $type = if ($name -eq 'DownwardGateShotBlock') { 'GateTriggerArgument' } else { 'MetroidRoomArgument' }
            return 'new {0}({1})' -f $type, ($word -shr 1)
        }
        throw ('Room-object kind {0} has no decoded room argument for {1:X4}.' -f $name, $word)
    }
    # Each population's area, from the area room-state catalog that loads it, and each load station by its area and number,
    # from the station identities and the save codec's numbering (#1623).
    $populationAreas = @{}
    foreach ($stateFile in Get-ChildItem (Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/World/RoomStates') -Filter '*RoomStateDefinitions.cs') {
        $stateArea = $stateFile.Name -replace 'RoomStateDefinitions\.cs$', ''
        foreach ($match in [regex]::Matches([IO.File]::ReadAllText($stateFile.FullName), 'RoomObjectPopulationDefinitions\.(\w+)')) {
            if (-not $populationAreas.ContainsKey($match.Groups[1].Value)) { $populationAreas[$match.Groups[1].Value] = [Collections.Generic.HashSet[string]]::new() }
            [void] $populationAreas[$match.Groups[1].Value].Add($stateArea)
        }
    }
    $stationAreas = @{}
    foreach ($match in [regex]::Matches([IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Progression/LoadStationDefinitions.cs')), 'public static LoadStation (\w+) \{ get; \} = new\(nameof\(\1\), AreaId\.(\w+)\);')) {
        $stationAreas[$match.Groups[1].Value] = $match.Groups[2].Value
    }
    $stationsByAreaNumber = @{}
    foreach ($match in [regex]::Matches([IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.AssetExtraction/Progression/LoadStationNumberCodec.cs')), '\[LoadStationDefinitions\.(\w+)\] = (\d+),')) {
        $stationsByAreaNumber['{0}:{1}' -f $stationAreas[$match.Groups[1].Value], $match.Groups[2].Value] = $match.Groups[1].Value
    }
    # Each population's rooms, from the room states that load it and the rooms whose state selection offers those states,
    # and each room's size in screens (#1624): a scroll change is named for the room whose populations place it and the
    # screens of that room it closes and opens.
    $statePopulations = @{}
    foreach ($stateFile in Get-ChildItem (Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/World/RoomStates') -Filter '*RoomStateDefinitions.cs') {
        foreach ($match in [regex]::Matches([IO.File]::ReadAllText($stateFile.FullName), 'public static RoomStateSettings (\w+) \{ get; \} =\s*new\(nameof\(\1\)[^;]*?RoomObjectPopulationDefinitions\.(\w+)')) {
            $statePopulations[$match.Groups[1].Value] = $match.Groups[2].Value
        }
    }
    $populationRooms = @{}
    $selectionText = [IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/World/RoomStates/RoomStateSelectionDefinitions.cs'))
    $roomChoices = [regex]::Matches($selectionText, '\[RoomDefinitions\.(\w+)\] = new\(')
    for ($choice = 0; $choice -lt $roomChoices.Count; $choice++) {
        $choiceEnd = if ($choice + 1 -lt $roomChoices.Count) { $roomChoices[$choice + 1].Index } else { $selectionText.Length }
        $room = $roomChoices[$choice].Groups[1].Value
        foreach ($state in [regex]::Matches($selectionText.Substring($roomChoices[$choice].Index, $choiceEnd - $roomChoices[$choice].Index), '\w+RoomStateDefinitions\.(\w+)')) {
            if (-not $statePopulations.ContainsKey($state.Groups[1].Value)) { throw "Room $room offers state $($state.Groups[1].Value), which names no room-object population." }
            $statePopulation = $statePopulations[$state.Groups[1].Value]
            if (-not $populationRooms.ContainsKey($statePopulation)) { $populationRooms[$statePopulation] = [Collections.Generic.HashSet[string]]::new() }
            [void] $populationRooms[$statePopulation].Add($room)
        }
    }
    $roomScreens = @{}
    foreach ($match in [regex]::Matches([IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomDefinitions.cs')), 'new\(nameof\((\w+)\), \d+, AreaId\.\w+, new\(\d+, \d+\), new\((\d+), (\d+)\)')) {
        $roomScreens[$match.Groups[1].Value] = [pscustomobject]@{ Width = [int]$match.Groups[2].Value; Height = [int]$match.Groups[3].Value }
    }
    if ($roomScreens.Count -ne 261) { throw "Expected 261 retail room sizes, found $($roomScreens.Count)." }
    $rom = [IO.File]::ReadAllBytes($romFile)
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('// <auto-generated/>')
    $lines.Add('#nullable enable')
    $lines.Add('using SuperMetroid.Core.Progression;')
    $lines.Add('')
    $lines.Add('namespace SuperMetroid.Core.Rooms.RoomObjects;')
    $lines.Add('')
    $lines.Add('/// <summary>')
    $lines.Add('/// The retail room-object populations, each named for its room state. Each holds its placements')
    $lines.Add('/// in authored order: the object''s identity, block X and Y, and the room argument decoded into what the object reads; regenerate with')
    $lines.Add('/// tools/generate-room-object-population-definitions.ps1.')
    $lines.Add('/// </summary>')
    $lines.Add('internal static class RoomObjectPopulationDefinitions')
    $lines.Add('{')
    $recordTotal = 0
    # The first placement of each door and item location, by room-data number: its population, kind and block.
    $doorPlacements = @{}
    $itemPlacements = @{}
    $placedWords = [Collections.Generic.HashSet[int]]::new()
    $scrollProgramPointers = [Collections.Generic.HashSet[int]]::new()
    # The one room whose populations place each scroll program; the program's name is resolved once its writes are decoded.
    $scrollRooms = @{}
    # A Draygon wall cannon's room argument is the WRAM address of the control word it reads and writes; the catalog
    # carries the decoded word instead.
    $draygonCannonControls = @{ 0x8802 = 'UpperLeft'; 0x8804 = 'LowerLeft'; 0x8806 = 'UpperRight'; 0x8808 = 'LowerRight'; 0x880A = 'UnusedBottom' }
    foreach ($pointer in $ordered) {
        $offset = 0x78000 + ($pointer - 0x8000)
        $name = $populationNames[$pointer]
        $lines.Add(('    /// <summary>The {0} room-object population.</summary>' -f $name))
        $lines.Add(('    public static RoomObjectPopulation {0} {{ get; }} = RoomObjectPopulation.FromPlacements(nameof({0}),' -f $name))
        $lines.Add('    [')
        $count = 0
        while ($true) {
            if ($offset -ge $rom.Length - 1) {
                throw ('Population $8F:{0:X4} exceeds the cartridge.' -f $pointer)
            }
            $kindWord = [int]$rom[$offset] -bor ([int]$rom[$offset + 1] -shl 8)
            if ($kindWord -eq 0) {
                break
            }
            if (++$count -gt 256 || $offset -ge $rom.Length - 5) {
                throw ('Population $8F:{0:X4} has no bounded terminator.' -f $pointer)
            }
            [void] $placedWords.Add($kindWord)
            $scrollProgram = ''
            if ($kindWord -eq 0xB703) {
                $scrollProgramPointer = [int]$rom[$offset + 4] -bor ([int]$rom[$offset + 5] -shl 8)
                [void] $scrollProgramPointers.Add($scrollProgramPointer)
                [string[]] $rooms = @()
                if ($populationRooms.ContainsKey($name)) { $rooms = [string[]] @($populationRooms[$name]) }
                if ($rooms.Count -ne 1) { throw "Scroll-trigger population $name is loaded by no single room." }
                if ($scrollRooms.ContainsKey($scrollProgramPointer) -and $scrollRooms[$scrollProgramPointer] -cne $rooms[0]) {
                    throw ('Scroll program $8F:{0:X4} is placed in both {1} and {2}.' -f $scrollProgramPointer, $scrollRooms[$scrollProgramPointer], $rooms[0])
                }
                $scrollRooms[$scrollProgramPointer] = $rooms[0]
                $scrollProgram = ', ScrollChange: RoomScrollChange.@@SCROLL:{0}@@' -f $scrollProgramPointer
            }
            if (-not $kindNames.ContainsKey($kindWord)) { throw ('Retail header word {0:X4} names no room-object kind.' -f $kindWord) }
            $argument = [int]$rom[$offset + 4] -bor ([int]$rom[$offset + 5] -shl 8)
            if ($kindNames[$kindWord].StartsWith('DraygonCannon')) {
                if (-not $draygonCannonControls.ContainsKey($argument)) {
                    throw ('Draygon cannon argument {0:X4} names no control word.' -f $argument)
                }
                $argumentText = 'null, DraygonWallCannon.{0}' -f $draygonCannonControls[$argument]
            }
            else {
                $argumentText = Format-RoomArgument $kindNames[$kindWord] $argument
                if ($argumentText -match '^@@STATION:(\d+)@@$') {
                    [string[]] $areas = @()
                    if ($populationAreas.ContainsKey($name)) { $areas = [string[]] @($populationAreas[$name]) }
                    if ($areas.Count -ne 1) { throw "Save station population $name is loaded by no single area." }
                    $stationKey = '{0}:{1}' -f $areas[0], $Matches[1]
                    if (-not $stationsByAreaNumber.ContainsKey($stationKey)) { throw "Save station population $name names no load station $stationKey." }
                    $argumentText = 'new SaveStationArgument(LoadStationDefinitions.{0})' -f $stationsByAreaNumber[$stationKey]
                }
                foreach ($token in [regex]::Matches($argumentText, '@@(DOOR|ITEM):(\d+)@@')) {
                    $placements = if ($token.Groups[1].Value -eq 'DOOR') { $doorPlacements } else { $itemPlacements }
                    $number = [int]$token.Groups[2].Value
                    if (-not $placements.ContainsKey($number)) {
                        $placements[$number] = [pscustomobject]@{ Base = $name + $kindNames[$kindWord]; X = $rom[$offset + 2]; Y = $rom[$offset + 3] }
                    }
                }
            }
            $lines.Add(('        new(RoomObjectKind.{0}, new({1}, {2}), {3}{4}),' -f $kindNames[$kindWord], $rom[$offset + 2], $rom[$offset + 3], $argumentText, $scrollProgram))
            $offset += 6
        }
        $recordTotal += $count
        $lines.Add('    ]);')
        $lines.Add('')
    }
    if ($recordTotal -ne 941) {
        throw "Expected 941 retail PLM records, found $recordTotal."
    }
    $lines.RemoveAt($lines.Count - 1)
    $lines.Add('}')

    # Each scroll program's ordered writes, and its name: its room, then the room's screens it closes, opens and opens
    # top-aligned, each screen as its column and row in the room (#1624). Within one room no two programs write the same
    # cells, so each name is one program; programs that write the same cells in different rooms change different screens
    # and stay distinct.
    $orderedScrollPrograms = @($scrollProgramPointers | Sort-Object)
    if ($orderedScrollPrograms.Count -ne 173) {
        throw "Expected 173 distinct retail scroll programs, found $($orderedScrollPrograms.Count)."
    }
    # Each scroll state's place in a name: what the write does to the screen, and how the summary says it.
    $scrollEffects = @(
        [pscustomobject]@{ State = 0; Verb = 'Closes'; Suffix = ''; Phrase = 'closes {0}' },
        [pscustomobject]@{ State = 2; Verb = 'Opens'; Suffix = ''; Phrase = 'opens {0}' },
        [pscustomobject]@{ State = 1; Verb = 'Opens'; Suffix = 'TopAligned'; Phrase = 'opens {0} with the camera aligned to the screen''s top' }
    )
    $scrollChanges = @{}
    $scrollChangeNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
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
        $room = $scrollRooms[$pointer]
        if (-not $roomScreens.ContainsKey($room)) { throw "Scroll program room $room has no retail size." }
        $screens = $roomScreens[$room]
        $writtenCells = [Collections.Generic.HashSet[int]]::new()
        $writes = [Collections.Generic.List[object]]::new()
        # The import decodes each stored row-major cell number once, at the room's width, into the screen's column and row
        # (#1741); a cell past the room's own screens is a row past its rows, the kept cells after them, named below the room.
        for ($pair = 0; $pair -lt $bytes.Count - 1; $pair += 2) {
            $cell = [int]$bytes[$pair]
            if (-not $writtenCells.Add($cell)) { throw ('Scroll program $8F:{0:X4} writes cell {1} twice.' -f $pointer, $cell) }
            $writes.Add([pscustomobject]@{
                Cell = $cell
                Column = $cell % $screens.Width
                Row = [int][Math]::Floor($cell / $screens.Width)
                BelowTheRoom = $cell -ge $screens.Width * $screens.Height
                State = [int]$bytes[$pair + 1]
            })
        }
        $changeName = $room
        $phrases = [Collections.Generic.List[string]]::new()
        foreach ($effect in $scrollEffects) {
            $affectedWrites = @($writes | Where-Object State -EQ $effect.State | Sort-Object Cell)
            if ($affectedWrites.Count -eq 0) { continue }
            [string[]] $affected = @($affectedWrites | ForEach-Object { '{0}x{1}{2}' -f $_.Column, $_.Row, $(if ($_.BelowTheRoom) { 'BelowTheRoom' } else { '' }) })
            [string[]] $described = @($affectedWrites | ForEach-Object { '{0}x{1}{2}' -f $_.Column, $_.Row, $(if ($_.BelowTheRoom) { ' below the room' } else { '' }) })
            $noun = if ($affected.Count -eq 1) { 'Screen' } else { 'Screens' }
            $changeName += '{0}{1}{2}{3}' -f $effect.Verb, $noun, ($affected -join 'And'), $effect.Suffix
            $phrases.Add(($effect.Phrase -f ('{0} {1}' -f $noun.ToLowerInvariant(), ($described -join ' and '))))
        }
        if (-not $scrollChangeNames.Add($changeName)) { throw "Two scroll programs of the $room room make the same change $changeName." }
        $scrollChanges[$pointer] = [pscustomobject]@{ Name = $changeName; Room = $room; Writes = $writes; Summary = ($phrases -join '; ') }
    }
    if ($scrollByteTotal -ne 743 -or $scrollPairTotal -ne 285) {
        throw "Expected 743 scroll-program bytes/285 pairs, found $scrollByteTotal/$scrollPairTotal."
    }

    function Get-PlacementNames($placements) {
        $counts = @{}
        foreach ($entry in $placements.Values) { $counts[$entry.Base] = 1 + [int]$counts[$entry.Base] }
        $names = @{}
        $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($number in $placements.Keys) {
            $entry = $placements[$number]
            $names[$number] = if ($counts[$entry.Base] -eq 1) { $entry.Base } else { '{0}At{1}x{2}' -f $entry.Base, $entry.X, $entry.Y }
            if (-not $seen.Add($names[$number])) { throw "Two placements share the name $($names[$number])." }
        }
        return $names
    }
    $doorNames = Get-PlacementNames $doorPlacements
    $itemNames = Get-PlacementNames $itemPlacements
    $evaluator = [Text.RegularExpressions.MatchEvaluator] {
        param($match)
        if ($match.Groups[1].Value -eq 'DOOR') { 'LockedDoorDefinitions.' + $doorNames[[int]$match.Groups[2].Value] }
        elseif ($match.Groups[1].Value -eq 'SCROLL') { $scrollChanges[[int]$match.Groups[2].Value].Name }
        else { 'ItemLocationDefinitions.' + $itemNames[[int]$match.Groups[2].Value] }
    }
    for ($index = 0; $index -lt $lines.Count; $index++) {
        $lines[$index] = [regex]::Replace($lines[$index], '@@(DOOR|ITEM|SCROLL):(\d+)@@', $evaluator)
    }

    function New-PlacementCatalog([string] $typeName, [string] $catalogName, [string[]] $summary, [string] $memberSummary, [string] $numberedReason, $names) {
        $catalog = [Collections.Generic.List[string]]::new()
        $catalog.Add('// <auto-generated/>')
        $catalog.Add('#nullable enable')
        $catalog.Add('namespace SuperMetroid.Core.Progression;')
        $catalog.Add('')
        $catalog.Add('/// <summary>')
        foreach ($line in $summary) { $catalog.Add('/// ' + $line) }
        $catalog.Add('/// </summary>')
        $catalog.Add(('[NumberedIdentities("{0}")]' -f $numberedReason))
        $catalog.Add(('public static class {0}' -f $catalogName))
        $catalog.Add('{')
        foreach ($number in @($names.Keys | Sort-Object)) {
            $catalog.Add(('    /// <summary>{0}</summary>' -f ($memberSummary -f $names[$number])))
            $catalog.Add(('    public static {0} {1} {{ get; }} = new(nameof({1}));' -f $typeName, $names[$number]))
            $catalog.Add('')
        }
        $catalog.Add(('    /// <summary>Every {0}, in the order room data lists them.</summary>' -f $typeName))
        $catalog.Add(('    public static IReadOnlyList<{0}> All {{ get; }} =' -f $typeName))
        $catalog.Add('    [')
        foreach ($number in @($names.Keys | Sort-Object)) { $catalog.Add(('        {0},' -f $names[$number])) }
        $catalog.Add('    ];')
        $catalog.Add('}')
        return $catalog
    }
    $doorCatalog = New-PlacementCatalog 'LockedDoor' 'LockedDoorDefinitions' @(
        'Every door of the retail room populations that a cap keeps shut: each coloured, grey and eye door, named for the',
        'first room-state population that places it and its cap and, where two would share that name, its block; an eye',
        "door's three parts are one door. Regenerate with tools/generate-room-object-population-definitions.ps1."
    ) 'The {0} door.' 'Each door named with its block is one distinct door of its population, told apart by where it stands; no caller indexes them.' $doorNames
    $itemCatalog = New-PlacementCatalog 'ItemLocation' 'ItemLocationDefinitions' @(
        'Every item location of the retail room populations, named for the first room-state population that places it and',
        'its item and, where two would share that name, its block. Regenerate with',
        'tools/generate-room-object-population-definitions.ps1.'
    ) 'The {0} item location.' 'Each item location named with its block is one distinct location of its population, told apart by where it stands; no caller indexes them.' $itemNames
    $numberLines = [Collections.Generic.List[string]]::new()
    $numberLines.Add('// <auto-generated/>')
    $numberLines.Add('#nullable enable')
    $numberLines.Add('using SuperMetroid.Core.Progression;')
    $numberLines.Add('')
    $numberLines.Add('namespace SuperMetroid.AssetExtraction.Progression;')
    $numberLines.Add('')
    $numberLines.Add('/// <summary>The number room data gives each door and item location; regenerate with tools/generate-room-object-population-definitions.ps1.</summary>')
    $numberLines.Add('public static partial class RoomPlacementNumbers')
    $numberLines.Add('{')
    $numberLines.Add('    /// <summary>Each door and the number room data gives it, ascending.</summary>')
    $numberLines.Add('    private static readonly NumberedPlacement<LockedDoor>[] NumberedDoors =')
    $numberLines.Add('    [')
    foreach ($number in @($doorNames.Keys | Sort-Object)) { $numberLines.Add(('        new({0}, LockedDoorDefinitions.{1}),' -f $number, $doorNames[$number])) }
    $numberLines.Add('    ];')
    $numberLines.Add('')
    $numberLines.Add('    /// <summary>Each item location and the number room data gives it, ascending.</summary>')
    $numberLines.Add('    private static readonly NumberedPlacement<ItemLocation>[] NumberedItems =')
    $numberLines.Add('    [')
    foreach ($number in @($itemNames.Keys | Sort-Object)) { $numberLines.Add(('        new({0}, ItemLocationDefinitions.{1}),' -f $number, $itemNames[$number])) }
    $numberLines.Add('    ];')
    $numberLines.Add('}')
    if (-not $KindsOnly) {
        [IO.File]::WriteAllLines($outputFile, $lines, [Text.UTF8Encoding]::new($false))
        [IO.File]::WriteAllLines($doorCatalogFile, $doorCatalog, [Text.UTF8Encoding]::new($false))
        [IO.File]::WriteAllLines($itemCatalogFile, $itemCatalog, [Text.UTF8Encoding]::new($false))
        [IO.File]::WriteAllLines($placementNumbersFile, $numberLines, [Text.UTF8Encoding]::new($false))
    }

    $placedKindWords = @($placedWords | Sort-Object)
    if ($placedKindWords.Count -ne 70) {
        throw "Expected 70 distinct placed room-object kinds, found $($placedKindWords.Count)."
    }
    $kindLines = [Collections.Generic.List[string]]::new()
    $kindLines.Add('// <auto-generated/>')
    $kindLines.Add('namespace SuperMetroid.Core.Rooms.RoomObjects;')
    $kindLines.Add('')
    $kindLines.Add('/// <summary>The room-object kinds retail room populations place; regenerate with tools/generate-room-object-population-definitions.ps1 -KindsOnly.</summary>')
    $kindLines.Add('internal static partial class RoomObjectKindDefinitions')
    $kindLines.Add('{')
    $kindLines.Add('    /// <summary>Every room-object kind a retail room population places.</summary>')
    $kindLines.Add('    internal static readonly IReadOnlySet<RoomObjectKind> Placed = new HashSet<RoomObjectKind>')
    $kindLines.Add('    {')
    foreach ($kindWord in $placedKindWords) {
        if (-not $kindNames.ContainsKey($kindWord)) { throw ('Retail header word {0:X4} names no room-object kind.' -f $kindWord) }
        $kindLines.Add(('        RoomObjectKind.{0},' -f $kindNames[$kindWord]))
    }
    $kindLines.Add('    };')
    $kindLines.Add('}')
    [IO.File]::WriteAllLines($kindOutputFile, $kindLines, [Text.UTF8Encoding]::new($false))
    if ($KindsOnly) { Write-Output "Generated $($placedKindWords.Count) placed room-object kinds."; exit 0 }

    $scrollLines = [Collections.Generic.List[string]]::new()
    $scrollLines.Add('// <auto-generated/>')
    $scrollLines.Add('#nullable enable')
    $scrollLines.Add('using System.Collections.Frozen;')
    $scrollLines.Add('using SuperMetroid.Core.Rooms.Scrolling;')
    $scrollLines.Add('')
    $scrollLines.Add('namespace SuperMetroid.Core.Rooms.RoomObjects;')
    $scrollLines.Add('')
    $scrollLines.Add('/// <summary>Explicit ordered room-scroll state writes; regenerate with tools/generate-room-object-population-definitions.ps1.</summary>')
    $scrollLines.Add('internal static partial class RoomObjectScrollChangeDefinitions')
    $scrollLines.Add('{')
    $scrollLines.Add('    /// <summary>Each retail scroll change''s ordered writes, each screen by its column and row in the change''s room.</summary>')
    $scrollLines.Add('    internal static readonly FrozenDictionary<RoomScrollChange, ScreenScrollChange[]> Changes =')
    $scrollLines.Add('        new Dictionary<RoomScrollChange, ScreenScrollChange[]>')
    $scrollLines.Add('        {')
    $identityLines = [Collections.Generic.List[string]]::new()
    $identityLines.Add('// <auto-generated/>')
    $identityLines.Add('namespace SuperMetroid.Core.Rooms.RoomObjects;')
    $identityLines.Add('')
    $identityLines.Add('/// <summary>')
    $identityLines.Add('/// A retail scroll change, named for its room and what it does to the room''s screens: the screens it closes to the')
    $identityLines.Add('/// camera, opens, and opens with the camera aligned to the screen''s top, each as its column and row in the room. Regenerate')
    $identityLines.Add('/// with tools/generate-room-object-population-definitions.ps1.')
    $identityLines.Add('/// </summary>')
    $identityLines.Add('internal enum RoomScrollChange')
    $identityLines.Add('{')
    $nativeLines = [Collections.Generic.List[string]]::new()
    $nativeLines.Add('// <auto-generated/>')
    $nativeLines.Add('using System.Collections.Frozen;')
    $nativeLines.Add('')
    $nativeLines.Add('namespace SuperMetroid.Core.Rooms.RoomObjects;')
    $nativeLines.Add('')
    $nativeLines.Add('/// <summary>Where each retail scroll program is stored in bank $8F, for verification; regenerate with tools/generate-room-object-population-definitions.ps1.</summary>')
    $nativeLines.Add('internal static class RoomScrollProgramNative')
    $nativeLines.Add('{')
    $nativeLines.Add('    /// <summary>The bank-$8F address of each scroll program.</summary>')
    $nativeLines.Add('    internal static readonly FrozenDictionary<RoomScrollChange, ushort> Addresses =')
    $nativeLines.Add('        new Dictionary<RoomScrollChange, ushort>')
    $nativeLines.Add('        {')
    foreach ($pointer in $orderedScrollPrograms) {
        $change = $scrollChanges[$pointer]
        [string[]] $writes = @($change.Writes | ForEach-Object {
            $stateName = switch ($_.State) { 0 { 'Closed' } 1 { 'OpenAlignedToTop' } 2 { 'Open' } }
            'new(new({0}, {1}), RoomScrollState.{2})' -f $_.Column, $_.Row, $stateName
        })
        $scrollLines.Add(('            [RoomScrollChange.{0}] = [{1}],' -f $change.Name, ($writes -join ', ')))
        $identityLines.Add(('    /// <summary>In the {0} room, {1}.</summary>' -f $change.Room, $change.Summary))
        $identityLines.Add(('    {0},' -f $change.Name))
        $identityLines.Add('')
        $nativeLines.Add(('            [RoomScrollChange.{0}] = 0x{1:x4},' -f $change.Name, $pointer))
    }
    $scrollLines.Add('        }.ToFrozenDictionary();')
    $scrollLines.Add('}')
    [IO.File]::WriteAllLines($scrollOutputFile, $scrollLines, [Text.UTF8Encoding]::new($false))
    $identityLines.RemoveAt($identityLines.Count - 1)
    $identityLines.Add('}')
    [IO.File]::WriteAllLines($scrollIdentityFile, $identityLines, [Text.UTF8Encoding]::new($false))
    $nativeLines.Add('        }.ToFrozenDictionary();')
    $nativeLines.Add('}')
    [IO.File]::WriteAllLines($scrollNativeFile, $nativeLines, [Text.UTF8Encoding]::new($false))
    Write-Output "Generated $($ordered.Count) PLM populations, $recordTotal records, $($placedKindWords.Count) placed kinds and $($orderedScrollPrograms.Count) scroll programs."
}
catch {
    [Console]::Error.WriteLine($_.Exception.ToString())
    exit 1
}
