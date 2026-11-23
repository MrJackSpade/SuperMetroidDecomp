using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Metaree;

/// <summary>Checks how long a Metaree burrows once its dive strikes the ground (#2377).</summary>
internal static class MetareeBurrowVerification
{
    /// <summary>
    /// A Metaree diving onto a solid row starts burrowing on the update the block below stops it, whatever its burrow
    /// countdown held while it dived, and is gone on the twenty-first burrowing update, not before.
    /// </summary>
    internal static void VerifyMetareeBurrow()
    {
        RoomLevelData level = VerificationHelpersVerification.CreateSolidRowsRoom(new(16, 16), firstSolidRow: 10, solidRowCount: 1);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Metaree, place.KeptDefinition);
        place.Radius = new(8, 8);
        place.Position = place.Position.WithWhole(new(64, 100));
        // The Metaree's setup builds the state its main AI moves.
        MetareeEnemyState state = MetareeBehavior.InitializeMetaree();
        state.Action = MetareeAction.LaunchedAttack;
        state.YVelocity = 8;
        // A countdown left over from before the dive never shortens the burrow.
        state.BurrowTimer = UpdateCountdown.For(3);
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(level, 0) with
            {
                Camera = Position16<RoomPixels>.Zero,
                TimeIsFrozen = false,
                Samus = VerificationHelpersVerification.CreateSamus(SamusPoseId.FacingRightNormalPose, new(64, 140)),
            },
            new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));

        int diveUpdates = 0;
        while (state.Action == MetareeAction.LaunchedAttack)
        {
            if (++diveUpdates > 16)
                throw new InvalidOperationException("The diving Metaree never struck the solid row.");
            enemies.MetareeBehavior.RunMetareeMainAi(place.HeldOccupant(), place, state, frame);
        }
        AssertEqual(MetareeAction.Burrowing, state.Action, "the dive ends in a burrow");
        AssertEqual(MetareeMotionDefinitions.BurrowLifetime, state.BurrowTimer.UpdatesLeft, "the burrow starts its whole countdown as the Metaree strikes the ground");

        for (int update = 1; update < MetareeMotionDefinitions.BurrowLifetime; update++)
        {
            enemies.MetareeBehavior.RunMetareeMainAi(place.HeldOccupant(), place, state, frame);
            AssertTrue(!place.Presence.HasAny(EnemyPresence.Deleted), $"the Metaree is still burrowing after {update} burrowing updates");
        }
        enemies.MetareeBehavior.RunMetareeMainAi(place.HeldOccupant(), place, state, frame);
        AssertTrue(place.Presence.HasAny(EnemyPresence.Deleted), "the Metaree is gone on its twenty-first burrowing update");
        Console.WriteLine("  Metaree burrow: the burrow lasts 21 updates from the dive's landing.");
    }

    /// <summary>Runs the focused Metaree burrow check.</summary>
    [VerificationCommand("--metaree-burrow")]
    internal static int MetareeBurrowCommand()
    {
        VerifyMetareeBurrow();
        return 0;
    }
}
