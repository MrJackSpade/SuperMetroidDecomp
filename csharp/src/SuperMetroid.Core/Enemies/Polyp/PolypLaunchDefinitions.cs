namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Fixed launch/cooldown mechanics selected by three independent RNG draws.</summary>
public static class PolypLaunchDefinitions
{
    /// <summary>The RNG bits 1..3 that select the cooldown; bit zero and the upper bits do not select a record.</summary>
    internal static readonly RandomSampleField CooldownSelector = new(1, 3);

    /// <summary>The RNG bits 1..4 that select the launch speed and the horizontal velocity.</summary>
    internal static readonly RandomSampleField LaunchSelector = new(1, 4);

    /// <summary>The cooldown each RNG selector chooses: sixteen frames, eight longer per selector.</summary>
    internal static readonly int[] Cooldowns = [16, 24, 32, 40, 48, 56, 64, 72];

    /// <summary>The quadratic speed index the first launch selector chooses; each following selector chooses the next.</summary>
    internal const int FirstInitialYIndex = 28;

    /// <summary>
    /// The horizontal velocity each launch selector chooses: the first eight push right, the second eight push left by
    /// the same magnitudes, each a sixteenth of a pixel per update faster than the last from 96 256ths.
    /// </summary>
    internal static readonly SignedEightEight[] XVelocities =
    [
        SignedEightEight.FromParts(0, 96), SignedEightEight.FromParts(0, 112), SignedEightEight.FromParts(0, 128), SignedEightEight.FromParts(0, 144),
        SignedEightEight.FromParts(0, 160), SignedEightEight.FromParts(0, 176), SignedEightEight.FromParts(0, 192), SignedEightEight.FromParts(0, 208),
        -SignedEightEight.FromParts(0, 96), -SignedEightEight.FromParts(0, 112), -SignedEightEight.FromParts(0, 128), -SignedEightEight.FromParts(0, 144),
        -SignedEightEight.FromParts(0, 160), -SignedEightEight.FromParts(0, 176), -SignedEightEight.FromParts(0, 192), -SignedEightEight.FromParts(0, 208),
    ];
}
