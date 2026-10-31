using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The shape of Crocomire's mouth volley: each shot's gradient and the speed its angle is scaled to.</summary>
public static class CrocomireProjectileDefinitions
{
    /// <summary>The factor each signed component of a shot's angle is scaled by ($86:909B and $86:90A7 shift it left twice).</summary>
    public const int VelocityMultiplier = 4;

    /// <summary>The gradients the volley's first nine shots cycle through, three at a time.</summary>
    public static ImmutableArray<short> GradientCycle { get; } = [-16, 0, 32];

    /// <summary>The shots the authored gradient table covers.</summary>
    public const int AuthoredShotCount = 9;

    /// <summary>
    /// The tenth shot's gradient: the cartridge reads it one word past the table, from the first two bytes of the
    /// setup instruction that follows ($86:906B), as a signed word.
    /// </summary>
    public const int FinalShotOverreadGradient = -18912;
}
