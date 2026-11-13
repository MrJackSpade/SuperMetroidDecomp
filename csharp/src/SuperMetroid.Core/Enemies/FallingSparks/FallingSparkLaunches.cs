using static SuperMetroid.Core.Enemies.FallingSparks.FallingSparkLaunchDefinitions;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Chooses a falling spark's horizontal launch from the random number.</summary>
internal static class FallingSparkLaunches
{
    /// <summary>
    /// The launch the random number divided by 4, modulo 8, selects: one of the authored launches, or, for the selection past them,
    /// the drift <paramref name="unauthoredLaunch"/> supplies, which is consulted only then.
    /// </summary>
    /// <param name="random">The random number the spark takes.</param>
    /// <param name="unauthoredLaunch">Supplies the drift of the selection past the authored launches.</param>
    /// <exception cref="InvalidDataException">The selection lies further past the authored launches than one record.</exception>
    internal static SignedSixteenSixteen FromRandom(int random, Func<FallingSparkUnauthoredLaunch> unauthoredLaunch)
    {
        int selection = LaunchSelector.Of(random);
        if (selection < Launches.Length)
            return Launches[selection];
        return selection == Launches.Length
            ? Ensure.NotNull(unauthoredLaunch)().Drift
            : throw new InvalidDataException(
                $"Falling spark launch selection {selection} lies more than one record past the {Launches.Length} authored launches.");
    }
}
