using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Hoppers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>The hop arc's quadratic-speed record steps, clamps at the apex and caps on the way back as the cartridge's do.</summary>
internal static class HopperHopArcVerification
{
    /// <summary>
    /// Runs the production main AI for one airborne update of a floor Hopper in open air. Moving away from the surface,
    /// the record retreats by its step; a result below zero is stored as zero and turns the hop back toward the surface,
    /// while a result of exactly zero keeps rising. Returning, the record grows by its step up to 64. The record's slot
    /// variable holds the same value.
    /// </summary>
    internal static void VerifyHopperHopArcRecord()
    {
        RoomLevelData level = VerificationHelpersVerification.CreateEmptyRoom(new(8, 8));
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(level, 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) },
            new FrameCounters(0));
        foreach ((bool falling, int start, int step, int expected, bool expectedFalling) in new VerifyHopperHopArcRecordCase[]
                 {
                     new VerifyHopperHopArcRecordCase(false, 2, 3, 0, true),
                     new VerifyHopperHopArcRecordCase(false, 3, 3, 0, false),
                     new VerifyHopperHopArcRecordCase(false, 10, 4, 6, false),
                     new VerifyHopperHopArcRecordCase(true, 59, 4, 63, true),
                     new VerifyHopperHopArcRecordCase(true, 62, 4, 64, true),
                 })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Sidehopper, place.Definition);
            place.Position = place.Position.WithWhole(new(64, 48));
            place.Radius = new(8, 8);
            // The hopper reads its surface from its placement (#2474).
            place.SetSpawnSettings(new HopperSpawnSettings(HopperSurface.Floor));
            // The hop's two halves are its two actions (#2473).
            var state = place.SetUpPlaced<HopperEnemyState>();
            state.Action = falling ? HopperAction.Returning : HopperAction.HoppingAway;
            state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(start);
            state.YSpeedCurvePlaceStep = QuadraticSpeedCurvePlace.FromMarks(step);

            enemies.HoppersBehavior.RunHoppersMainAi(place, place.RequireSpeciesState<HopperEnemyState>(), frame);

            string context = $"Hopper {(falling ? "returning" : "rising")} from record {start} by {step}";
            AssertEqual(QuadraticSpeedCurvePlace.FromMarks(expected), state.YSpeedCurvePlace, context);
            AssertEqual(expectedFalling ? HopperAction.Returning : HopperAction.HoppingAway, state.Action, $"{context}: direction");
        }
        Console.WriteLine("  Hopper hop arc: the record clamps to zero past the apex and caps at 64 on the way back.");
    }

    /// <summary>Runs the Hopper hop-arc record check.</summary>
    [VerificationCommand("--hopper-hop-arc")]
    internal static int HopperHopArcCommand()
    {
        Suite(nameof(VerifyHopperHopArcRecord), () => VerifyHopperHopArcRecord());
        return 0;
    }
}
