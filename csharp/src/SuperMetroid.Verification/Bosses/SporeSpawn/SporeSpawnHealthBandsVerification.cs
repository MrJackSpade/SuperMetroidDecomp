using SuperMetroid.Core.Bosses.SporeSpawn;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>
/// #2019, #2242: Spore Spawn's health bands keep the colours his body shows and the health below which a hit speeds up his
/// swing at the bounds his shot reaction tested inline before they were named.
/// </summary>
internal static class SporeSpawnHealthBandsVerification
{
    /// <summary>
    /// Each band starts exactly at its bound and the band before ends one health above it: 770 and more is the unhurt
    /// band, 410 through 769 the wounded band, 70 through 409 the badly wounded band and below 70 the nearly-dead band; a
    /// hit below 400 health speeds up his swing and one at 400 does not.
    /// </summary>
    internal static void VerifySporeSpawnHealthBands()
    {
        SporeSpawnHealthBand unhurt = SporeSpawnHealthBandDefinitions.Unhurt, wounded = SporeSpawnHealthBandDefinitions.Wounded,
            badly = SporeSpawnHealthBandDefinitions.BadlyWounded, nearlyDead = SporeSpawnHealthBandDefinitions.NearlyDead;
        foreach ((int health, SporeSpawnHealthBand band) in new[] { (960, unhurt), (770, unhurt), (769, wounded), (410, wounded),
            (409, badly), (70, badly), (69, nearlyDead), (0, nearlyDead) })
            AssertEqual(band, SporeSpawnHealthBand.Of(health), $"Spore Spawn at {health} health is in health band {band}");
        foreach ((int health, bool fast) in new[] { (401, false), (400, false), (399, true), (0, true) })
            AssertEqual(fast, SporeSpawnHealthBands.SwingsFast(health), $"a hit leaving Spore Spawn at {health} health speeds his swing: {fast}");
        Console.WriteLine("  Spore Spawn health bands: the four health bands and the fast swing start at their bounds.");
    }

    /// <summary>Runs the Spore Spawn health band check.</summary>
    [VerificationCommand("--spore-spawn-health-bands")]
    internal static int SporeSpawnHealthBandsCommand()
    {
        Suite(nameof(VerifySporeSpawnHealthBands), () => VerifySporeSpawnHealthBands());
        return 0;
    }
}
