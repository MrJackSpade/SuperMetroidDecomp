using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Projectiles;

/// <summary>Checks that a missile hit resets Mother Brain's sequence-owned walk counter.</summary>
internal static class MotherBrainMissileWalkResetVerification
{
    /// <summary>
    /// Confirms the head-hit branch zeros both the rainbow sequence's live counter and
    /// the encounter projection, preventing the recorded forward walk after a missile.
    /// </summary>
    internal static void VerifyMotherBrainMissileWalkReset()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot head = state.Head;

        var sequence = new MotherBrainRainbowBeamAttackSequence();
        sequence.Body.Form = 4;
        sequence.Phase3WalkCounter = (ushort)0x0040;
        state.RainbowBeamSequence = sequence;
        state.Form = 4;
        state.WalkCounter = 0x0040;
        head.Health = 0x8bd8;

        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x8100; shot.Damage = 100; shot.Direction = 8;
        shot.Position = shot.Position.WithWhole(head.Position.Whole);
        shot.Radius = new(8, 8);
        runtime.Enemies.MotherBrainBehavior.ResolveMotherBrainLaterFormHeadShot(state, head, shot, runtime.Projectiles, new SamusProjectileTypeWord(0x8100), (ushort)100);
        AssertEqual((ushort)0, sequence.Phase3WalkCounter, "a missile zeroes the sequence-owned walk counter");
        AssertEqual((ushort)0, state.WalkCounter, "the encounter projection publishes the reset");
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
