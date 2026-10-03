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
    /// <summary>Enumerates every selected record in ascending native identity order without a stored cache.</summary>
    public static IEnumerable<RoomFxRecordDefinition> All
    {
        get
        {
            for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
                if (SelectRecord((ushort)pointer) is { } record)
                    yield return record;
        }
    }

    /// <summary>Directly selects the room-FX configuration for one original record identity.</summary>
    /// <remarks>All 295 selected identities preserve the native four words/eight bytes.
    /// Terminator identities preserve the existing canonical view: door FFFF and
    /// zero payload, rather than exposing adjacent native records. Byte and odd-word
    /// views preserve that same representation. Unknown/interior addresses reject.
    /// Each identity selects a room/entrance configuration: liquid positions and velocity,
    /// timer, effect kind, layer blend controls and enabled palette/animation effects.
    /// These are semantic configuration cases, not samples of a numerical curve.
    /// Field layout and original values are independently verified against supported
    /// NTSC J/U v1.0 bank83 and pinned bank_83.asm (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// No record array or generated dictionary is retained.</remarks>
    public static RoomFxRecordDefinition Get(ushort pointer) => SelectRecord(pointer) ??
        throw new InvalidDataException($"No compiled room-FX record at $83:{pointer:X4}.");

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

}
