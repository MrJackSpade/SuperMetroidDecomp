using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The shape of Crocomire's mouth volley: each shot's gradient and the speed its angle is scaled to.</summary>
public static class CrocomireProjectileDefinitions
{
    /// <summary>The factor each signed component of a shot's angle is scaled by (the shot's initializer shifts it left twice).</summary>
    public const int VelocityMultiplier = 4;

    /// <summary>The gradients the volley's first nine shots cycle through, three at a time.</summary>
    public static ImmutableArray<short> GradientCycle { get; } = [-16, 0, 32];

    /// <summary>
    /// The shots the authored gradient table covers. The volley's last shot reads the gradient after them, which the
    /// installed <see cref="CrocomireTenthShotGradient"/> holds (#1500).
    /// </summary>
    public const int AuthoredShotCount = 9;
}
