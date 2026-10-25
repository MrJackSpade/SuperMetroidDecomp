using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Golden Torizo's instruction-owned whole-pixel walking displacements.</summary>
public static class GoldenTorizoWalkDefinitions
{
    /// <summary>$AA:D59A, walking instruction velocities: twenty signed words in left/right groups.</summary>
    private static short Displacement(int index) =>
        (short)((index < 10 ? -1 : 1) * GoldenTorizoStrideGeometryDefinitions.HorizontalAdvance(index % 10));

    /// <summary>Preserves complete byte-indexed word windows; normal instruction operands are even.</summary>
    public static ushort Velocity(int byteOffset)
    {
        if (byteOffset is < 0 or > 38) throw new ArgumentOutOfRangeException(nameof(byteOffset));
        int value = unchecked((ushort)Displacement(byteOffset >> 1));
        return (byteOffset & 1) == 0 ? (ushort)value : SnesLittleEndian.Word(SnesLittleEndian.HighByte(value), unchecked((byte)Displacement((byteOffset >> 1) + 1)));
    }
}
