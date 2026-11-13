using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The shape of Crocomire's mouth volley: each shot's gradient and the speed its angle is scaled to.</summary>
public static class CrocomireProjectileDefinitions
{
    /// <summary>The factor each signed component of a shot's angle is scaled by (the shot's initializer quadruples it).</summary>
    public const int VelocityMultiplier = 4;

    /// <summary>The gradients the volley's first nine shots cycle through, three at a time.</summary>
    public static ImmutableArray<short> GradientCycle { get; } = [-16, 0, 32];

    /// <summary>
    /// The shots the projectile attack spits before Crocomire steps forward. Each reads the gradient its place in the
    /// volley names from the authored ones, which name nine places counting from none, so the last shot reads the
    /// gradient after them, which the installed <see cref="CrocomireTenthShotGradient"/> holds (#1500).
    /// </summary>
    public const int VolleyShotCount = 9;
}
