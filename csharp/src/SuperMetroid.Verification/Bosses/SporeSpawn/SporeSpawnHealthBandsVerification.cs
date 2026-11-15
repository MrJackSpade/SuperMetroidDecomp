using SuperMetroid.Core.Bosses.SporeSpawn;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>
/// #2019: Spore Spawn's health bands keep the health stages his body shows and the health below which a hit speeds up his
/// swing at the bounds his shot reaction tested inline before they were named.
/// </summary>
internal static class SporeSpawnHealthBandsVerification
{
    /// <summary>
    /// Each stage starts exactly at its bound and the stage before ends one health above it: 770 and more is the unhurt
    /// stage, 410 through 769 the first hurt stage, 70 through 409 the second and below 70 the last; a hit below 400 health
    /// speeds up his swing and one at 400 does not.
    /// </summary>
    internal static void VerifySporeSpawnHealthBands()
    {
        foreach ((int health, int stage) in new[] { (960, 0), (770, 0), (769, 1), (410, 1), (409, 2), (70, 2), (69, 3), (0, 3) })
            AssertEqual(stage, SporeSpawnHealthBands.Stage(health), $"Spore Spawn at {health} health shows health stage {stage}");
        foreach ((int health, bool fast) in new[] { (401, false), (400, false), (399, true), (0, true) })
            AssertEqual(fast, SporeSpawnHealthBands.SwingsFast(health), $"a hit leaving Spore Spawn at {health} health speeds his swing: {fast}");
        Console.WriteLine("  Spore Spawn health bands: the four health stages and the fast swing start at their bounds.");
    }

    /// <summary>Runs the Spore Spawn health band check.</summary>
    [VerificationCommand("--spore-spawn-health-bands")]
    internal static int SporeSpawnHealthBandsCommand()
    {
        Suite(nameof(VerifySporeSpawnHealthBands), () => VerifySporeSpawnHealthBands());
        return 0;
    }
}
