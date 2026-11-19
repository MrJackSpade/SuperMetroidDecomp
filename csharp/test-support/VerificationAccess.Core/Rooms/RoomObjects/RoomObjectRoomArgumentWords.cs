using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The native room-argument word of a PLM (#1452): Core keeps the typed argument the import decoded, and verification
/// translates between it and the word the cartridge stores and work RAM holds, where it decodes native populations and
/// compares the port's slots with native memory.
/// </summary>
internal static class RoomObjectRoomArgumentWords
{
    /// <summary>The word's sign bit: a door or item argument with it set never persists.</summary>
    private const int NeverPersists = 0x8000;

    /// <summary>The bits of a grey door's word that hold its door bit.</summary>
    private const int GreyDoorBitMask = 0x3ff;

    /// <summary>The lowest bit of a grey door's unlock-condition index.</summary>
    private const int GreyConditionShift = 10;

    /// <summary>The bits of a grey door's unlock-condition index once shifted down.</summary>
    private const int GreyConditionMask = 0x1f;

    /// <summary>The unlock conditions in the order of the cartridge's grey-door condition table, which a grey door's word indexes.</summary>
    private static readonly GreyDoorCondition[] GreyDoorConditionsInTableOrder =
    [
        GreyDoorCondition.AreaBossDefeated, GreyDoorCondition.AreaMiniBossDefeated, GreyDoorCondition.AreaTorizoDefeated,
        GreyDoorCondition.EnemyDeathQuota, GreyDoorCondition.Never, GreyDoorCondition.TourianStatueFinished,
        GreyDoorCondition.CrittersEscaped,
    ];

    /// <summary>
    /// The shot-block triggers in the order of the cartridge's trigger tables (blue, red, green, yellow; left then right),
    /// which a gate trigger's word indexes in steps of two.
    /// </summary>
    internal static IReadOnlyList<DownwardGateTrigger> GateTriggersInTableOrder { get; } =
    [
        new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Left), new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Right),
        new DownwardGateTrigger(DownwardGateTriggerColor.Red, HorizontalDirection.Left), new DownwardGateTrigger(DownwardGateTriggerColor.Red, HorizontalDirection.Right),
        new DownwardGateTrigger(DownwardGateTriggerColor.Green, HorizontalDirection.Left), new DownwardGateTrigger(DownwardGateTriggerColor.Green, HorizontalDirection.Right),
        new DownwardGateTrigger(DownwardGateTriggerColor.Yellow, HorizontalDirection.Left), new DownwardGateTrigger(DownwardGateTriggerColor.Yellow, HorizontalDirection.Right),
    ];

    /// <summary>The entries of the cartridge's Metroids-cleared table, which a clearance watch's word indexes in steps of two.</summary>
    internal const int MetroidRoomEntryCount = 13;

    /// <summary>The table's leading entries, which observe nothing.</summary>
    internal const int MetroidRoomNoOpEntryCount = 9;

    /// <summary>The events the table's quota observers mark, in entry order after the no-op entries.</summary>
    private static readonly StoryEvent[] MetroidRoomQuotaObserverEvents =
    [
        StoryEvent.FirstMetroidHallCleared,
        StoryEvent.FirstMetroidShaftCleared,
        StoryEvent.SecondMetroidHallCleared,
        StoryEvent.SecondMetroidShaftCleared,
    ];

    /// <summary>The word a placement that carries nothing for its setup holds.</summary>
    private const ushort Filler = 0x8000;

    /// <summary>Headers whose retail placements carry a fixed word that no setup reads.</summary>
    private static readonly Dictionary<RoomObjectKind, ushort> FixedWords = new Dictionary<RoomObjectKind, ushort>
    {
        [RoomObjectKind.NoobTube] = 0x0080,
        [RoomObjectKind.BombTorizoHand] = 0x0000,
        [RoomObjectKind.DraygonCannonFacingLeft] = 0x0000,
        [RoomObjectKind.DraygonCannonFacingRight] = 0x0000,
        [RoomObjectKind.DraygonCannonFacingRightDestroyed] = 0x0000,
    };

    /// <summary>Headers whose retail placements carry the filler word.</summary>
    private static readonly HashSet<RoomObjectKind> FillerHeaders =
    [
        RoomObjectKind.DownwardGate, RoomObjectKind.RightwardsScrollExtension, RoomObjectKind.LeftwardsScrollExtension,
        RoomObjectKind.DownwardsScrollExtension, RoomObjectKind.UpwardsScrollExtension, RoomObjectKind.ElevatorPlatform,
        RoomObjectKind.MapStation, RoomObjectKind.MotherBrainEscapeRoomGate, RoomObjectKind.MotherBrainGlass,
        RoomObjectKind.SpeedBoosterEscape, RoomObjectKind.WreckedShipAtticHeldPlace,
    ];

    /// <summary>
    /// Whether <paramref name="header"/>'s retail placements carry a number that nothing reads: a refill station's word
    /// numbers it in room data, but neither its setup nor its refill reads the number (#1623), so the import places the
    /// station without an argument and the number is not kept.
    /// </summary>
    internal static bool NumbersUnread(RoomObjectKind header) =>
        header is RoomObjectKind.EnergyStation or RoomObjectKind.MissileStation;

    /// <summary>Whether a door word's opening is remembered: its sign bit set means it never is.</summary>
    private static DoorOpeningPersistence Persistence(int word) =>
        (word & NeverPersists) == 0 ? DoorOpeningPersistence.Persistent : DoorOpeningPersistence.Transient;

    /// <summary>Decodes a cartridge placement's word into the argument the import gives the placement.</summary>
    /// <param name="header">The placement's header.</param>
    /// <param name="word">The placement's room-argument word.</param>
    /// <param name="area">
    /// The area of the room the population is placed in, whose station list a save station's word numbers its station in;
    /// null when the population is decoded outside a room, which then holds no save station.
    /// </param>
    /// <exception cref="InvalidDataException">The word does not decode for the header.</exception>
    internal static RoomObjectRoomArgument? Decode(RoomObjectKind header, int word, SuperMetroid.Core.Progression.AreaId? area = null)
    {
        string name = header.ToString();
        if (header == RoomObjectKind.ScrollTrigger || FixedWords.ContainsKey(header) || FillerHeaders.Contains(header))
            return null;
        // A held place's word only selects one of the clearance table's no-op entries, which nothing reads after.
        if (header == RoomObjectKind.EscapeRoomHeldPlace)
            return PlacedKind(RoomObjectKind.MetroidRoomClearance, word) == header ? null
                : throw new InvalidDataException($"Held-place argument ${word:X4} selects a clearance watch, not one of the no-op entries.");
        if (RoomObjectKindVariants.Items.ContainsKey(header))
            return new CollectibleIdentity(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Item(word));
        if (name.StartsWith("GreyDoor", StringComparison.Ordinal) || header == RoomObjectKind.BombTorizoGreyDoor)
        {
            int condition = (word >> GreyConditionShift) & GreyConditionMask;
            return new GreyDoorOpening(condition < GreyDoorConditionsInTableOrder.Length ? GreyDoorConditionsInTableOrder[condition]
                    : throw new InvalidDataException($"Grey door argument ${word:X4} selects condition {condition}, past the condition table."),
                new DoorLock(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(word & GreyDoorBitMask), Persistence(word)));
        }
        if (name.StartsWith("RedDoor", StringComparison.Ordinal) || name.StartsWith("GreenDoor", StringComparison.Ordinal) ||
            name.StartsWith("YellowDoor", StringComparison.Ordinal) || name.StartsWith("EyeDoor", StringComparison.Ordinal))
        {
            return new DoorLock(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(word & ~NeverPersists), Persistence(word));
        }
        if (NumbersUnread(header))
            return null;
        if (header == RoomObjectKind.SaveStation)
        {
            return new SaveStationPlace(SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Decode(
                area ?? throw new InvalidDataException($"A save station's argument ${word:X4} names its station only in a room's area."),
                word, "A save station's room argument"));
        }
        if (header == RoomObjectKind.MetroidRoomClearance)
        {
            int entry = MetroidRoomEntry(word);
            return entry < MetroidRoomNoOpEntryCount
                ? throw new InvalidDataException($"Metroids-cleared argument ${word:X4} selects a no-op entry: the placement is an escape room's held place.")
                : new WatchedMetroidRoom(MetroidRoomQuotaObserverEvents[entry - MetroidRoomNoOpEntryCount]);
        }
        if (header == RoomObjectKind.DownwardGateShotBlock)
        {
            int trigger = (word & 1) == 0 ? word / 2 : throw new InvalidDataException($"Downward gate trigger argument ${word:X4} is odd.");
            return trigger < GateTriggersInTableOrder.Count
                ? new GateShotTrigger(GateTriggersInTableOrder[trigger])
                : throw new InvalidDataException($"Downward gate trigger argument ${word:X4} does not name one of the eight colours and sides.");
        }
        throw new InvalidDataException($"PLM {header} has no decoded room argument for ${word:X4}.");
    }

    /// <summary>
    /// The entry of the cartridge's Metroids-cleared table a clearance object's <paramref name="word"/> selects.
    /// </summary>
    /// <exception cref="InvalidDataException">The word is odd or past the table.</exception>
    private static int MetroidRoomEntry(int word)
    {
        int entry = (word & 1) == 0 ? word / 2 : throw new InvalidDataException($"Metroids-cleared argument ${word:X4} is odd.");
        return entry < MetroidRoomEntryCount ? entry
            : throw new InvalidDataException($"Metroids-cleared argument ${word:X4} is not one of its {MetroidRoomEntryCount} entries.");
    }

    /// <summary>
    /// The kind a cartridge record of <paramref name="header"/> with argument <paramref name="word"/> places: an escape
    /// room's held place for a Metroids-cleared object whose word selects one of its table's no-op entries (#2426), else the
    /// header's own kind.
    /// </summary>
    /// <exception cref="InvalidDataException">A Metroids-cleared object's word selects none of its table's entries.</exception>
    internal static RoomObjectKind PlacedKind(RoomObjectKind header, int word) =>
        header == RoomObjectKind.MetroidRoomClearance && MetroidRoomEntry(word) < MetroidRoomNoOpEntryCount
            ? RoomObjectKind.EscapeRoomHeldPlace
            : header;

    /// <summary>The header a placement of <paramref name="kind"/> is stored under: a held place's is the Metroids-cleared object's.</summary>
    internal static RoomObjectKind Header(RoomObjectKind kind) =>
        kind == RoomObjectKind.EscapeRoomHeldPlace ? RoomObjectKind.MetroidRoomClearance : kind;

    /// <summary>
    /// The native word of a placement's argument: its typed argument's word, the scroll program's place for a scroll
    /// trigger, the fixed or filler word of a placement that carries nothing, else zero. A held place has no word: the
    /// cartridge's is one of the clearance table's no-op entries, which the port does not keep.
    /// </summary>
    internal static ushort Encode(RoomObjectKind header, RoomObjectRoomArgument? argument, RoomScrollChange? scroll) =>
        header == RoomObjectKind.EscapeRoomHeldPlace
            ? throw new InvalidOperationException("A held place keeps no no-op entry's word.")
            : ArgumentWord(argument) ?? (header == RoomObjectKind.ScrollTrigger && scroll is { } program ? RoomScrollProgramNative.Addresses[program]
            : FixedWords.TryGetValue(header, out ushort fixedWord) ? fixedWord
            : FillerHeaders.Contains(header) || header == RoomObjectKind.ScrollTrigger ? Filler
            : (ushort)0);

    /// <summary>
    /// The native word of a live object's argument (#1691): the hit count for a cannon or the glass once set up, else the
    /// word of what its placement gave it, which it holds (#2091), else the scroll program's place for a scroll trigger. A
    /// live object that holds none of these has no word here: the cartridge's is its placement's filler or its spawner's,
    /// which names no state the port keeps.
    /// </summary>
    private static ushort? HeldWord(RoomObjectPlace place) =>
        HitWord(place) is { } hits ? unchecked((ushort)hits)
        : PlacedWord(place.Behaviour)
            ?? ((place.Behaviour as ScrollTriggerObject)?.Change is { } program ? (ushort?)RoomScrollProgramNative.Addresses[program] : null);

    /// <summary>
    /// The native word of what a live object's placement gave it, as the object holds it: a door object's door (its spent
    /// mark once it recorded the door opened, or the cleared word a shot-opened cap keeps), an item's location, a save
    /// station's load station, a clearance watch's event, or a gate trigger's shot block; null for none.
    /// </summary>
    private static ushort? PlacedWord(RoomObjectBehaviour? behaviour) => behaviour switch
    {
        // A shot-opened cap's argument is the cleared word, which names the door its closing rule consults.
        BlueDoorCapObject { OpenedByShot: true } => checked((ushort)SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(LockedDoorDefinitions.LandingSiteGreenDoorFacingLeft)),
        ColoredDoorObject cap => DoorWord(cap.Door),
        GreyDoorObject cap => DoorWord(cap.Door),
        BlueDoorCapObject cap => DoorWord(cap.Door),
        EyeDoorEyeObject eye => DoorWord(eye.Door),
        EyeDoorPartObject part => DoorWord(part.Door),
        CollectibleObject item => ArgumentWord(new CollectibleIdentity(item.Location)),
        SaveStationObject station => ArgumentWord(new SaveStationPlace(station.SavedAt)),
        MetroidRoomClearanceObject watch => ArgumentWord(new WatchedMetroidRoom(watch.Cleared)),
        TerrainChangeObject { Change: var change } when GateTriggerDrawing(change) is { } trigger =>
            ArgumentWord(new GateShotTrigger(trigger)),
        _ => null,
    };

    /// <summary>The gate trigger whose shot block <paramref name="change"/> draws, or null for any other terrain change.</summary>
    private static DownwardGateTrigger? GateTriggerDrawing(TerrainChange change)
    {
        foreach (DownwardGateTrigger trigger in GateTriggersInTableOrder)
        {
            if (DownwardGateTriggerDefinitions.Triggers[trigger].Change == change)
                return trigger;
        }
        return null;
    }

    /// <summary>The native word of a door object's door: its spent mark once it recorded the door opened, else its door's.</summary>
    private static ushort? DoorWord(PlacedDoor? door) => door is null ? null : door.Spent ? SpentMark : ArgumentWord(door.Placed);

    /// <summary>Whether a live object is a door object that recorded its door opened for good, whose word keeps only its spent mark.</summary>
    internal static bool IsSpentDoor(RoomObjectBehaviour? behaviour) => behaviour switch
    {
        ColoredDoorObject cap => cap.Door.Spent,
        GreyDoorObject cap => cap.Door.Spent,
        BlueDoorCapObject cap => cap.Door?.Spent == true,
        EyeDoorEyeObject eye => eye.Door.Spent,
        EyeDoorPartObject part => part.Door.Spent,
        _ => false,
    };

    /// <summary>The native word of a typed argument, or null for none.</summary>
    private static ushort? ArgumentWord(RoomObjectRoomArgument? argument) =>
        argument switch
        {
            CollectibleIdentity item => checked((ushort)SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(item.Location)),
            DoorLock door => checked((ushort)(SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(door.Door) | (door.Persists ? 0 : NeverPersists))),
            GreyDoorOpening grey => checked((ushort)((grey.Door.Persists ? 0 : NeverPersists) |
                (Array.IndexOf(GreyDoorConditionsInTableOrder, grey.Condition) << GreyConditionShift) | SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(grey.Door.Door))),
            SaveStationPlace saved => checked((ushort)SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Number(saved.Station)),
            WatchedMetroidRoom watch =>
                checked((ushort)((MetroidRoomNoOpEntryCount + Array.IndexOf(MetroidRoomQuotaObserverEvents, watch.Cleared)) * 2)),
            GateShotTrigger gate => checked((ushort)(GateTriggerIndex(gate.Trigger) * 2)),
            null => null,
            _ => throw new InvalidOperationException($"Undefined room argument {argument}."),
        };

    /// <summary>The trigger's place in the cartridge's trigger tables.</summary>
    private static int GateTriggerIndex(DownwardGateTrigger trigger)
    {
        for (int index = 0; index < GateTriggersInTableOrder.Count; index++)
        {
            if (GateTriggersInTableOrder[index] == trigger)
                return index;
        }
        throw new InvalidOperationException($"Downward gate trigger {trigger} is not in the trigger tables.");
    }

    /// <summary>
    /// The part of a spent cap's word the port keeps: its never-persists mark. The cartridge also leaves the bits of the
    /// doors open around it at the opening, which nothing reads afterwards, so a comparison checks only this mark.
    /// </summary>
    internal const ushort SpentMark = NeverPersists;

    /// <summary>
    /// The hit count the cartridge keeps in a cannon's or the glass's argument word: a typed cannon's count, or the word
    /// the cartridge leaves once a counted hit destroyed it. A smoking cannon was shot down unless it was set up
    /// destroyed, which alone holds the destroying count: the hit that destroys a cannon is not added to its count.
    /// </summary>
    private static int? HitWord(RoomObjectPlace place) => place.Behaviour switch
    {
        DraygonCannonObject cannon => cannon.Phase == DraygonCannonPhase.Smoking && cannon.Hits != DraygonCannonDefinitions.HitsToDestroy
            ? ushort.MaxValue
            : cannon.Hits,
        MotherBrainGlassObject glass => glass.Hits,
        _ => null,
    };

    /// <summary>The native room-argument word of a live place.</summary>
    extension(RoomObjectPlace place)
    {
        /// <summary>The word native work RAM holds for the slot's argument, or null where the port keeps nothing it stands for.</summary>
        internal ushort? NativeRoomArgument => HeldWord(place);
    }

    /// <summary>The native room-argument word of a placement.</summary>
    extension(RoomObjectPlacement placement)
    {
        /// <summary>The word the cartridge stores for the placement's argument.</summary>
        internal ushort NativeRoomArgument => Encode(placement.Kind, placement.Argument, placement.ScrollChange);
    }
}
