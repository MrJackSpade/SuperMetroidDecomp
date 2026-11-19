using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>Checks how long a Skree burrows once its dive strikes the ground (#2454).</summary>
internal static class SkreeBurrowVerification
{
    /// <summary>
    /// A Skree diving onto a solid row leaves its burrow countdown alone while it dives, starts its whole burrow on the
    /// update the block below stops it, throws up its debris with eight burrow updates left, and is gone on the
    /// twenty-first burrowing update, not before.
    /// </summary>
    internal static void VerifySkreeBurrow()
    {
        RoomLevelData level = VerificationHelpersVerification.CreateSolidRowsRoom(new(16, 16), firstSolidRow: 10, solidRowCount: 1);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Skree, place.Definition);
        place.Radius = new(8, 8);
        place.Position = place.Position.WithWhole(new(64, 100));
        SkreeBehavior.InitializeSkree(place);
        SkreeEnemyState state = enemies.SkreeStates[0] ?? throw new InvalidOperationException("The Skree has no state.");
        state.Action = SkreeAction.Diving;
        SamusState samus = VerificationHelpersVerification.CreateSamus(SamusPoseId.FacingRightNormalPose, new(64, 140));

        int diveUpdates = 0;
        while (state.Action == SkreeAction.Diving)
        {
            if (++diveUpdates > 16)
                throw new InvalidOperationException("The diving Skree never struck the solid row.");
            AssertEqual(0, state.BurrowTimer.UpdatesLeft, $"dive update {diveUpdates} leaves the burrow unstarted");
            enemies.SkreeBehavior.RunSkreeMain(place, samus, level);
        }
        AssertEqual(SkreeAction.Burrowing, state.Action, "the dive ends in a burrow");
        AssertEqual(SkreeMotionDefinitions.BurrowDuration, state.BurrowTimer.UpdatesLeft, "the burrow starts its whole countdown as the Skree strikes the ground");

        int burrowTop = place.Position.Y.Whole;
        for (int update = 1; update < SkreeMotionDefinitions.BurrowDuration; update++)
        {
            enemies.SkreeBehavior.RunSkreeMain(place, samus, level);
            AssertTrue(!place.Presence.HasAny(EnemyPresence.Deleted), $"the Skree is still burrowing after {update} burrowing updates");
            AssertEqual(burrowTop + update * SkreeMotionDefinitions.BurrowSink, place.Position.Y.Whole, $"burrowing update {update} sinks the Skree");
        }
        enemies.SkreeBehavior.RunSkreeMain(place, samus, level);
        AssertTrue(place.Presence.HasAny(EnemyPresence.Deleted), "the Skree is gone on its twenty-first burrowing update");
        Console.WriteLine("  Skree burrow: the burrow lasts 21 updates from the dive's landing and the dive leaves it unstarted.");
    }

    /// <summary>Runs the focused Skree burrow check.</summary>
    [VerificationCommand("--skree-burrow")]
    internal static int SkreeBurrowCommand()
    {
        VerifySkreeBurrow();
        return 0;
    }
}
