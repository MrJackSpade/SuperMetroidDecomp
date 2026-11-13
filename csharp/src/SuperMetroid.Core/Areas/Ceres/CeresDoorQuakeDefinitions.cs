namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// How far Ceres Ridley's scene shakes the escape door it draws (<see cref="CeresDoorQuake"/>): one horizontal offset per
/// phase of the earthquake's frame count, as authored, how the door's own earthquake chooses its strength and how its
/// destruction's explosions choose their animation.
/// </summary>
internal static class CeresDoorQuakeDefinitions
{
    /// <summary>
    /// The door's earthquake strength, read from the current sample without drawing (#1619): the lowest 128 of the 4096
    /// rolls of the sample modulo 4096, one in 32, start the stronger quake; the rest the light one.
    /// </summary>
    internal static readonly RandomChance StrongQuake = RandomChance.Lowest(128, new RandomSampleField(0, 12));

    /// <summary>
    /// The animation of each explosion of the door's destruction (#1619): the lowest 16384 of the 65536 samples, a
    /// quarter, play the door's explosion animation; the rest its smoke animation.
    /// </summary>
    internal static readonly RandomChance RumbleExplosion = RandomChance.Lowest(16384, RandomSampleField.Whole);

    /// <summary>The door's horizontal offset in pixels, rightward positive, for each earthquake phase in order.</summary>
    internal static readonly int[] PhaseOffsets = [0, -4, -1, 3];
}
