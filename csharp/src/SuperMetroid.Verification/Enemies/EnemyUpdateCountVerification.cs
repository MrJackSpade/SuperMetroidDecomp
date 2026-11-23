using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks an enemy's own update count, which its behaviour reads only through power-of-two cadences (#2454).</summary>
internal static class EnemyUpdateCountVerification
{
    /// <summary>The span of the original's sixteen-bit update count, which wrapped to none past its last value.</summary>
    private const int SixteenBitSpan = 1 << 16;

    /// <summary>
    /// An enemy whose update count stands at the original's last sixteen-bit value counts on past it through the production
    /// enemy pass, and every cadence and phase its readers take reads the same at the plain count as at the wrapped one.
    /// </summary>
    internal static void VerifyEnemyUpdateCountCountsOn()
    {
        SamusState samus = EnemyDropsVerification.CreateDropTestSamus();
        // A waiting Sbug on screen, outside its one-pixel reach of Samus, runs its main behaviour and stays where it is. The
        // room's population load sets it up (#2554), with the blank definition the rest of the check overrides.
        EnemyDropsVerification.EnemyDropFixture fixture = EnemyDropsVerification.CreateEnemyDropFixture(samus, randomValues: [],
            population: [PopulationPlacementAccess.Entry(EnemyDefinitionId.Sbug,
                SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Sbug, SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(1, 1)),
                new(samus.Position.Whole.X + 60, samus.Position.Whole.Y))],
            authored: new Dictionary<EnemyDefinitionId, RoomEnemyDefinition> { [EnemyDefinitionId.Sbug] = EnemyDefinitionFixtures.Blank });
        RoomEnemyPlace enemy = fixture.System.Places.InUpdateOrder[0];
        _ = enemy.EntityOf<SuperMetroid.Core.Enemies.Sbug.SbugEntity>();
        enemy.Position = enemy.Position.WithWhole(new(samus.Position.Whole.X + 60, samus.Position.Whole.Y));
        enemy.Radius = new(8, 8);
        enemy.Health = 1;
        enemy.Contact = EnemyContact.IgnoresSamus;
        enemy.Processing = EnemyProcessing.None;
        enemy.Presence = EnemyPresence.None;
        enemy.FrameCounter = SixteenBitSpan - 1;
        var projectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new SuperMetroid.Core.Compatibility.LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SuperMetroid.Core.Compatibility.SamusShotCount(), InstalledFixtures.SamusArtwork(), new SuperMetroid.Core.Fx.RoomEarthquake());
        fixture.System.StepStandaloneFrame(fixture.System.StandaloneFrame(fixture.Level) with { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, SamusProjectiles = projectiles, SamusColours = new SamusColours() });
        AssertEqual(SixteenBitSpan, enemy.FrameCounter, "the enemy's update counts on past the original's last sixteen-bit value");

        for (int count = 0; count < SixteenBitSpan; count++)
        {
            AssertEqual(EnemyDrawPhaseDefinitions.ShakeShiftsLeft.IsInSecondHalfAt(count), EnemyDrawPhaseDefinitions.ShakeShiftsLeft.IsInSecondHalfAt(count + SixteenBitSpan),
                $"the shake phase at update {count}");
            AssertEqual(EnemyDrawPhaseDefinitions.HurtFlashShown.IsInSecondHalfAt(count), EnemyDrawPhaseDefinitions.HurtFlashShown.IsInSecondHalfAt(count + SixteenBitSpan),
                $"the hurt flash phase at update {count}");
        }
        Console.WriteLine("  Enemy update count: counts on past 65535, its draw phases unchanged.");
    }

    /// <summary>Runs the focused enemy update count check.</summary>
    [VerificationCommand("--enemy-update-count")]
    internal static int EnemyUpdateCountCommand()
    {
        VerifyEnemyUpdateCountCountsOn();
        return 0;
    }
}
