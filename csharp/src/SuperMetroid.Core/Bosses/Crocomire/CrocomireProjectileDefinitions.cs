using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The shape of Crocomire's mouth volley: each shot's gradient and the speed its angle is scaled to.</summary>
public static class CrocomireProjectileDefinitions
{
    /// <summary>The factor each signed component of a shot's angle is scaled by (the shot's setup quadruples it).</summary>
    public const int VelocityMultiplier = 4;

    /// <summary>
    /// The gradients the volley's shots are fired along, in firing order, every shot but the last (#2303); the last shot is
    /// fired along the installed <see cref="CrocomireTenthShotGradient"/> (#1500).
    /// </summary>
    public static ImmutableArray<int> AuthoredGradients { get; } = [0, 32, -16, 0, 32, -16, 0, 32];

    /// <summary>The shots the projectile attack spits before Crocomire steps forward: the authored ones and the last.</summary>
    public const int VolleyShotCount = 9;
}
