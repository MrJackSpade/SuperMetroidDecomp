namespace SuperMetroid.Core.Fx;

/// <summary>Cartridge values and helpers of <see cref="RoomPaletteFxSystem"/> that only verification reads.</summary>
internal static class RoomPaletteFxSystemConstants
{
    /// <summary>
    /// The CGRAM colour a native SetColorIndex byte index selects; an odd byte index names no colour. The typed
    /// programs already carry colour indices; this decodes a byte index read from a native snapshot.
    /// </summary>
    internal static int PaletteFxColorIndex(ushort byteIndex, ushort objectId) => (byteIndex & 1) == 0
        ? byteIndex / sizeof(ushort)
        : throw new InvalidDataException($"Palette-FX object $8D:{objectId:X4} selected odd CGRAM byte index ${byteIndex:X4}.");
}
