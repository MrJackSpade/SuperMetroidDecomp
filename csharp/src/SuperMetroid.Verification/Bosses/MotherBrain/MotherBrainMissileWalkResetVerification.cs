using SuperMetroid.Core.Runtime;
using System.Reflection;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks that a missile hit resets Mother Brain's sequence-owned walk counter.</summary>
internal static class MotherBrainMissileWalkResetVerification
{
    /// <summary>
    /// Confirms the head-hit branch zeros both the rainbow sequence's live counter and
    /// the encounter projection, preventing the recorded forward walk after a missile.
    /// </summary>
    internal static void VerifyMotherBrainMissileWalkReset()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot head = state.Head;

        var sequence = new MotherBrainRainbowBeamAttackSequence();
        sequence.Body.Form = MotherBrainForm.ThirdPhase;
        sequence.Phase3WalkCounter = MotherBrainWalkCredit.Of(64);
        state.RainbowBeamSequence = sequence;
        state.Form = MotherBrainForm.ThirdPhase;
        state.WalkCounter = MotherBrainWalkCredit.Of(64);
        head.Health = 0x8bd8;

        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x8100; shot.Damage = 100; shot.Direction = 8;
        shot.Position = shot.Position.WithWhole(head.Position.Whole);
        shot.Radius = new(8, 8);
        runtime.Enemies.MotherBrainBehavior.ResolveMotherBrainLaterFormHeadShot(state, head, shot, runtime.Projectiles, new SamusProjectileTypeWord(0x8100), (ushort)100);
        AssertEqual(MotherBrainWalkCredit.Empty, sequence.Phase3WalkCounter, "a missile empties the sequence-owned walk credit");
        AssertEqual(MotherBrainWalkCredit.Empty, state.WalkCounter, "the encounter projection publishes the reset");
        Console.WriteLine("Mother Brain missile walk reset: the reaction reaches the counter's owner.");
    }

    /// <summary>Runs the focused Mother Brain missile walk-reset check.</summary>
    [VerificationCommand("--mother-brain-missile-walk-reset")]
    internal static int MotherBrainMissileWalkResetCommand()
    {
        VerifyMotherBrainMissileWalkReset();
        return 0;
    }
}
