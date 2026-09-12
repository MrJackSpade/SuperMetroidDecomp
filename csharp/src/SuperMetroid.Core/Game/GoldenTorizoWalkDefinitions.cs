namespace SuperMetroid.Core.Game;

/// <summary>Golden Torizo's instruction-owned whole-pixel walking displacements.</summary>
public static class GoldenTorizoWalkDefinitions
{
    /// <summary>$AA:D59A, walking instruction velocities: twenty signed words in left/right groups.</summary>
    private static ReadOnlySpan<short> Velocities => [-5,0,-5,-19,-16,-7,0,-7,-17,-18,5,0,5,19,16,7,0,7,17,18];

    /// <summary>Preserves complete byte-indexed word windows; normal instruction operands are even.</summary>
    public static ushort Velocity(ushort byteOffset)
    {
        if (byteOffset > 38) throw new ArgumentOutOfRangeException(nameof(byteOffset));
        ushort value = unchecked((ushort)Velocities[byteOffset >> 1]);
        return (byteOffset & 1) == 0 ? value : (ushort)((value >> 8) | (unchecked((ushort)Velocities[(byteOffset >> 1) + 1]) << 8));
    }
}
