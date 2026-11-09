using SuperMetroid.Core.Bosses.Draygon;
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
    private static readonly Dictionary<RoomObjectHeaderId, ushort> FixedWords = new Dictionary<RoomObjectHeaderId, ushort>
    {
        [RoomObjectHeaderId.NoobTube] = 0x0080,
        [RoomObjectHeaderId.BombTorizoHand] = 0x0000,
        [RoomObjectHeaderId.DraygonCannonFacingLeft] = 0x0000,
        [RoomObjectHeaderId.DraygonCannonFacingRight] = 0x0000,
        [RoomObjectHeaderId.DraygonCannonFacingRightDestroyed] = 0x0000,
    };

    /// <summary>Headers whose retail placements carry the filler word.</summary>
    private static readonly HashSet<RoomObjectHeaderId> FillerHeaders =
    [
        RoomObjectHeaderId.DownwardGate, RoomObjectHeaderId.RightwardsScrollExtension, RoomObjectHeaderId.LeftwardsScrollExtension,
        RoomObjectHeaderId.DownwardsScrollExtension, RoomObjectHeaderId.UpwardsScrollExtension, RoomObjectHeaderId.ElevatorPlatform,
        RoomObjectHeaderId.MapStation, RoomObjectHeaderId.MotherBrainEscapeRoomGate, RoomObjectHeaderId.MotherBrainGlass,
        RoomObjectHeaderId.SpeedBoosterEscape, RoomObjectHeaderId.WreckedShipAttic,
    ];

    /// <summary>Whether a door word's opening is remembered: its sign bit set means it never is.</summary>
    private static DoorOpeningPersistence Persistence(int word) =>
        (word & NeverPersists) == 0 ? DoorOpeningPersistence.Persistent : DoorOpeningPersistence.Transient;

    /// <summary>Decodes a cartridge placement's word into the argument the import gives the placement.</summary>
    /// <exception cref="InvalidDataException">The word does not decode for the header.</exception>
    internal static RoomObjectRoomArgument? Decode(RoomObjectHeaderId header, int word)
    {
        string name = header.ToString();
        if (header == RoomObjectHeaderId.ScrollTrigger || FixedWords.ContainsKey(header) || FillerHeaders.Contains(header))
            return null;
        if (CollectibleObjects.TryIdentifyPermanentCollectible(header, out _, out _))
            return new CollectedItemArgument(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Item(word));
        if (name.StartsWith("GreyDoor", StringComparison.Ordinal) || header == RoomObjectHeaderId.BombTorizoGreyDoor)
        {
            return new GreyDoorArgument((word >> GreyConditionShift) & GreyConditionMask,
                new DoorBitArgument(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(word & GreyDoorBitMask), Persistence(word)));
        }
        if (name.StartsWith("RedDoor", StringComparison.Ordinal) || name.StartsWith("GreenDoor", StringComparison.Ordinal) ||
            name.StartsWith("YellowDoor", StringComparison.Ordinal) || name.StartsWith("EyeDoor", StringComparison.Ordinal))
        {
            return new DoorBitArgument(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(word & ~NeverPersists), Persistence(word));
        }
        if (header is RoomObjectHeaderId.EnergyStation or RoomObjectHeaderId.MissileStation or RoomObjectHeaderId.SaveStation)
            return new StationArgument(word);
        if (header == RoomObjectHeaderId.SetMetroidsClearedStatesWhenRequired)
        {
            return (word & 1) == 0
                ? new MetroidRoomArgument(word / 2)
                : throw new InvalidDataException($"Metroids-cleared argument ${word:X4} is odd.");
        }
        if (header == RoomObjectHeaderId.DownwardGateShotBlock)
        {
            return (word & 1) == 0
                ? new GateTriggerArgument(word / 2)
                : throw new InvalidDataException($"Downward gate trigger argument ${word:X4} is odd.");
        }
        throw new InvalidDataException($"PLM {header} has no decoded room argument for ${word:X4}.");
    }

    /// <summary>
    /// The native word of an argument: the hit count for a cannon or the glass once set up, the scroll program's place for
    /// a scroll trigger, the fixed or filler word of a placement that carries nothing, else zero.
    /// </summary>
    internal static ushort Encode(RoomObjectHeaderId header, RoomObjectRoomArgument? argument, int? hitCounter, RoomScrollProgram? scroll)
    {
        if (hitCounter is { } hits && (DraygonCannonObjects.IsDraygonCannonHeader(header) || header == RoomObjectHeaderId.MotherBrainGlass))
            return unchecked((ushort)hits);
        return argument switch
        {
            CollectedItemArgument item => checked((ushort)SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(item.Location)),
            DoorBitArgument door => checked((ushort)(SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(door.Door) | (door.Persists ? 0 : NeverPersists))),
            GreyDoorArgument grey => checked((ushort)((grey.Door.Persists ? 0 : NeverPersists) |
                (grey.ConditionIndex << GreyConditionShift) | SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(grey.Door.Door))),
            SpentDoorArgument => SpentMark,
            StationArgument station => checked((ushort)station.Station),
            MetroidRoomArgument metroids => checked((ushort)(metroids.Entry * 2)),
            GateTriggerArgument gate => checked((ushort)(gate.Trigger * 2)),
            null when header == RoomObjectHeaderId.ScrollTrigger && scroll is { } program => RoomScrollProgramNative.Addresses[program],
            null when FixedWords.TryGetValue(header, out ushort fixedWord) => fixedWord,
            null when FillerHeaders.Contains(header) || header == RoomObjectHeaderId.ScrollTrigger => Filler,
            null => 0,
            _ => throw new InvalidOperationException($"Undefined room argument {argument}."),
        };
    }

    /// <summary>
    /// The part of a spent cap's word the port keeps: its never-persists mark. The cartridge also leaves the bits of the
    /// doors open around it at the opening, which nothing reads afterwards, so a comparison checks only this mark.
    /// </summary>
    internal const ushort SpentMark = NeverPersists;

    /// <summary>The native room-argument word of a live slot.</summary>
    extension(RoomObjectSlot slot)
    {
        /// <summary>The word native work RAM holds for the slot's argument.</summary>
        internal ushort NativeRoomArgument => Encode(slot.Header, slot.Argument, slot.HitCounter,
            (slot.Family as ScrollObjects.ScrollObjectState)?.CompiledProgram);
    }

    /// <summary>The native room-argument word of a placement.</summary>
    extension(RoomObjectPlacement placement)
    {
        /// <summary>The word the cartridge stores for the placement's argument.</summary>
        internal ushort NativeRoomArgument => Encode(placement.Header.Header, placement.Argument, null, placement.CompiledScrollProgram);
    }
}
