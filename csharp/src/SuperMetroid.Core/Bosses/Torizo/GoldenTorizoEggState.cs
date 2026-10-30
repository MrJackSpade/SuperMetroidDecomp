using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What one of Golden Torizo's eggs keeps: which way it rolls and how long until it hatches.</summary>
/// <param name="facingRight">Whether the egg starts rolling right, as Golden Torizo faced when it laid it.</param>
internal sealed class GoldenTorizoEggState(bool facingRight) : EnemyProjectileState
{
    /// <summary>Whether the egg faces right; it turns about when it rolls into a wall, and hatches and charges that way.</summary>
    internal bool FacingRight { get; set; } = facingRight;

    /// <summary>The countdown to hatching; the egg hatches on the update it passes zero.</summary>
    internal WrappingCountdown HatchTimer { get; set; } = WrappingCountdown.Start(TorizoMotionDefinitions.EggHatchTimer);
}
