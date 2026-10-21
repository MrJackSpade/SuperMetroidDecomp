using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks that a missile hit resets Mother Brain's sequence-owned walk counter.</summary>
internal static class MotherBrainMissileWalkResetVerification
{
    /// <summary>
    /// Confirms the head-hit branch zeros both the rainbow sequence's live counter and
    /// the encounter projection, preventing the recorded forward walk after a missile.
    /// </summary>
    internal static void VerifyMotherBrainMissileWalkReset()
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot head = state.Head;

        var sequence = new MotherBrainRainbowBeamAttackSequence();
        sequence.State.Body.Form = MotherBrainForm.ThirdPhase;
        sequence.ThirdPhaseFight.Phase3WalkCounter = MotherBrainWalkCredit.Of(64);
        state.RainbowBeamSequence = sequence;
        state.Form = MotherBrainForm.ThirdPhase;
        state.WalkCounter = MotherBrainWalkCredit.Of(64);
        head.Health = 0x8bd8;

        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x8100; shot.Damage = 100; shot.Direction = 8;
        shot.Position = shot.Position.WithWhole(head.Position.Whole);
        shot.Radius = new(8, 8);
        runtime.Enemies.MotherBrainBehavior.Combat.ResolveMotherBrainLaterFormHeadShot(state, head, shot, runtime.Projectiles, new SamusProjectileTypeWord(0x8100), 100);
        AssertEqual(MotherBrainWalkCredit.Empty, sequence.ThirdPhaseFight.Phase3WalkCounter, "a missile empties the sequence-owned walk credit");
        AssertEqual(MotherBrainWalkCredit.Empty, state.WalkCounter, "the encounter projection publishes the reset");
        Console.WriteLine("Mother Brain missile walk reset: the reaction reaches the counter's owner.");
    }

    /// <summary>
    /// #1388: the encounter and the sequence it attaches share one walk-credit word ($7E:780E), so a credit the
    /// encounter holds (an imported snapshot's, or one a head bomb spent) is the credit phase three walks on, and the
    /// sequence's writes are the encounter's.
    /// </summary>
    internal static void VerifyMotherBrainWalkCreditShared()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]) { WalkCounter = MotherBrainWalkCredit.Of(0x0180) };
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        state.RainbowBeamSequence = sequence;
        AssertEqual(MotherBrainWalkCredit.Of(0x0180), sequence.ThirdPhaseFight.Phase3WalkCounter, "an attached sequence reads the encounter's credit");
        state.WalkCounter = state.WalkCounter.SpendForShot(MotherBrainWalkCredit.StepCredit);
        AssertEqual(MotherBrainWalkCredit.Of(0x0080), sequence.ThirdPhaseFight.Phase3WalkCounter, "a spend through the encounter reaches the sequence");
        sequence.ThirdPhaseFight.Phase3WalkCounter = MotherBrainWalkCredit.Of(0x0040);
        AssertEqual(MotherBrainWalkCredit.Of(0x0040), state.WalkCounter, "the sequence's writes are the encounter's");
        Console.WriteLine("Mother Brain walk credit: the encounter and its attached sequence share one word.");
    }

    /// <summary>Runs the focused Mother Brain missile walk-reset check.</summary>
    [VerificationCommand("--mother-brain-missile-walk-reset")]
    internal static int MotherBrainMissileWalkResetCommand()
    {
        VerifyMotherBrainMissileWalkReset();
        VerifyMotherBrainWalkCreditShared();
        return 0;
    }
}
