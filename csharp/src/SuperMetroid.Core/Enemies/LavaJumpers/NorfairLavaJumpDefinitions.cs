namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>Compiled launch velocities, gravity and animation switch of the Norfair lava-jumping enemy (Squeept).</summary>
internal static class NorfairLavaJumpDefinitions
{
    /// <summary>The four upward launch velocities, from the highest jump to the lowest.</summary>
    internal static readonly SignedEightEight[] InitialVerticalVelocities =
    [
        -SignedEightEight.FromParts(8, 1),
        -SignedEightEight.FromParts(7, 2),
        -SignedEightEight.FromParts(6, 65),
        -SignedEightEight.FromParts(5, 1),
    ];

    /// <summary>Which of the four launch velocities a jump uses.</summary>
    internal static readonly RandomSampleField LaunchSelector = new(9, 2);

    /// <summary>How much the jumper's vertical velocity grows downward after each moving update: 56 256ths of a pixel (#2339).</summary>
    internal static SignedEightEight Gravity { get; } = SignedEightEight.FromParts(0, 56);

    /// <summary>
    /// The rising velocity, four pixels an update upward, at and above which a still-rising jumper switches to its visible
    /// jump animation (#2339).
    /// </summary>
    internal static SignedEightEight AnimationSwitchVelocity { get; } = -SignedEightEight.FromParts(4, 0);
}
