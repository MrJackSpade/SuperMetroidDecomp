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

    private static readonly Offset16<RoomPixels>[] Offsets =
    [
        new(-24, -24), new(-20, 20), new(16, -30), new(30, -3), new(14, -13),
        new(-2, 18), new(-2, -32), new(-31, 8), new(-4, -10), new(19, 19),
    ];

    internal static Offset16<RoomPixels> Offset(int index) => (uint)index < Count
        ? Offsets[index]
        : throw new ArgumentOutOfRangeException(nameof(index));
}
