using static SuperMetroid.Core.Enemies.FallingSparks.FallingSparkLaunchDefinitions;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Chooses a falling spark's horizontal launch from the random number.</summary>
public static class FallingSparkLaunches
{
    /// <summary>The launch random bits two through four select.</summary>
    public static SignedSixteenSixteen FromRandom(int random) => Launches[LaunchSelector.Of(random)];
}
