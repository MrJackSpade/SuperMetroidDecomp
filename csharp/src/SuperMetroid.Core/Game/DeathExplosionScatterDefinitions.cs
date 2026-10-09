namespace SuperMetroid.Core.Game;

/// <summary>
/// The ten-point small-explosion scatter shared by Ridley's death ($A6:C66E-$A6:C695) and the
/// Baby Metroid's death ($A9:CDFC-$A9:CE23): both banks store the identical interleaved X/Y
/// word pairs, so one owner serves both consumers. The points are an authored scatter pattern
/// with no symmetry, ordering or radial rule (see residualScalarInputsReview).
/// </summary>
internal static class DeathExplosionScatterDefinitions
{
    /// <summary>Number of paired scatter points shared by the two death-explosion sequences.</summary>
    internal const int Count = 10;

    /// <summary>Authored horizontal pixel offsets, paired by index with <see cref="YOffsets"/>.</summary>
    private static ReadOnlySpan<short> XOffsets => [-24, -20, 16, 30, 14, -2, -2, -31, -4, 19];

    /// <summary>Authored vertical pixel offsets, paired by index with <see cref="XOffsets"/>.</summary>
    private static ReadOnlySpan<short> YOffsets => [-24, 20, -30, -3, -13, 18, -32, 8, -10, 19];

    /// <summary>Returns the paired screen displacement for one authored scatter point.</summary>
    /// <param name="index">Zero-based point index from zero through <see cref="Count"/> minus one.</param>
    /// <returns>The horizontal and vertical pixel offsets for the selected explosion.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index does not identify one of the authored points.</exception>
    internal static (short X, short Y) Offset(int index) => (uint)index < Count
        ? (XOffsets[index], YOffsets[index])
        : throw new ArgumentOutOfRangeException(nameof(index));
}
