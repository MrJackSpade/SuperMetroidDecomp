using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The native room-argument word of a PLM (#1452): Core keeps the typed argument the import decoded, and verification
/// translates between it and the word the cartridge stores and work RAM holds, where it decodes native populations and
/// compares the port's slots with native memory.
/// </summary>
internal static class PlmRoomArgumentWords
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
    private static readonly IReadOnlyDictionary<PlmHeaderId, ushort> FixedWords = new Dictionary<PlmHeaderId, ushort>
    {
        [PlmHeaderId.NoobTube] = 0x0080,
        [PlmHeaderId.BombTorizoHand] = 0x0000,
        [PlmHeaderId.DraygonCannonFacingLeft] = 0x0000,
        [PlmHeaderId.DraygonCannonFacingRight] = 0x0000,
        [PlmHeaderId.DraygonCannonFacingRightDestroyed] = 0x0000,
    };

    /// <summary>Headers whose retail placements carry the filler word.</summary>
    private static readonly HashSet<PlmHeaderId> FillerHeaders =
    [
        PlmHeaderId.DownwardGate, PlmHeaderId.RightwardsScrollExtension, PlmHeaderId.LeftwardsScrollExtension,
        PlmHeaderId.DownwardsScrollExtension, PlmHeaderId.UpwardsScrollExtension, PlmHeaderId.ElevatorPlatform,
        PlmHeaderId.MapStation, PlmHeaderId.MotherBrainEscapeRoomGate, PlmHeaderId.MotherBrainGlass,
        PlmHeaderId.SpeedBoosterEscape, PlmHeaderId.WreckedShipAttic,
    ];

    /// <summary>Decodes a cartridge placement's word into the argument the import gives the placement.</summary>
    /// <exception cref="InvalidDataException">The word does not decode for the header.</exception>
    internal static PlmRoomArgument? Decode(PlmHeaderId header, int word)
    {
        string name = header.ToString();
        if (header == PlmHeaderId.ScrollTrigger || FixedWords.ContainsKey(header) || FillerHeaders.Contains(header))
            return null;
        if (CollectiblePlms.TryIdentifyPermanentCollectible(header, out _, out _))
            return new CollectedItemArgument(word);
        if (name.StartsWith("GreyDoor", StringComparison.Ordinal) || header == PlmHeaderId.BombTorizoGreyDoor)
        {
            return new GreyDoorArgument((word >> GreyConditionShift) & GreyConditionMask,
                new DoorBitArgument(word & GreyDoorBitMask, (word & NeverPersists) == 0));
        }
        if (name.StartsWith("RedDoor", StringComparison.Ordinal) || name.StartsWith("GreenDoor", StringComparison.Ordinal) ||
            name.StartsWith("YellowDoor", StringComparison.Ordinal) || name.StartsWith("EyeDoor", StringComparison.Ordinal))
        {
            return new DoorBitArgument(word & ~NeverPersists, (word & NeverPersists) == 0);
        }
        if (header is PlmHeaderId.EnergyStation or PlmHeaderId.MissileStation or PlmHeaderId.SaveStation)
            return new StationArgument(word);
        if (header == PlmHeaderId.SetMetroidsClearedStatesWhenRequired)
        {
            return (word & 1) == 0
                ? new MetroidRoomArgument(word / 2)
                : throw new InvalidDataException($"Metroids-cleared argument ${word:X4} is odd.");
        }
        if (header == PlmHeaderId.DownwardGateShotBlock)
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
    internal static ushort Encode(PlmHeaderId header, PlmRoomArgument? argument, int? hitCounter, RoomScrollProgram? scroll)
    {
        if (hitCounter is { } hits && (DraygonCannonPlms.IsDraygonCannonHeader(header) || header == PlmHeaderId.MotherBrainGlass))
            return unchecked((ushort)hits);
        return argument switch
        {
            CollectedItemArgument item => checked((ushort)item.ItemBit),
            DoorBitArgument door => checked((ushort)(door.DoorBit | (door.Persists ? 0 : NeverPersists))),
            GreyDoorArgument grey => checked((ushort)((grey.Door.Persists ? 0 : NeverPersists) |
                (grey.ConditionIndex << GreyConditionShift) | grey.Door.DoorBit)),
            SpentDoorArgument spent => unchecked((ushort)(NeverPersists | spent.OpenedDoorWindow)),
            StationArgument station => checked((ushort)station.Station),
            MetroidRoomArgument metroids => checked((ushort)(metroids.Entry * 2)),
            GateTriggerArgument gate => checked((ushort)(gate.Trigger * 2)),
            null when header == PlmHeaderId.ScrollTrigger && scroll is { } program => RoomScrollProgramNative.Addresses[program],
            null when FixedWords.TryGetValue(header, out ushort fixedWord) => fixedWord,
            null when FillerHeaders.Contains(header) || header == PlmHeaderId.ScrollTrigger => Filler,
            null => 0,
            _ => throw new InvalidOperationException($"Undefined room argument {argument}."),
        };
    }

    /// <summary>The native room-argument word of a live slot.</summary>
    extension(PlmSlot slot)
    {
        /// <summary>The word native work RAM holds for the slot's argument.</summary>
        internal ushort NativeRoomArgument => Encode(slot.Header, slot.Argument, slot.HitCounter,
            (slot.Family as ScrollPlms.ScrollPlmState)?.CompiledProgram);
    }

    /// <summary>The native room-argument word of a placement.</summary>
    extension(RoomPlmPlacement placement)
    {
        /// <summary>The word the cartridge stores for the placement's argument.</summary>
        internal ushort NativeRoomArgument => Encode(placement.Header.Header, placement.Argument, null, placement.CompiledScrollProgram);
    }
}
