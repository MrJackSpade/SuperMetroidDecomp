using SuperMetroid.Core.World;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks that the Baby Metroid's place alone holds its contact, processing and presence through the cutscene (#2310).</summary>
internal static class BabyMetroidPresenceVerification
{
    /// <summary>
    /// Spawns the cutscene Baby in Mother Brain's room and runs its main AI from its death on: initialization makes its place
    /// block the Plasma Beam and animate, its death's flicker shows it on odd enemy frames and hides it on even, its finished
    /// fade hides it through Mother Brain's readying, and the cutscene's last update deletes its place, after which Mother
    /// Brain's rings no longer look for it. The cutscene keeps no copy of these that it writes into the place every update.
    /// </summary>
    internal static void VerifyBabyMetroidPresence()
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.Loaded);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.BossEncounters.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        MotherBrainRainbowBeamAttackSequence lateFight = state.BeginLateFight();
        runtime.Enemies.BabyMetroidCutsceneBehavior.SpawnMotherBrainBabyMetroid(lateFight);
        BabyMetroidCutsceneState baby = lateFight.Baby.Arrived();
        RoomEnemyPlace place = baby.Place
            ?? throw new InvalidOperationException("Verification requires the Baby's place.");

        // The population's animation and off-screen processing, plus initialization's Plasma Beam block.
        const EnemyContact contact = EnemyContact.BlocksPlasmaBeam;
        const EnemyProcessing processing = EnemyProcessing.Animates | EnemyProcessing.OffScreen;
        AssertEqual(contact, place.Contact, "initialization makes the Baby's place block the Plasma Beam");
        AssertEqual(processing, place.Processing, "initialization starts the Baby's animation over its population's processing");
        AssertEqual(EnemyPresence.None, place.Presence, "the Baby's place keeps its population's presence");

        void RunMain(int enemyFrame)
        {
            place.FrameCounter = enemyFrame;
            runtime.Enemies.BabyMetroidCutsceneBehavior.RunMotherBrainBabyMetroidMain(place, runtime.Player.Samus, Position16<RoomPixels>.Zero);
            AssertEqual(contact, place.Contact, $"enemy frame {enemyFrame}: the Baby's contact stands");
            AssertEqual(processing, place.Processing, $"enemy frame {enemyFrame}: the Baby's processing stands");
        }

        baby.Phase = BabyMetroidCutscenePhase.DeathSequence;
        int frame = 0;
        while (baby.Phase == BabyMetroidCutscenePhase.DeathSequence)
        {
            if (frame == 1000)
                throw new InvalidOperationException("The Baby's fade to black never finished.");
            RunMain(frame);
            bool flickering = baby.Phase == BabyMetroidCutscenePhase.DeathSequence;
            EnemyPresence expected = flickering && frame % 2 == 1 ? EnemyPresence.None : EnemyPresence.Hidden;
            AssertEqual(expected, place.Presence, flickering
                ? $"enemy frame {frame}: the death's flicker shows the Baby on odd frames and hides it on even"
                : $"enemy frame {frame}: the finished fade hides the Baby");
            frame++;
        }
        AssertTrue(frame > 2, "the flicker ran on both parities before the fade finished");

        while (baby.Phase != BabyMetroidCutscenePhase.FinalCutscene)
        {
            if (frame == 2000)
                throw new InvalidOperationException("Mother Brain's readying never reached the final cutscene.");
            RunMain(frame);
            AssertEqual(EnemyPresence.Hidden, place.Presence, $"enemy frame {frame}: the Baby stays hidden while Mother Brain readies");
            frame++;
        }

        // The update that reaches the final cutscene shows the room lights' first recovery step; the call after the
        // seventh ends it.
        for (int lights = 1; baby.Phase == BabyMetroidCutscenePhase.FinalCutscene; lights++)
        {
            AssertEqual(EnemyPresence.Hidden, place.Presence, $"recovery step {lights}: the Baby stays hidden and in its place");
            AssertTrue(!baby.IsDeleted, $"recovery step {lights}: Mother Brain's rings still look for the Baby");
            if (lights == 8)
                throw new InvalidOperationException("The final cutscene did not end after the room lights' seven recovery steps.");
            RunMain(frame++);
        }
        AssertEqual(BabyMetroidCutscenePhase.Deleted, baby.Phase, "the call after the last recovery step ends the cutscene");
        AssertEqual(EnemyPresence.Hidden | EnemyPresence.Deleted, place.Presence, "the cutscene's end deletes the Baby's place");
        AssertTrue(baby.IsDeleted, "Mother Brain's rings no longer look for the deleted Baby");
        Console.WriteLine("Baby Metroid presence: the place alone holds the Baby's contact, processing, flicker, hiding and deletion.");
    }

    /// <summary>Runs the Baby's place-owned contact, processing and presence check.</summary>
    [VerificationCommand("--baby-metroid-presence")]
    internal static int BabyMetroidPresenceCommand()
    {
        VerifyBabyMetroidPresence();
        return 0;
    }
}
