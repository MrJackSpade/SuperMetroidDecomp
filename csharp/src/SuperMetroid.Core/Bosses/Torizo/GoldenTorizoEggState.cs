using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What one of Golden Torizo's eggs keeps: which way it was laid and rolls, and how long until it hatches.</summary>
/// <param name="laidFacing">The way the egg starts rolling, as Golden Torizo faced when it laid it.</param>
internal sealed class GoldenTorizoEggState(HorizontalDirection laidFacing) : EnemyProjectileState
{
    /// <summary>
    /// The way Golden Torizo faced when it laid the egg: the egg rolls showing that side, and hatches showing it
    /// whichever way it has turned since.
    /// </summary>
    internal HorizontalDirection LaidFacing { get; } = laidFacing;

    /// <summary>The way the egg faces; it turns about when it rolls into a wall, and hatches and charges that way.</summary>
    internal HorizontalDirection Facing { get; set; } = laidFacing;

    /// <summary>The countdown to hatching; the egg hatches on the update it runs out (#2233).</summary>
    internal BossCountdown HatchTimer { get; } = new() { Count = TorizoMotionDefinitions.EggHatchTimer };
}
