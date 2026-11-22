namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Where a puff of smoke lands around the point it is spawned at (#2044): a point, or a box centred on it that one
/// random sample scatters the puff across, one part of the sample across the box and another down it.
/// </summary>
internal static partial class SmokePuffPlacements
{
    /// <summary>The puff appears at the point itself: its fields read nothing of the sample, so each has one outcome, zero.</summary>
    internal static DustExplosionPlacement Point { get; } = new(RandomSampleField.Nothing, RandomSampleField.Nothing, default);

    /// <summary>The puff lands anywhere in a centred box 16 pixels wide and 32 tall.</summary>
    internal static DustExplosionPlacement CentredBox { get; } = new(RandomSampleField.OneOf(16), RandomSampleField.SecondOf(32), new(-8, -16));
}
