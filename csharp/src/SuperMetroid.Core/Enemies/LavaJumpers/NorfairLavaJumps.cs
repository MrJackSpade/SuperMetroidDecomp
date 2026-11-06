using static SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumpDefinitions;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>How hard a Norfair lava-jumping enemy (Squeept) launches out of the lava.</summary>
internal static class NorfairLavaJumps
{
    /// <summary>The launch velocity the random sample <paramref name="random"/> selects.</summary>
    /// <param name="random">A random sample, 0 through 65535.</param>
    internal static SignedEightEight InitialVerticalVelocity(int random) =>
        InitialVerticalVelocities[LaunchSelector.Of(random)];
}
