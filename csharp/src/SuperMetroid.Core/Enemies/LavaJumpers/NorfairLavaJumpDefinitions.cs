namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>The launch velocities, gravity and animation switch of the Norfair lava-jumping enemy (Squeept).</summary>
internal static class NorfairLavaJumpDefinitions
{
    /// <summary>
    /// The upward velocity a jump launches with, chosen from four rolls of its random number (#2510): the four launch
    /// velocities in turn, from the highest jump to the lowest.
    /// </summary>
    internal static readonly WeightedRandomChoice<SignedEightEight> Launch = WeightedRandomChoice<SignedEightEight>.InTurn(
        EnemyRandomRolls.NorfairLavaJumpLaunch,
        -SignedEightEight.FromPixelFraction(2049, 256),
        -SignedEightEight.FromPixelFraction(897, 128),
        -SignedEightEight.FromPixelFraction(1601, 256),
        -SignedEightEight.FromPixelFraction(1281, 256));

    /// <summary>How much the jumper's vertical velocity grows downward after each moving update (#2339).</summary>
    internal static SignedEightEight Gravity { get; } = SignedEightEight.FromPixelFraction(7, 32);

    /// <summary>
    /// The rising velocity, four pixels an update upward, at and above which a still-rising jumper switches to its visible
    /// jump animation (#2339).
    /// </summary>
    internal static SignedEightEight AnimationSwitchVelocity { get; } = -SignedEightEight.FromPixels(4);
}
