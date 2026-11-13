using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
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
        RoomObjectKind.SpeedBoosterEscape, RoomObjectKind.WreckedShipAttic,
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
        if (CollectibleObjects.TryIdentifyPermanentCollectible(header, out _, out _))
            return new CollectedItemArgument(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Item(word));
        if (name.StartsWith("GreyDoor", StringComparison.Ordinal) || header == RoomObjectKind.BombTorizoGreyDoor)
        {
            return new GreyDoorArgument((word >> GreyConditionShift) & GreyConditionMask,
                new DoorBitArgument(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(word & GreyDoorBitMask), Persistence(word)));
        }
        if (name.StartsWith("RedDoor", StringComparison.Ordinal) || name.StartsWith("GreenDoor", StringComparison.Ordinal) ||
            name.StartsWith("YellowDoor", StringComparison.Ordinal) || name.StartsWith("EyeDoor", StringComparison.Ordinal))
        {
            return new DoorBitArgument(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(word & ~NeverPersists), Persistence(word));
        }
        if (NumbersUnread(header))
            return null;
        if (header == RoomObjectKind.SaveStation)
        {
            return new SaveStationArgument(SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Decode(
                area ?? throw new InvalidDataException($"A save station's argument ${word:X4} names its station only in a room's area."),
                word, "A save station's room argument"));
        }
        if (header == RoomObjectKind.MetroidRoomClearance)
        {
            return (word & 1) == 0
                ? new MetroidRoomArgument(word / 2)
                : throw new InvalidDataException($"Metroids-cleared argument ${word:X4} is odd.");
        }
        if (header == RoomObjectKind.DownwardGateShotBlock)
        {
            return (word & 1) == 0
                ? new GateTriggerArgument(word / 2)
                : throw new InvalidDataException($"Downward gate trigger argument ${word:X4} is odd.");
        }
        throw new InvalidDataException($"PLM {header} has no decoded room argument for ${word:X4}.");
    }

    /// <summary>
    /// The native word of a placement's argument: its typed argument's word, the scroll program's place for a scroll
    /// trigger, the fixed or filler word of a placement that carries nothing, else zero.
    /// </summary>
    internal static ushort Encode(RoomObjectKind header, RoomObjectRoomArgument? argument, RoomScrollChange? scroll) =>
        ArgumentWord(argument) ?? (header == RoomObjectKind.ScrollTrigger && scroll is { } program ? RoomScrollProgramNative.Addresses[program]
            : FixedWords.TryGetValue(header, out ushort fixedWord) ? fixedWord
            : FillerHeaders.Contains(header) || header == RoomObjectKind.ScrollTrigger ? Filler
            : (ushort)0);

    /// <summary>
    /// The native word of a live object's argument (#1691): the hit count for a cannon or the glass once set up, else its
    /// typed argument's word, else the scroll program's place for a scroll trigger. A live object that holds none of these
    /// has no word here: the cartridge's is its placement's filler or its spawner's, which names no state the port keeps.
    /// </summary>
    private static ushort? HeldWord(RoomObjectSlot slot) =>
        HitWord(slot) is { } hits ? unchecked((ushort)hits)
        : ArgumentWord(slot.Argument)
            ?? ((slot.Behaviour as ScrollTriggerObject)?.Change is { } program ? (ushort?)RoomScrollProgramNative.Addresses[program] : null);

    /// <summary>The native word of a typed argument, or null for none.</summary>
    private static ushort? ArgumentWord(RoomObjectRoomArgument? argument) =>
        argument switch
        {
            CollectedItemArgument item => checked((ushort)SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(item.Location)),
            DoorBitArgument door => checked((ushort)(SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(door.Door) | (door.Persists ? 0 : NeverPersists))),
            GreyDoorArgument grey => checked((ushort)((grey.Door.Persists ? 0 : NeverPersists) |
                (grey.ConditionIndex << GreyConditionShift) | SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(grey.Door.Door))),
            SpentDoorArgument => SpentMark,
            SaveStationArgument saved => checked((ushort)SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Number(saved.Station)),
            MetroidRoomArgument metroids => checked((ushort)(metroids.Entry * 2)),
            GateTriggerArgument gate => checked((ushort)(gate.Trigger * 2)),
            null => null,
            _ => throw new InvalidOperationException($"Undefined room argument {argument}."),
        };

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
    private static int? HitWord(RoomObjectSlot slot) => slot.Behaviour switch
    {
        DraygonCannonObject cannon => cannon.Phase == DraygonCannonPhase.Smoking && cannon.Hits != DraygonCannonDefinitions.HitsToDestroy
            ? ushort.MaxValue
            : cannon.Hits,
        MotherBrainGlassObject glass => glass.Hits,
        _ => null,
    };

    /// <summary>The native room-argument word of a live slot.</summary>
    extension(RoomObjectSlot slot)
    {
        /// <summary>The word native work RAM holds for the slot's argument, or null where the port keeps nothing it stands for.</summary>
        internal ushort? NativeRoomArgument => HeldWord(slot);
    }

    /// <summary>The native room-argument word of a placement.</summary>
    extension(RoomObjectPlacement placement)
    {
        /// <summary>The word the cartridge stores for the placement's argument.</summary>
        internal ushort NativeRoomArgument => Encode(placement.Kind, placement.Argument, placement.CompiledScrollChange);
    }
}
