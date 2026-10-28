using System.Buffers.Binary;
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
        if ((byteOffset & 1) == 0)
            return (ushort)value;
        // An odd offset reads the little-endian word straddling two table words.
        Span<byte> table = stackalloc byte[2 * sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(table, (ushort)value);
        BinaryPrimitives.WriteUInt16LittleEndian(table[sizeof(ushort)..], unchecked((ushort)Displacement((byteOffset >> 1) + 1)));
        return BinaryPrimitives.ReadUInt16LittleEndian(table[1..]);
    }
}
