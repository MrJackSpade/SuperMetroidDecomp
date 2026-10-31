using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the test-support codec between the typed enemy properties and their native words (#1472): every engine flag
/// crosses it unchanged in both directions, a word whose low byte holds species settings is not an engine property
/// word, and a kill's power-bomb radius follows the respawn flag.
/// </summary>
internal static class EnemyPropertyWordCodecVerification
{
    /// <summary>Round-trips every engine and additional property, and checks the power-bomb radius after a kill.</summary>
    internal static void VerifyEnemyPropertyWordCodec()
    {
        EnemyProperties[] flags =
        [
            EnemyProperties.Invisible, EnemyProperties.Deleted, EnemyProperties.IgnoreSamusCollision,
            EnemyProperties.ProcessOffScreen, EnemyProperties.BlocksPlasmaBeam, EnemyProperties.ProcessInstructions,
            EnemyProperties.RespawnIfKilled, EnemyProperties.SolidToSamus,
        ];
        foreach (EnemyProperties flag in flags)
        {
            AssertEqual(flag, EnemyPropertyWordCodec.Decode(EnemyPropertyWordCodec.Encode(flag)), $"property {flag} round trips");
            AssertEqual(flag, flag.With(EnemyProperties.Deleted).Without(EnemyProperties.Deleted).With(flag), $"flag operations keep {flag}");
        }
        for (int bit = 0; bit < 8; bit++)
        {
            int word = 1 << bit;
            AssertThrows<InvalidDataException>(() => EnemyPropertyWordCodec.Decode(word), $"property bit {bit} holds species settings");
            AssertEqual(word, EnemyPropertyWordCodec.SpeciesBitsOf(word), $"property bit {bit} is a species bit");
        }

        foreach (EnemyExtraProperties flag in new[] { EnemyExtraProperties.UsesExtendedSpritemap, EnemyExtraProperties.NewInstructionFrame })
            AssertEqual(flag, EnemyPropertyWordCodec.DecodeExtra(EnemyPropertyWordCodec.Encode(flag)), $"additional property {flag} round trips");

        // A kill inside a power bomb's sweep leaves the sweep the radius the respawn flag names, applied once; a sweep
        // that starts forgets earlier kills.
        var sweep = new PowerBombRadiusAfterKill();
        AssertEqual(64, sweep.HorizontalRadius(64), "with no kill the sweep keeps its radius");
        sweep.RecordKill(killedEnemyRespawns: true);
        AssertEqual(16384, sweep.HorizontalRadius(64), "a respawning death leaves radius 16384");
        AssertEqual(16384, sweep.HorizontalRadius(16384), "the kill is applied once");
        sweep.RecordKill(killedEnemyRespawns: false);
        AssertEqual(0, sweep.HorizontalRadius(16384), "an ordinary death leaves radius zero");
        sweep.RecordKill(killedEnemyRespawns: false);
        sweep.Forget();
        AssertEqual(64, sweep.HorizontalRadius(64), "a sweep forgets kills made before it starts");

        Console.WriteLine(
            "Enemy property word codec: every engine and additional property round trips; a kill's power-bomb radius follows the respawn flag.");
    }

    /// <summary>Runs the focused enemy property word codec verification command.</summary>
    [VerificationCommand("--enemy-property-word-codec")]
    internal static int EnemyPropertyWordCodecCommand()
    {
        Suite(nameof(VerifyEnemyPropertyWordCodec), () => VerifyEnemyPropertyWordCodec());
        return 0;
    }
}
