using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.MaridiaSnails;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Oum's bounce record steps, caps, goes below zero on a late impact and clamps on the rise as the cartridge's does.</summary>
internal static class OumBounceVerification
{
    /// <summary>
    /// Runs the production main AI for one bounce update of a freshly initialized Oum. Falling in open air, the record
    /// grows by 384 and is capped at 16384. Falling onto a floor with a record below 4096, the impact leaves it below
    /// zero, which ends the bounces, and the slot variable holds that negative record as the cartridge's signed word.
    /// Rising, a record that would go below zero is stored as zero and the fall begins.
    /// </summary>
    internal static void VerifyOumBounceRecord()
    {
        RoomLevelData air = VerificationHelpersVerification.CreateEmptyRoom(new(8, 8));
        RoomLevelData floor = VerificationHelpersVerification.CreateSolidRowsRoom(new(8, 8), firstSolidRow: 4, solidRowCount: 4);

        foreach ((string context, RoomLevelData level, OumBouncePhase phase, int start, int expected, OumBouncePhase expectedPhase, bool expectedSettled) in
                 new VerifyOumBounceRecordCase[]
                 {
                     new VerifyOumBounceRecordCase("falling in the air past the cap", air, OumBouncePhase.Falling, 16200, 16384, OumBouncePhase.Falling, false),
                     new VerifyOumBounceRecordCase("falling in the air below the cap", air, OumBouncePhase.Falling, 1000, 1384, OumBouncePhase.Falling, false),
                     new VerifyOumBounceRecordCase("landing with a low record", floor, OumBouncePhase.Falling, 1000, -2712, OumBouncePhase.Rising, true),
                     new VerifyOumBounceRecordCase("rising past zero", air, OumBouncePhase.Rising, 200, 0, OumBouncePhase.Falling, false),
                 })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Oum, place.Definition);
            OumBehavior.InitializeOum(place);
            OumEnemyState state = place.RequireSpeciesState<OumEnemyState>();
            place.Radius = new(16, 16);
            // On the floor level the lowest pixel row already rests in the floor, so even a sub-pixel fall lands.
            place.Position = FixedPosition<RoomPixels>.FromWhole(new(64, level == floor ? (ushort)49 : (ushort)48));
            state.BouncePhase = phase;
            state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(start);
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
            samus.Position = samus.Position.WithWhole(new(1000, 1000));
            var frame = new ClockedEnemyAiFrame(
                RoomEnemyFrameFixtures.Standalone(level, 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
                new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));

            enemies.OumBehavior.RunOumMainAi(place, frame);

            AssertEqual(expected, state.YSpeedCurvePlace.InTwoHundredFiftySixthsOfMark, $"Oum {context}: record");
            AssertEqual(expectedPhase, state.BouncePhase, $"Oum {context}: bounce phase");
            AssertEqual(expectedSettled, state.RemainingBounces == 0, $"Oum {context}: bounces ended");
        }
        Console.WriteLine("  Oum bounce record: caps at 16384, keeps a negative record after a late impact and clamps to zero rising.");
    }

    /// <summary>Runs the Oum bounce-record check.</summary>
    [VerificationCommand("--maridia-large-snail-bounce-record")]
    internal static int OumBounceRecordCommand()
    {
        Suite(nameof(VerifyOumBounceRecord), () => VerifyOumBounceRecord());
        return 0;
    }
}
