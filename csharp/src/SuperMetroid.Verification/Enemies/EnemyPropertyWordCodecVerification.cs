using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the test-support codec between an enemy's typed contact, processing, presence, extended-frame and new-frame
/// state and their two native words (#1472, #1549): every flag crosses it unchanged in both directions, every engine
/// bit of the property word is named, a word whose low byte holds species settings is not an engine property word, an
/// unnamed extra-property bit is rejected, and a kill's power-bomb radius follows the respawn flag.
/// </summary>
internal static class EnemyPropertyWordCodecVerification
{
    /// <summary>Round-trips every flag of both words, and checks the power-bomb radius after a kill.</summary>
    internal static void VerifyEnemyPropertyWordCodec()
    {
        foreach (EnemyContact flag in new[] { EnemyContact.IgnoresSamus, EnemyContact.SolidToSamus, EnemyContact.BlocksPlasmaBeam })
        {
            AssertRoundTrips(EnemyPropertyState.None with { Contact = flag }, $"contact {flag}");
            AssertEqual(flag, flag.With(EnemyContact.BlocksPlasmaBeam).Without(EnemyContact.BlocksPlasmaBeam).With(flag), $"flag operations keep {flag}");
        }
        foreach (EnemyProcessing flag in new[] { EnemyProcessing.OffScreen, EnemyProcessing.Animates })
        {
            AssertRoundTrips(EnemyPropertyState.None with { Processing = flag }, $"processing {flag}");
            AssertEqual(flag, flag.With(EnemyProcessing.OffScreen).Without(EnemyProcessing.OffScreen).With(flag), $"flag operations keep {flag}");
        }
        foreach (EnemyPresence flag in new[] { EnemyPresence.Hidden, EnemyPresence.Deleted, EnemyPresence.RespawnsIfKilled })
        {
            AssertRoundTrips(EnemyPropertyState.None with { Presence = flag }, $"presence {flag}");
            AssertEqual(flag, flag.With(EnemyPresence.Deleted).Without(EnemyPresence.Deleted).With(flag), $"flag operations keep {flag}");
        }
        AssertRoundTrips(EnemyPropertyState.None with { CollidesByFrameHitboxes = true }, "frame-hitbox collision");
        AssertRoundTrips(EnemyPropertyState.None with { NewInstructionFrame = true }, "the new-frame flag");

        for (int bit = 8; bit < 16; bit++)
        {
            int word = 1 << bit;
            AssertEqual(word, EnemyPropertyWordCodec.EncodePropertyWord(EnemyPropertyWordCodec.Decode(word, 0)), $"property bit {bit} is named");
        }
        for (int bit = 0; bit < 8; bit++)
        {
            int word = 1 << bit;
            AssertThrows<InvalidDataException>(() => EnemyPropertyWordCodec.Decode(word, 0), $"property bit {bit} holds species settings");
            AssertEqual(word, EnemyPropertyWordCodec.SpeciesBitsOf(word), $"property bit {bit} is a species bit");
        }
        for (int bit = 0; bit < 16; bit++)
        {
            int word = 1 << bit;
            if (bit is 2 or 15)
                continue;
            AssertThrows<InvalidDataException>(() => EnemyPropertyWordCodec.Decode(0, word), $"extra-property bit {bit} is not named");
        }

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
            "Enemy property word codec: every contact, processing, presence and extra-property flag round trips; a kill's power-bomb radius follows the respawn flag.");
    }

    /// <summary>Asserts that <paramref name="state"/> crosses both words and back unchanged.</summary>
    private static void AssertRoundTrips(EnemyPropertyState state, string name) =>
        AssertEqual(state, EnemyPropertyWordCodec.Decode(
            EnemyPropertyWordCodec.EncodePropertyWord(state), EnemyPropertyWordCodec.EncodeExtraPropertyWord(state)), $"{name} round trips");

    /// <summary>Runs the focused enemy property word codec verification command.</summary>
    [VerificationCommand("--enemy-property-word-codec")]
    internal static int EnemyPropertyWordCodecCommand()
    {
        Suite(nameof(VerifyEnemyPropertyWordCodec), () => VerifyEnemyPropertyWordCodec());
        return 0;
    }
}
