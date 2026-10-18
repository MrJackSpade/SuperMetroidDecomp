using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mother Brain's persistent head collision frame separately from her drawn brain animation.</summary>
internal static class MotherBrainHeadHitboxVerification
{
    /// <summary>
    /// Confirms the head retains dummy frame $A9:A320 and its body hitbox while the
    /// visible brain animates separately, so the recorded charged beam lands.
    /// </summary>
    internal static void VerifyMotherBrainHeadHitbox()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot head = state.Head;
        AssertEqual(AnimationProgramCursor<MotherBrainHeadProgram>.Start(MotherBrainHeadProgram.Initial), state.BrainCursor,
            "the brain program starts at InstList_MotherBrainHead_Initial");

        EnemyInstructionInterpreter.ProcessInstructions(head, null, null, Position16<RoomPixels>.Zero, (ushort)0);
        AssertEqual((ushort)0xa320, head.SpritemapPointer, "the head enemy carries the dummy list's hitbox frame");

        // The movie's frame: second form, head at ($7C,$58) when the beam at ($9E,$81) reaches
        // it. A 16-pixel radius test misses by one pixel (41 > 40); the hitbox overlaps.
        state.Form = MotherBrainForm.SecondPhase;
        head.Health = 18000;
        head.Position = head.Position.WithWhole(new(0x007c, 0x0058));
        head.InvincibilityTimer = NativeCountdown.Start(0);
        runtime.Enemies.ProcessingWindow.PrepareEnemyProcessingList(new(0x0000, 0x0000));
        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x901b; shot.Damage = 900; shot.Direction = 8;
        shot.Position = shot.Position.WithWhole(new(0x009e, 0x0081)); shot.Radius = new(24, 24);
        shot.Cursor = ProjectileFixtureCursors.Active; shot.InstructionTimer = NativeCountdown.Start(1);
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(runtime.Projectiles, runtime.BombProjectiles, runtime.Player.Samus);
        AssertEqual((ushort)17100, head.Health, "the charged beam lands on the head's native hitbox");
        Console.WriteLine("Mother Brain head hitbox: the head enemy keeps $A320; the brain list runs separately.");
    }

    /// <summary>Runs the focused head-hitbox projectile check.</summary>
    [VerificationCommand("--mother-brain-head-hitbox")]
    internal static int MotherBrainHeadHitboxCommand()
    {
        VerifyMotherBrainHeadHitbox();
        return 0;
    }
}
