namespace SuperMetroid.Core.Game;

/// <summary>
/// The ten-point small-explosion scatter shared by Ridley's death ($A6:C66E-$A6:C695) and the
/// Baby Metroid's death ($A9:CDFC-$A9:CE23): both banks store the identical interleaved X/Y
/// word pairs, so one owner serves both consumers. The points are an authored scatter pattern
/// with no symmetry, ordering or radial rule (see residualScalarInputsReview).
/// </summary>
internal static class DeathExplosionScatterDefinitions
{
    internal const int Count = 10;

    private static ReadOnlySpan<short> XOffsets => [-24, -20, 16, 30, 14, -2, -2, -31, -4, 19];
    private static ReadOnlySpan<short> YOffsets => [-24, 20, -30, -3, -13, 18, -32, 8, -10, 19];

    internal static (short X, short Y) Offset(int index) => (uint)index < Count
        ? (XOffsets[index], YOffsets[index])
        : throw new ArgumentOutOfRangeException(nameof(index));
}
