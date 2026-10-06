using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Golden Torizo's instruction-owned whole-pixel walking displacements.</summary>
public static class GoldenTorizoWalkDefinitions
{
    /// <summary>$AA:D59A, walking instruction velocities: twenty signed words in left/right groups.</summary>
    private static short Displacement(int index) =>
        (short)((index < 10 ? -1 : 1) * GoldenTorizoStrideGeometryDefinitions.HorizontalAdvance(index % 10));

    /// <summary>Preserves complete byte-indexed word windows; normal instruction operands are even.</summary>
    public static ushort Velocity(ushort byteOffset)
    {
        if (byteOffset > 38) throw new ArgumentOutOfRangeException(nameof(byteOffset));
        ushort value = unchecked((ushort)Displacement(byteOffset >> 1));
        return (byteOffset & 1) == 0 ? value : (ushort)((value >> 8) | (unchecked((ushort)Displacement((byteOffset >> 1) + 1)) << 8));
    }
}
