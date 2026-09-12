namespace SuperMetroid.Core.Game;

/// <summary>One native hop's fixed mechanics; the delta also serves as packed 8.8 dropping speed.</summary>
public readonly record struct PuyoHopDefinition(ushort Height, ushort XSpeed, ushort YIndexDelta, PuyoAirborneFunction Function);

/// <summary>Seven immutable NTSC Puyo hop definitions, independent of animation artwork.</summary>
public static class PuyoHopDefinitions
{
    /// <summary>$A2:9A07, PuyoHopTable: height, X speed, Y index delta and airborne callback.</summary>
    public const int ReferenceAddress = 0xa29a07;
    /// <summary>Native table stride for the serialized byte-index selector.</summary>
    public const int RecordSize = 8;
    /// <summary>Native authored record count, including unused long hop.</summary>
    public const int Count = 7;

    /// <summary>Preserves native byte-index state while returning named fields.</summary>
    public static PuyoHopDefinition FromByteIndex(ushort index) => index switch
    {
        0 => new(0x10, 0x100, 0x200, PuyoAirborneFunction.NormalShortHop),
        8 => new(0x20, 0x100, 0x200, PuyoAirborneFunction.NormalBigHop),
        16 => new(0x20, 0x200, 0x300, PuyoAirborneFunction.NormalLongHop),
        24 => new(0x80, 0x140, 0x200, PuyoAirborneFunction.GiantHop),
        32 => new(0, 0, 0x100, PuyoAirborneFunction.Dropping),
        40 => new(0x10, 0x100, 0x1c0, PuyoAirborneFunction.Dropped),
        48 => new(0x15, 0x100, 0x1c0, PuyoAirborneFunction.Dropped),
        _ => throw new InvalidDataException($"Puyo hop byte index ${index:X4} is outside seven authored records."),
    };
}
