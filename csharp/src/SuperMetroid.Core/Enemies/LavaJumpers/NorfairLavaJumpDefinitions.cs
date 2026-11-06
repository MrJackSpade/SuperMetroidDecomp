namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>Compiled launch velocities for the Norfair lava-jumping enemy (Squeept).</summary>
internal static class NorfairLavaJumpDefinitions
{
    /// <summary>The four signed 8.8 upward launch velocities, from the highest jump to the lowest.</summary>
    internal static readonly SignedEightEight[] InitialVerticalVelocities =
    [
        SignedEightEight.FromSignedRaw(-2049),
        SignedEightEight.FromSignedRaw(-1794),
        SignedEightEight.FromSignedRaw(-1601),
        SignedEightEight.FromSignedRaw(-1281),
    ];

    /// <summary>The random bits 9 and 10 that select which of the four launch velocities a jump uses.</summary>
    internal static readonly RandomSampleField LaunchSelector = new(9, 2);
}
