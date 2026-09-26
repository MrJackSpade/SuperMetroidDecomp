namespace SuperMetroid.Core.Game;

/// <summary>One immutable bank-$83 room-FX record, with its native field identities preserved.</summary>
public sealed record RoomFxRecordDefinition(
    ushort Pointer,
    ushort DoorPointer,
    ushort BaseYPosition,
    ushort TargetYPosition,
    ushort PackedYVelocity,
    byte Timer,
    byte Type,
    byte DefaultLayerBlend,
    byte Layer3LayerBlend,
    byte LiquidOptions,
    byte PaletteFxBitset,
    byte AnimatedTileBitset,
    byte PaletteBlend)
{
    /// <summary>Exposes one native byte for consumers that use offset-based FX fields.</summary>
    public byte ReadByte(int offset) => offset switch
    {
        0 => (byte)DoorPointer,
        1 => (byte)(DoorPointer >> 8),
        2 => (byte)BaseYPosition,
        3 => (byte)(BaseYPosition >> 8),
        4 => (byte)TargetYPosition,
        5 => (byte)(TargetYPosition >> 8),
        6 => (byte)PackedYVelocity,
        7 => (byte)(PackedYVelocity >> 8),
        8 => Timer,
        9 => Type,
        10 => DefaultLayerBlend,
        11 => Layer3LayerBlend,
        12 => LiquidOptions,
        13 => PaletteFxBitset,
        14 => AnimatedTileBitset,
        15 => PaletteBlend,
        _ => throw new ArgumentOutOfRangeException(nameof(offset)),
    };

    public ushort ReadWord(int offset)
    {
        if ((uint)offset > RoomFxRomData.Record.ByteCount - sizeof(ushort))
            throw new ArgumentOutOfRangeException(nameof(offset));
        return (ushort)(ReadByte(offset) | ReadByte(offset + 1) << 8);
    }
}

/// <summary>Compiled retail room-FX records selected by all known room states.</summary>
public static partial class RoomFxRecordDefinitions
{
    // Partial-file static field order is unspecified; defer the index until generated is ready.
    private static readonly Lazy<IReadOnlyDictionary<ushort, RoomFxRecordDefinition>> byPointer =
        new(BuildIndex);

    public static IReadOnlyList<RoomFxRecordDefinition> All => generated;

    public static RoomFxRecordDefinition Get(ushort pointer) =>
        byPointer.Value.TryGetValue(pointer, out RoomFxRecordDefinition? record)
            ? record
            : throw new InvalidDataException($"No compiled room-FX record at $83:{pointer:X4}.");

    /// <summary>Replays the native first-default-or-matching-door walk over typed records.</summary>
    public static ushort Select(ushort fxPointer, ushort doorPointer)
    {
        if (fxPointer == 0) return 0;
        ushort pointer = fxPointer;
        for (int guard = 0; guard < 256; guard++)
        {
            ushort candidate = Get(pointer).DoorPointer;
            if (candidate == 0 || candidate == doorPointer) return pointer;
            if (candidate == RoomFxRomData.Record.TerminatorDoorPointer) return 0;
            pointer = unchecked((ushort)(pointer + RoomFxRomData.Record.ByteCount));
        }
        throw new InvalidDataException(
            $"Compiled room-FX list $83:{fxPointer:X4} did not terminate for door $83:{doorPointer:X4}.");
    }

    private static IReadOnlyDictionary<ushort, RoomFxRecordDefinition> BuildIndex()
    {
        var result = new Dictionary<ushort, RoomFxRecordDefinition>(generated.Length);
        ushort previous = 0;
        foreach (RoomFxRecordDefinition record in generated)
        {
            if (record.Pointer <= previous || !result.TryAdd(record.Pointer, record))
                throw new InvalidDataException("Compiled room-FX record pointers must be unique and sorted.");
            previous = record.Pointer;
        }
        return result;
    }
}
