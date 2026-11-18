namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The ten-point small-explosion scatter shared by Ridley's death and the Baby Metroid's death, which scatter their
/// explosions over the same points. The points are an authored scatter pattern with no symmetry or radial rule.
/// </summary>
internal static class DeathExplosionScatterDefinitions
{
    /// <summary>
    /// Signed pixel offsets from each explosion centre, in the order a death bursts its explosions at them: its first at
    /// the first, then each in turn, the first again after the last.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] Offsets =
    [
        new(-20, 20), new(16, -30), new(30, -3), new(14, -13), new(-2, 18),
        new(-2, -32), new(-31, 8), new(-4, -10), new(19, 19), new(-24, -24),
    ];
}
