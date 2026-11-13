namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>Compiled launch velocities for the Norfair lava-jumping enemy (Squeept).</summary>
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

    /// <summary>The random bits 9 and 10 that select which of the four launch velocities a jump uses.</summary>
    internal static readonly RandomSampleField LaunchSelector = new(9, 2);
}
