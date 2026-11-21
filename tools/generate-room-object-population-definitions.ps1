param(
    [string] $RomPath = 'Super Metroid.smc'
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
    # location, a door, a grey door's condition and door, a save station's load station, a Metroid room's cleared event or a gate trigger's colour and side. Doors and
    # item locations are named identities (#1553), each named for the room that places it and its kind (#2491, #2501),
    # with what tells two apart where they would share a name; the room-data number stays in the import pipeline's numbering. Scroll
    # triggers carry their program and Draygon cannons their control word instead; filler and fixed unread words become
    # the absence of an argument.
    $fixedArgumentWords = @{ 'NoobTube' = 0x0080; 'BombTorizoHand' = 0x0000 }
    # A grey door's word selects its unlock condition by its place in the cartridge's condition table, a gate trigger's
    # its colour and side by its place in the trigger table, and a Metroid room's clearance watch its cleared event by its
    # place in the watch table (#1746). The table's nine leading entries observe nothing: a placement of one is an escape
    # room's held place, an object that only holds its place (#2426). The places stay here; the definitions name the
    # condition, the trigger, the event and the held place.
    $greyDoorConditions = @('AreaBossDefeated', 'AreaMiniBossDefeated', 'AreaTorizoDefeated', 'EnemyDeathQuota', 'Never', 'TourianStatueFinished', 'CrittersEscaped')
    # The trigger table runs blue, red, green and yellow, each left of its gate and then right of it (#2484).
    $gateTriggerColors = @('Blue', 'Red', 'Green', 'Yellow')
    $gateTriggerSides = @('Left', 'Right')
    $metroidRoomEvents = @($null, $null, $null, $null, $null, $null, $null, $null, $null, 'FirstMetroidHallCleared', 'FirstMetroidShaftCleared', 'SecondMetroidHallCleared', 'SecondMetroidShaftCleared')
    function Format-RoomArgument([string] $name, [int] $word) {
        if ($name -eq 'ScrollTrigger' -or $word -eq 0x8000) { return 'null' }
        if ($fixedArgumentWords.ContainsKey($name)) {
            if ($word -ne $fixedArgumentWords[$name]) { throw ('{0} argument {1:X4} differs from its fixed word.' -f $name, $word) }
            return 'null'
        }
        if ($name -match '^(Chozo|Exposed|ShotBlock)') { return 'new CollectibleIdentity(@@ITEM:{0}@@)' -f $word }
        if ($name -match '^(GreyDoor|BombTorizoGreyDoor)') {
            $persistence = if (($word -band 0x8000) -eq 0) { 'Persistent' } else { 'Transient' }
            $condition = ($word -shr 10) -band 0x1f
            if ($condition -ge $greyDoorConditions.Count) { throw ('Grey door {0} argument {1:X4} selects no condition.' -f $name, $word) }
            return 'new GreyDoorOpening(GreyDoorCondition.{0}, new DoorLock(@@DOOR:{1}@@, DoorOpeningPersistence.{2}))' -f $greyDoorConditions[$condition], ($word -band 0x3ff), $persistence
        }
        if ($name -match '^((Red|Green|Yellow)Door|EyeDoor)') {
            if (($word -band 0x8000) -ne 0) { throw ('Door {0} argument {1:X4} never persists.' -f $name, $word) }
            return 'new DoorLock(@@DOOR:{0}@@, DoorOpeningPersistence.Persistent)' -f $word
        }
        # A refill station's word numbers it in room data, but neither its setup nor its refill reads the number (#1623):
        # the station is placed without an argument.
        if ($name -in @('EnergyStation', 'MissileStation')) { return 'null' }
        # A save station's argument numbers its load station in its area's station list; the population's area resolves it.
        if ($name -eq 'SaveStation') { return '@@STATION:{0}@@' -f $word }
        if ($name -eq 'MetroidRoomClearance' -or $name -eq 'DownwardGateShotBlock') {
            if (($word -band 1) -ne 0) { throw ('{0} argument {1:X4} is odd.' -f $name, $word) }
            $entry = $word -shr 1
            if ($name -eq 'DownwardGateShotBlock') {
                if ($entry -ge $gateTriggerColors.Count * $gateTriggerSides.Count) { throw ('{0} argument {1:X4} selects no trigger.' -f $name, $word) }
                return 'new GateShotTrigger(new DownwardGateTrigger(DownwardGateTriggerColor.{0}, HorizontalDirection.{1}))' -f $gateTriggerColors[$entry -shr 1], $gateTriggerSides[$entry -band 1]
            }
            if ($entry -ge $metroidRoomEvents.Count) { throw ('{0} argument {1:X4} selects no entry.' -f $name, $word) }
            if ($null -eq $metroidRoomEvents[$entry]) { return '@@HELDPLACE@@' }
            return 'new WatchedMetroidRoom(StoryEvent.{0})' -f $metroidRoomEvents[$entry]
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
    foreach ($match in [regex]::Matches([IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Progression/LoadStationDefinitions.cs')), 'public static LoadStation (\w+) \{ get; \} = new\("\1", AreaId\.(\w+)\);')) {
        $stationAreas[$match.Groups[1].Value] = $match.Groups[2].Value
    }
    $stationsByAreaNumber = @{}
    foreach ($match in [regex]::Matches([IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.AssetExtraction/Progression/LoadStationNumberCodec.cs')), '\[LoadStationDefinitions\.(\w+)\] = (\d+),')) {
        $stationsByAreaNumber['{0}:{1}' -f $stationAreas[$match.Groups[1].Value], $match.Groups[2].Value] = $match.Groups[1].Value
    }
    # Each population's rooms, from the room states that load it and the rooms whose state selection offers those states,
    # and each room's size in screens (#1624): a scroll change is named for the room whose populations place it and the
    # screens of that room it closes and opens.
    # Each state's summary says which room it belongs to and when the room loads it ("The Landing Site room's state once
    # Zebes is awake"); the catalogs' summaries reuse both phrases.
    $statePopulations = @{}
    $statePhrases = @{}
    $roomPhrases = @{}
    foreach ($stateFile in Get-ChildItem (Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/World/RoomStates') -Filter '*RoomStateDefinitions.cs') {
        foreach ($match in [regex]::Matches([IO.File]::ReadAllText($stateFile.FullName), '/// <summary>The ([^<]+?)''s ([^<]+)\.</summary>\s*public static RoomStateSettings (\w+) \{ get; \} =\s*new\("\w+"[^;]*?RoomObjectPopulationDefinitions\.(\w+)')) {
            $statePopulations[$match.Groups[3].Value] = $match.Groups[4].Value
            $statePhrases[$match.Groups[3].Value] = [pscustomobject]@{ Room = 'the ' + $match.Groups[1].Value; State = $match.Groups[2].Value }
        }
    }
    # Each population's rooms, and each room's states in its selection's order, the first its default.
    $populationRooms = @{}
    $roomStates = @{}
    $selectionText = [IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/World/RoomStates/RoomStateSelectionDefinitions.cs'))
    $roomChoices = [regex]::Matches($selectionText, '\[RoomDefinitions\.(\w+)\] = new\(')
    for ($choice = 0; $choice -lt $roomChoices.Count; $choice++) {
        $choiceEnd = if ($choice + 1 -lt $roomChoices.Count) { $roomChoices[$choice + 1].Index } else { $selectionText.Length }
        $room = $roomChoices[$choice].Groups[1].Value
        $roomStates[$room] = [Collections.Generic.List[string]]::new()
        foreach ($state in [regex]::Matches($selectionText.Substring($roomChoices[$choice].Index, $choiceEnd - $roomChoices[$choice].Index), '\w+RoomStateDefinitions\.(\w+)')) {
            $stateName = $state.Groups[1].Value
            if (-not $statePopulations.ContainsKey($stateName)) { throw "Room $room offers state $stateName, which names no room-object population or has no summary naming its room." }
            if ($roomStates[$room].Contains($stateName)) { throw "Room $room offers state $stateName twice." }
            $roomStates[$room].Add($stateName)
            $phrase = $statePhrases[$stateName]
            if ($roomPhrases.ContainsKey($room) -and $roomPhrases[$room] -cne $phrase.Room) { throw "Room $room's states name it both $($roomPhrases[$room]) and $($phrase.Room)." }
            $roomPhrases[$room] = $phrase.Room
            $statePopulation = $statePopulations[$stateName]
            if (-not $populationRooms.ContainsKey($statePopulation)) { $populationRooms[$statePopulation] = [Collections.Generic.HashSet[string]]::new() }
            [void] $populationRooms[$statePopulation].Add($room)
        }
    }
    # A population is named for its room, or for its room and the state that loads it; one that several, but not all, of a
    # room's states share is named for what they have in common (#2501), never for a list of those states.
    foreach ($population in $populationNames.Values) {
        if ($population -cmatch '(Default|When[A-Z]\w*|With[A-Z]\w*)Or[A-Z]|Or[A-Z]\w*(Default|When[A-Z]\w*)$') {
            throw "Population $population is named for a list of room states, not for what they share."
        }
    }
    # A sentence's list of phrases: "a", "a and b", "a, b and c".
    function Join-Phrases([string[]] $phrases) {
        if ($phrases.Count -le 2) { return $phrases -join ' and ' }
        return '{0} and {1}' -f ($phrases[0..($phrases.Count - 2)] -join ', '), $phrases[$phrases.Count - 1]
    }
    # Which of a room's states load any of the populations, said as "its default state and its state once ...", or empty
    # when every state of the room does.
    function Get-StatesClause([string] $room, [string[]] $populations) {
        [string[]] $states = @($roomStates[$room] | Where-Object { $populations -ccontains $statePopulations[$_] })
        if ($states.Count -eq 0) { throw "No state of room $room loads $($populations -join ', ')." }
        if ($states.Count -eq $roomStates[$room].Count) { return '' }
        return Join-Phrases @($states | ForEach-Object { 'its ' + $statePhrases[$_].State })
    }
    function Get-Capitalised([string] $text) { return $text.Substring(0, 1).ToUpperInvariant() + $text.Substring(1) }
    $roomScreens = @{}
    foreach ($match in [regex]::Matches([IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomDefinitions.cs')), 'public static RoomDefinition (\w+) \{ get; \} = (?:\w+\()?new\("\1", AreaId\.\w+, new\(\d+, \d+\), new\((\d+), (\d+)\)')) {
        $roomScreens[$match.Groups[1].Value] = [pscustomobject]@{ Width = [int]$match.Groups[2].Value; Height = [int]$match.Groups[3].Value }
    }
    if ($roomScreens.Count -ne 261) { throw "Expected 261 retail room sizes, found $($roomScreens.Count)." }
    $rom = [IO.File]::ReadAllBytes($romFile)
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('// <auto-generated/>')
    $lines.Add('#nullable enable')
    $lines.Add('using SuperMetroid.Core.Progression;')
    $lines.Add('using SuperMetroid.Core.Rooms.Doors;')
    $lines.Add('using SuperMetroid.Core.Rooms.Scrolling;')
    $lines.Add('')
    $lines.Add('namespace SuperMetroid.Core.Rooms.RoomObjects;')
    $lines.Add('')
    $lines.Add('/// <summary>')
    $lines.Add('/// The game''s room-object populations, each named for its room, for the state that loads it or for what the states')
    $lines.Add('/// that share it have in common. Each holds its placements')
    $lines.Add('/// in authored order: the object''s identity, block X and Y, and its family''s setup with the room argument decoded into what it reads; regenerate with')
    $lines.Add('/// tools/generate-room-object-population-definitions.ps1.')
    $lines.Add('/// </summary>')
    $lines.Add('internal static class RoomObjectPopulationDefinitions')
    $lines.Add('{')
    # Each placed kind's family setup (#2383), carrying what the kind places (#2481): the coloured, grey and eye doors,
    # the items, the map and refill stations and the scroll extensions are the kinds Core's table of kinds by setup lists,
    # each with the door, item, station access rule or extension it places; the scroll trigger and each other kind are
    # named for it. A setup carries every argument its family requires (#2483), so a placement of a family that requires
    # one and gives none is refused. A placement names its object once, by its setup, whose kind follows from it (#2557).
    $familySetups = @{}
    $familyVariants = @{}
    $variantSource = [IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectKindDefinitions.cs'))
    function Add-FamilyKinds([string] $table, [string] $setup) {
        $start = $variantSource.IndexOf(" $table ")
        if ($start -lt 0) { throw "RoomObjectKindDefinitions declares no $table table." }
        $end = $variantSource.IndexOf('}.ToFrozenDictionary();', $start)
        foreach ($match in [regex]::Matches($variantSource.Substring($start, $end - $start), '(?m)^\s*\[(.+)\] = RoomObjectKind\.(\w+),\r?$')) {
            if ($familySetups.ContainsKey($match.Groups[2].Value)) { throw "Room-object kind $($match.Groups[2].Value) is listed by two families." }
            $familySetups[$match.Groups[2].Value] = $setup
            $familyVariants[$match.Groups[2].Value] = $match.Groups[1].Value -replace '^new \w+\(', 'new('
        }
    }
    Add-FamilyKinds 'ColoredDoors' 'ColoredDoorSetup'
    Add-FamilyKinds 'GreyDoors' 'GreyDoorSetup'
    Add-FamilyKinds 'Items' 'CollectibleSetup'
    Add-FamilyKinds 'EyeDoorEyes' 'EyeDoorEyeSetup'
    Add-FamilyKinds 'EyeDoorParts' 'EyeDoorPartSetup'
    Add-FamilyKinds 'AccessStations' 'AccessStationSetup'
    Add-FamilyKinds 'ScrollExtensions' 'ScrollExtensionSetup'
    $draygonCannonSetups = @{}
    $cannonStart = $variantSource.IndexOf(' DraygonCannons ')
    if ($cannonStart -lt 0) { throw 'RoomObjectKindDefinitions declares no DraygonCannons table.' }
    foreach ($match in [regex]::Matches($variantSource.Substring($cannonStart, $variantSource.IndexOf('}.ToFrozenDictionary();', $cannonStart) - $cannonStart), '(?m)^\s*\[new DraygonCannonVariant\((.+)\)\] = RoomObjectKind\.(\w+),\r?$')) {
        $draygonCannonSetups[$match.Groups[2].Value] = $match.Groups[1].Value
    }
    if ($draygonCannonSetups.Count -ne 3) { throw "Expected three Draygon cannon kinds, found $($draygonCannonSetups.Count)." }
    function Format-PlacementSetup([string] $name, [string] $argumentText, [string] $scrollProgram) {
        if ($name -eq 'ScrollTrigger') { return 'new ScrollTriggerSetup({0})' -f $scrollProgram }
        # A Draygon cannon's placement carries the wall cannon it controls, the wall it faces out from and whether it starts
        # destroyed (#2400): the cartridge gives each facing and start an object definition of its own.
        if ($draygonCannonSetups.ContainsKey($name)) {
            if ($argumentText -notmatch '^null, (DraygonWallCannon\.\w+)$') { throw "Draygon cannon $name names no wall cannon: $argumentText." }
            return 'new DraygonWallCannonSetup({0}, {1})' -f $Matches[1], $draygonCannonSetups[$name]
        }
        if ($argumentText -match '^null, DraygonWallCannon\.') { throw "Room-object kind $name controls a Draygon wall cannon but is no cannon." }
        if ($familySetups.ContainsKey($name)) {
            $setup = $familySetups[$name]
            if ($setup -in @('ScrollExtensionSetup', 'AccessStationSetup')) {
                if ($argumentText -ne 'null') { throw "Room-object kind $name reads no argument, but its placement gives $argumentText." }
                return 'new {0}({1})' -f $setup, $familyVariants[$name]
            }
            if ($argumentText -eq 'null') { throw "Room-object kind $name requires its argument, but its placement gives none." }
            return 'new {0}({1}, {2})' -f $setup, $familyVariants[$name], $argumentText
        }
        $setup = $name + 'Setup'
        if ($setup -in @('MetroidRoomClearanceSetup', 'DownwardGateShotBlockSetup', 'SaveStationSetup')) {
            if ($argumentText -eq 'null') { throw "Room-object kind $name requires its argument, but its placement gives none." }
            return 'new {0}({1})' -f $setup, $argumentText
        }
        if ($argumentText -ne 'null') { throw "Room-object kind $name reads no argument, but its placement gives $argumentText." }
        return 'new {0}()' -f $setup
    }
    $recordTotal = 0
    # The first placement of each door and item location, by room-data number: its population, kind and block.
    $doorPlacements = @{}
    $itemPlacements = @{}
    $placedWords = [Collections.Generic.HashSet[int]]::new()
    # Whether any population places an escape room's held place, which the cartridge stores as a clearance watch.
    $heldPlacePlaced = $false
    $scrollProgramPointers = [Collections.Generic.HashSet[int]]::new()
    # The one room whose populations place each scroll program; the program's name is resolved once its writes are decoded.
    $scrollRooms = @{}
    # The retail scroll extensions that continue no trigger or extension, which the catalog leaves out.
    $orphanExtensions = 0
    # A Draygon wall cannon's room argument is the WRAM address of the control word it reads and writes; the catalog
    # carries the decoded word instead.
    $draygonCannonControls = @{ 0x8802 = 'UpperLeft'; 0x8804 = 'LowerLeft'; 0x8806 = 'UpperRight'; 0x8808 = 'LowerRight'; 0x880A = 'UnusedBottom' }
    foreach ($pointer in $ordered) {
        $offset = 0x78000 + ($pointer - 0x8000)
        $name = $populationNames[$pointer]
        if (-not $populationRooms.ContainsKey($name)) { throw "Population $name is loaded by no room state." }
        [string[]] $loaders = @($roomStates.Keys | Where-Object { $populationRooms[$name].Contains($_) } | Sort-Object | ForEach-Object {
            $clause = Get-StatesClause $_ @($name)
            '{0} places in {1}' -f $roomPhrases[$_], $(if ($clause -eq '') { 'every state' } else { $clause })
        })
        $summaryIndex = $lines.Count
        $lines.Add(('    /// <summary>The room objects {0}.</summary>' -f (Join-Phrases $loaders)))
        $lines.Add(('    public static RoomObjectPopulation {0} {{ get; }} = RoomObjectPopulation.FromPlacements(nameof({0}),' -f $name))
        $lines.Add('    [')
        $count = 0
        $placementLines = [Collections.Generic.List[object]]::new()
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
                $scrollProgram = 'RoomScrollChange.@@SCROLL:{0}@@' -f $scrollProgramPointer
            }
            if (-not $kindNames.ContainsKey($kindWord)) { throw ('Retail header word {0:X4} names no room-object kind.' -f $kindWord) }
            $placedKind = $kindNames[$kindWord]
            $argument = [int]$rom[$offset + 4] -bor ([int]$rom[$offset + 5] -shl 8)
            # The import decodes each placement to the block it occupies (#2034): the game finds it as its row times the
            # room's width plus its column, so a column past the width is the block that many columns into the following
            # rows, and a row past the room's last is a block of the terrain beyond the room.
            $blockX = [int]$rom[$offset + 2]
            $blockY = [int]$rom[$offset + 3]
            $site = ''
            [string[]] $placedRooms = @()
            if ($populationRooms.ContainsKey($name)) { $placedRooms = [string[]] @($populationRooms[$name]) }
            $decoded = @($placedRooms | ForEach-Object {
                $screens = $roomScreens[$_]
                $width = $screens.Width * 16
                $index = $blockY * $width + $blockX
                $row = [int][Math]::Floor($index / $width)
                '{0},{1},{2}' -f ($index % $width), $row, ($row -ge $screens.Height * 16)
            } | Sort-Object -Unique)
            if ($decoded.Count -gt 1) { throw "Population $name places $($kindNames[$kindWord]) at ($blockX,$blockY), which its rooms decode to different blocks." }
            if ($decoded.Count -eq 1) {
                $parts = $decoded[0].Split(',')
                $blockX = [int]$parts[0]
                $blockY = [int]$parts[1]
                if ($parts[2] -eq 'True') { $site = ', Site: RoomObjectSite.OffTheRoom' }
            }
            if ($kindNames[$kindWord].StartsWith('DraygonCannon')) {
                if (-not $draygonCannonControls.ContainsKey($argument)) {
                    throw ('Draygon cannon argument {0:X4} names no control word.' -f $argument)
                }
                $argumentText = 'null, DraygonWallCannon.{0}' -f $draygonCannonControls[$argument]
            }
            else {
                $argumentText = Format-RoomArgument $kindNames[$kindWord] $argument
                if ($argumentText -eq '@@HELDPLACE@@') {
                    if ($name -notmatch 'ZebesTimebombSet|TourianEscape') { throw "Population $name holds a place for good outside the escape rooms." }
                    $placedKind = 'EscapeRoomHeldPlace'
                    $argumentText = 'null'
                    $heldPlacePlaced = $true
                }
                if ($argumentText -match '^@@STATION:(\d+)@@$') {
                    [string[]] $areas = @()
                    if ($populationAreas.ContainsKey($name)) { $areas = [string[]] @($populationAreas[$name]) }
                    if ($areas.Count -ne 1) { throw "Save station population $name is loaded by no single area." }
                    $stationKey = '{0}:{1}' -f $areas[0], $Matches[1]
                    if (-not $stationsByAreaNumber.ContainsKey($stationKey)) { throw "Save station population $name names no load station $stationKey." }
                    $argumentText = 'new SaveStationPlace(LoadStationDefinitions.{0})' -f $stationsByAreaNumber[$stationKey]
                }
                foreach ($token in [regex]::Matches($argumentText, '@@(DOOR|ITEM):(\d+)@@')) {
                    $placements = if ($token.Groups[1].Value -eq 'DOOR') { $doorPlacements } else { $itemPlacements }
                    $number = [int]$token.Groups[2].Value
                    # A door or item location stands in one room, whichever of its states places it (#2491, #2501): it is
                    # named for that room and its kind, never for the states that load the population; a kind named for its
                    # room (Bomb Torizo's grey door) is not named for the room twice.
                    if ($placedRooms.Count -ne 1) { throw "Population $name places $($token.Groups[1].Value) $number but is loaded by $($placedRooms.Count) rooms, not one." }
                    if (-not $placements.ContainsKey($number)) {
                        $condition = if ($argumentText -match 'GreyDoorCondition\.(\w+)') { $Matches[1] } else { $null }
                        $persistence = if ($argumentText -match 'DoorOpeningPersistence\.(\w+)') { $Matches[1] } else { $null }
                        $placedKindName = $kindNames[$kindWord]
                        $base = if ($placedKindName.StartsWith($placedRooms[0], [StringComparison]::Ordinal)) { $placedKindName } else { $placedRooms[0] + $placedKindName }
                        $placements[$number] = [pscustomobject]@{
                            Base = $base; Room = $placedRooms[0]; Kind = $placedKindName
                            X = $rom[$offset + 2]; Y = $rom[$offset + 3]; BlockX = $blockX; BlockY = $blockY
                            Condition = $condition; Persistence = $persistence
                            Populations = [Collections.Generic.List[string]]::new()
                        }
                    }
                    elseif ($placements[$number].Room -cne $placedRooms[0]) {
                        throw "$($token.Groups[1].Value) $number is placed in both $($placements[$number].Room) and $($placedRooms[0])."
                    }
                    if (-not $placements[$number].Populations.Contains($name)) { $placements[$number].Populations.Add($name) }
                }
            }
            $setupText = Format-PlacementSetup $placedKind $argumentText $scrollProgram
            $placementLines.Add([pscustomobject]@{ Kind = $placedKind; X = $blockX; Y = $blockY; Program = $scrollProgram
                Line = ('        new(new({0}, {1}), {2}{3}),' -f $blockX, $blockY, $setupText, $site) })
            $offset += 6
        }
        # A scroll extension continues the trigger or extension its step back leads to (#2572); one whose step back leads to
        # none extends nothing, changing nothing the game reads, and is left out. East Pants' rightwards extension at
        # (10, 18) is the only one.
        $scrollChain = [Collections.Generic.HashSet[string]]::new()
        foreach ($placed in $placementLines) {
            if ($placed.Kind -ceq 'ScrollTrigger' -or $placed.Kind.EndsWith('ScrollExtension')) { [void] $scrollChain.Add(('{0},{1}' -f $placed.X, $placed.Y)) }
        }
        $stepsBack = @{ RightwardsScrollExtension = @(-1, 0); LeftwardsScrollExtension = @(1, 0); DownwardsScrollExtension = @(0, -1); UpwardsScrollExtension = @(0, 1) }
        # Each trigger is placed once with the reach its extensions give it (#2557): the run of blocks the extensions
        # continue it through, each straight stretch one run of its extension. An extension's setup only writes its own
        # block and frees its place at once, so placing the trigger's extensions straight after it, rather than where the
        # cartridge lists them, takes no place another object would take and changes nothing the game reads. Every
        # retail reach is one unbranched path from its trigger.
        $extensionsByBlock = @{}
        foreach ($placed in $placementLines) {
            if ($stepsBack.ContainsKey($placed.Kind)) {
                $back = $stepsBack[$placed.Kind]
                $from = '{0},{1}' -f ($placed.X + $back[0]), ($placed.Y + $back[1])
                if (-not $scrollChain.Contains($from)) {
                    $orphanExtensions++
                    continue
                }
                if ($extensionsByBlock.ContainsKey($from)) { throw "Population $name branches the reach of its scroll trigger at ($from)." }
                $extensionsByBlock[$from] = $placed
            }
        }
        $reached = 0
        foreach ($placed in $placementLines) {
            if ($stepsBack.ContainsKey($placed.Kind)) { continue }
            if ($placed.Kind -cne 'ScrollTrigger') {
                $lines.Add($placed.Line)
                continue
            }
            $runs = [Collections.Generic.List[object]]::new()
            $at = '{0},{1}' -f $placed.X, $placed.Y
            while ($extensionsByBlock.ContainsKey($at)) {
                $extension = $extensionsByBlock[$at]
                if ($runs.Count -gt 0 -and $runs[$runs.Count - 1].Kind -ceq $extension.Kind) { $runs[$runs.Count - 1].Blocks++ }
                else { $runs.Add([pscustomobject]@{ Kind = $extension.Kind; Blocks = 1 }) }
                $reached++
                $at = '{0},{1}' -f $extension.X, $extension.Y
            }
            if ($runs.Count -eq 0) {
                $lines.Add($placed.Line)
                continue
            }
            [string[]] $reach = @($runs | ForEach-Object { 'new ScrollReachRun({0}, {1})' -f $familyVariants[$_.Kind], $_.Blocks })
            $lines.Add(('        .. ScrollTriggerPlacements.Reaching(new({0}, {1}), {2}, {3}),' -f $placed.X, $placed.Y, $placed.Program, ($reach -join ', ')))
        }
        if ($reached -ne $extensionsByBlock.Count) { throw "Population $name places scroll extensions that no trigger's reach covers." }
        $recordTotal += $count
        if ($count -eq 0) { $lines[$summaryIndex] = $lines[$summaryIndex] -replace '\.</summary>$', ': none.</summary>' }
        $lines.Add('    ]);')
        $lines.Add('')
    }
    if ($recordTotal -ne 941) {
        throw "Expected 941 retail PLM records, found $recordTotal."
    }
    if ($orphanExtensions -ne 1) {
        throw "Expected one retail scroll extension that extends nothing, found $orphanExtensions."
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
    $scrollChangeContents = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
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
        $changeContent = $room
        $phrases = [Collections.Generic.List[string]]::new()
        foreach ($effect in $scrollEffects) {
            $affectedWrites = @($writes | Where-Object State -EQ $effect.State | Sort-Object Cell)
            if ($affectedWrites.Count -eq 0) { continue }
            [string[]] $affected = @($affectedWrites | ForEach-Object { '{0}x{1}{2}' -f $_.Column, $_.Row, $(if ($_.BelowTheRoom) { 'BelowTheRoom' } else { '' }) })
            [string[]] $described = @($affectedWrites | ForEach-Object { '{0}x{1}{2}' -f $_.Column, $_.Row, $(if ($_.BelowTheRoom) { ' below the room' } else { '' }) })
            $noun = if ($affected.Count -eq 1) { 'Screen' } else { 'Screens' }
            $changeContent += '{0}{1}{2}{3}' -f $effect.Verb, $noun, ($affected -join 'And'), $effect.Suffix
            $phrases.Add(($effect.Phrase -f ('{0} {1}' -f $noun.ToLowerInvariant(), ($described -join ' and '))))
        }
        if (-not $scrollChangeContents.Add($changeContent)) { throw "Two scroll programs of the $room room make the same change." }
        $scrollChanges[$pointer] = [pscustomobject]@{ Name = $null; Room = $room; Pointer = $pointer; Writes = $writes; Summary = ($phrases -join '; ') }
    }
    # A scroll change is an identity of its room (#2574): named for the room, numbered in storage order when the room has
    # more than one; its writes are stated once, in the change's definition, never again in its name.
    foreach ($roomChanges in @($scrollChanges.Values | Group-Object Room)) {
        [object[]] $ordered = @($roomChanges.Group | Sort-Object Pointer)
        for ($index = 0; $index -lt $ordered.Count; $index++) {
            $ordered[$index].Name = if ($ordered.Count -eq 1) { $roomChanges.Name } else { '{0}{1}' -f $roomChanges.Name, ($index + 1) }
        }
    }
    if ($scrollByteTotal -ne 743 -or $scrollPairTotal -ne 285) {
        throw "Expected 743 scroll-program bytes/285 pairs, found $scrollByteTotal/$scrollPairTotal."
    }

    # A door or item location is named for its room and kind (#2501). Where two would share that name, each is told apart by
    # the first of these that differs among them: a grey door's condition, the block it stands on, and last the one state
    # of the room that places it, which only the Climb's escape needs: its grey doors stand where the doors its other states
    # place do, with the same condition.
    $greyConditionNames = @{
        AreaBossDefeated = 'UntilAreaBossDead'; AreaMiniBossDefeated = 'UntilAreaMiniBossDead'; AreaTorizoDefeated = 'UntilAreaTorizoDead'
        EnemyDeathQuota = 'UntilEnemiesKilled'; Never = 'NeverOpening'; TourianStatueFinished = 'UntilStatuesFinished'; CrittersEscaped = 'UntilCrittersEscaped'
    }
    function Get-PlacementPart($entry, [string] $step) {
        if ($step -eq 'Condition') {
            if ($null -eq $entry.Condition) { return '' }
            if (-not $greyConditionNames.ContainsKey($entry.Condition)) { throw "Grey door condition $($entry.Condition) has no name." }
            return $greyConditionNames[$entry.Condition]
        }
        if ($step -eq 'Block') { return 'At{0}x{1}' -f $entry.X, $entry.Y }
        [string[]] $states = @($roomStates[$entry.Room] | Where-Object { $entry.Populations -ccontains $statePopulations[$_] })
        if ($states -ccontains $roomStates[$entry.Room][0]) { return '' }
        if ($states.Count -ne 1 -or -not $states[0].StartsWith($entry.Room, [StringComparison]::Ordinal)) {
            throw "$($entry.Base) at ($($entry.X),$($entry.Y)) is told apart only by the states $($states -join ', ') that place it."
        }
        return $states[0].Substring($entry.Room.Length)
    }
    function Get-PlacementNames($placements) {
        $names = @{}
        foreach ($number in $placements.Keys) { $names[$number] = $placements[$number].Base }
        foreach ($step in @('Condition', 'Block', 'State')) {
            $groups = [Collections.Generic.Dictionary[string, Collections.Generic.List[int]]]::new([StringComparer]::Ordinal)
            foreach ($number in $placements.Keys) {
                if (-not $groups.ContainsKey($names[$number])) { $groups[$names[$number]] = [Collections.Generic.List[int]]::new() }
                $groups[$names[$number]].Add($number)
            }
            foreach ($group in $groups.Values) {
                if ($group.Count -lt 2) { continue }
                $parts = @{}
                foreach ($number in $group) { $parts[$number] = Get-PlacementPart $placements[$number] $step }
                if (@($parts.Values | Sort-Object -Unique -CaseSensitive).Count -lt 2) { continue }
                foreach ($number in $group) { $names[$number] += $parts[$number] }
            }
        }
        $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($number in $placements.Keys) {
            if (-not $seen.Add($names[$number])) { throw "Two placements share the name $($names[$number])." }
        }
        return $names
    }
    $doorNames = Get-PlacementNames $doorPlacements
    $itemNames = Get-PlacementNames $itemPlacements
    # Each placement's summary says what it is, where it stands and, when not every state of its room places it, which do.
    function Get-PlacedAt($entry) {
        $clause = Get-StatesClause $entry.Room @($entry.Populations)
        $states = if ($clause -eq '') { '' } else { ', in {0}' -f $clause }
        return 'at column {0}, row {1} of {2}{3}' -f $entry.BlockX, $entry.BlockY, $roomPhrases[$entry.Room], $states
    }
    $greyConditionPhrases = @{
        AreaBossDefeated = 'the area''s boss is dead'; AreaMiniBossDefeated = 'the area''s mini-boss is dead'; AreaTorizoDefeated = 'the area''s Torizo is dead'
        EnemyDeathQuota = 'enough of the room''s enemies are killed'; TourianStatueFinished = 'the Tourian statues have finished'; CrittersEscaped = 'the critters have escaped'
    }
    $doorSummaries = @{}
    foreach ($number in $doorPlacements.Keys) {
        $entry = $doorPlacements[$number]
        $door = if ($entry.Kind -cmatch '^(Red|Green|Yellow|Grey|Eye)DoorFacing(Left|Right|Up|Down)$') {
            '{0} door facing {1}' -f $Matches[1].ToLowerInvariant(), $Matches[2].ToLowerInvariant()
        }
        elseif ($entry.Kind -ceq 'BombTorizoGreyDoor') { 'Bomb Torizo''s grey door' }
        else { throw "Door kind $($entry.Kind) has no description." }
        $kept = switch ($entry.Persistence) {
            'Persistent' { 'once opened, it stays open for good' }
            'Transient' { 'the room shuts it again whenever it loads' }
            default { throw "Door $($entry.Base) has no opening persistence." }
        }
        $opens = if ($null -eq $entry.Condition) { $kept }
            elseif ($entry.Condition -ceq 'Never') { 'it never opens' }
            elseif ($greyConditionPhrases.ContainsKey($entry.Condition)) { 'it stays shut until {0}, and {1}' -f $greyConditionPhrases[$entry.Condition], $kept }
            else { throw "Grey door condition $($entry.Condition) has no description." }
        $doorSummaries[$number] = 'The {0} {1}: {2}.' -f $door, (Get-PlacedAt $entry), $opens
    }
    # An item location is what its kind places: "A Missile Tank lying in the open" becomes "The Missile Tank lying in the open".
    $kindSummaries = @{}
    foreach ($match in [regex]::Matches([IO.File]::ReadAllText((Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectKind.cs')), '/// <summary>(?:An?|The) ([^<]+)\.</summary>\s*(\w+),')) {
        $kindSummaries[$match.Groups[2].Value] = $match.Groups[1].Value
    }
    $itemSummaries = @{}
    foreach ($number in $itemPlacements.Keys) {
        $entry = $itemPlacements[$number]
        if (-not $kindSummaries.ContainsKey($entry.Kind)) { throw "Item kind $($entry.Kind) has no summary saying what it places." }
        $itemSummaries[$number] = 'The {0} {1}.' -f $kindSummaries[$entry.Kind], (Get-PlacedAt $entry)
    }
    $evaluator = [Text.RegularExpressions.MatchEvaluator] {
        param($match)
        if ($match.Groups[1].Value -eq 'DOOR') { 'LockedDoorDefinitions.' + $doorNames[[int]$match.Groups[2].Value] }
        elseif ($match.Groups[1].Value -eq 'SCROLL') { $scrollChanges[[int]$match.Groups[2].Value].Name }
        else { 'ItemLocationDefinitions.' + $itemNames[[int]$match.Groups[2].Value] }
    }
    for ($index = 0; $index -lt $lines.Count; $index++) {
        $lines[$index] = [regex]::Replace($lines[$index], '@@(DOOR|ITEM|SCROLL):(\d+)@@', $evaluator)
    }

    function New-PlacementCatalog([string] $typeName, [string] $catalogName, [string[]] $summary, $memberSummaries, [string] $allSummary, [string] $numberedReason, $names) {
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
            $catalog.Add(('    /// <summary>{0}</summary>' -f $memberSummaries[$number]))
            $catalog.Add(('    public static {0} {1} {{ get; }} = new("{1}");' -f $typeName, $names[$number]))
            $catalog.Add('')
        }
        $catalog.Add(('    /// <summary>{0}</summary>' -f $allSummary))
        $catalog.Add(('    public static IReadOnlyList<{0}> All {{ get; }} =' -f $typeName))
        $catalog.Add('    [')
        foreach ($number in @($names.Keys | Sort-Object)) { $catalog.Add(('        {0},' -f $names[$number])) }
        $catalog.Add('    ];')
        $catalog.Add('}')
        return $catalog
    }
    $doorCatalog = New-PlacementCatalog 'LockedDoor' 'LockedDoorDefinitions' @(
        'Every door of the game''s room populations that a cap keeps shut: each coloured, grey and eye door, named for the',
        'room that places it, whichever of its states does, and its cap and, where two would share that name, the grey door''s',
        'condition, its block or, for the Climb''s escape, the state that places it; an eye door''s three parts are one door.',
        'Regenerate with tools/generate-room-object-population-definitions.ps1.'
    ) $doorSummaries 'Every door, in the order saves list them.' 'Each door named with its block is one distinct door of its room, told apart by where it stands; nothing indexes them.' $doorNames
    $itemCatalog = New-PlacementCatalog 'ItemLocation' 'ItemLocationDefinitions' @(
        'Every item location of the game''s room populations, named for the room that places it, whichever of its states does,',
        'and its item and, where two would share that name, its block. Regenerate with',
        'tools/generate-room-object-population-definitions.ps1.'
    ) $itemSummaries 'Every item location, in the order saves list them.' 'Each item location named with its block is one distinct location of its room, told apart by where it stands; nothing indexes them.' $itemNames
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
    [IO.File]::WriteAllLines($outputFile, $lines, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllLines($doorCatalogFile, $doorCatalog, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllLines($itemCatalogFile, $itemCatalog, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllLines($placementNumbersFile, $numberLines, [Text.UTF8Encoding]::new($false))

    # Every room-object kind is one a retail population places (#2354, #2487), so Core keeps no set of placed kinds: a
    # placement's kind is any kind but none. The held place is stored under the clearance watch's header word.
    if ($placedWords.Count -ne 70) {
        throw "Expected 70 distinct placed room-object kinds, found $($placedWords.Count)."
    }
    foreach ($kindWord in $placedWords) {
        if (-not $kindNames.ContainsKey($kindWord)) { throw ('Retail header word {0:X4} names no room-object kind.' -f $kindWord) }
    }
    foreach ($kindWord in $kindNames.Keys) {
        if (-not $placedWords.Contains($kindWord)) { throw ('Room-object kind {0} is placed by no retail population.' -f $kindNames[$kindWord]) }
    }
    if (-not $heldPlacePlaced) { throw 'No retail population places the escape rooms'' held place.' }

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
    $scrollLines.Add('    /// <summary>Each scroll change''s ordered writes, each screen by its column and row in the change''s room.</summary>')
    $scrollLines.Add('    internal static readonly FrozenDictionary<RoomScrollChange, ScreenScrollChange[]> Changes =')
    $scrollLines.Add('        new Dictionary<RoomScrollChange, ScreenScrollChange[]>')
    $scrollLines.Add('        {')
    $identityLines = [Collections.Generic.List[string]]::new()
    $identityLines.Add('// <auto-generated/>')
    $identityLines.Add('namespace SuperMetroid.Core.Rooms.RoomObjects;')
    $identityLines.Add('')
    $identityLines.Add('/// <summary>')
    $identityLines.Add('/// A scroll change: one of its room''s, named for the room and numbered in storage order where the room has more than')
    $identityLines.Add('/// one (#2574). What it does to the room''s screens is its definition''s (RoomObjectScrollChangeDefinitions.Changes).')
    $identityLines.Add('/// Regenerate with tools/generate-room-object-population-definitions.ps1.')
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
        if (-not $roomPhrases.ContainsKey($change.Room)) { throw "Scroll change $($change.Name)'s room $($change.Room) has no state summary naming it." }
        $identityLines.Add(('    /// <summary>In {0}, {1}.</summary>' -f $roomPhrases[$change.Room], $change.Summary))
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
    Write-Output "Generated $($ordered.Count) PLM populations, $recordTotal records, $($placedWords.Count) placed kinds and $($orderedScrollPrograms.Count) scroll programs."
}
catch {
    [Console]::Error.WriteLine($_.Exception.ToString())
    exit 1
}
