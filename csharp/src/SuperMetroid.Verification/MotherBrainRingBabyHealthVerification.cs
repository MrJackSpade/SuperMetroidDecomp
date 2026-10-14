using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;

/// <summary>Reproduces issue #1269's Mother Brain onion-ring hit on the Baby Metroid.</summary>
internal static class MotherBrainRingBabyHealthVerification
{
    /// <summary>
    /// Confirms a $86:C381 onion-ring hit subtracts $50 from the Baby's actor and enemy
    /// slot during the projectile pass. The 100% movie requires $0C30 in both places
    /// that frame rather than publishing the slot update on the next AI turn.
    /// </summary>
    internal static void VerifyMotherBrainRingBabyHealth()
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
        state.RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence();
        runtime.Enemies.BabyMetroidCutsceneBehavior.SpawnMotherBrainBabyMetroid(state);
        RoomEnemySlot babySlot = (state.BabyMetroidSlot ?? throw new InvalidOperationException("Verification requires state.BabyMetroidSlot."));
        AssertEqual((ushort)0x0c80, (babySlot ?? throw new InvalidOperationException("Verification requires babySlot.")).Health, "the Baby spawns with 3200 health");

        // The movie's hit: the ring reaches ($AE,$8C), exactly $24 above the Baby at ($CE,$B0).
        RoomEnemyProjectileSlot ring = runtime.Enemies.Projectiles.EnemyProjectiles[16];
        ring.Kind = RoomEnemyProjectileKind.MotherBrainOnionRing;
        ring.Position = ring.Position.WithWhole(new(0x00ae, 0x008c));
        ring.Radius = new(1, 1);
        ring.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        ring.Variable0 = 0;
        (state.BabyMetroid ?? throw new InvalidOperationException("Verification requires state.BabyMetroid.")).Position = state.BabyMetroid.Position.WithWhole(new(0x00ce, 0x00b0));

        runtime.Enemies.MotherBrainBehavior.RunMotherBrainOnionRingPreInstruction(ring, runtime.Player.Samus, (ushort)0);
        AssertEqual((ushort)0x0c30, state.BabyMetroid.Health, "the ring charges the Baby $50");
        AssertEqual((ushort)0x0c30, babySlot.Health, "the Baby slot's health drops within the projectile pass");
        Console.WriteLine("Mother Brain ring Baby health: the hit writes the Baby slot's health immediately.");
    }

    /// <summary>Runs the focused same-frame Baby health publication check.</summary>
    [VerificationCommand("--mother-brain-ring-baby-health")]
    internal static int MotherBrainRingBabyHealthCommand()
    {
        VerifyMotherBrainRingBabyHealth();
        return 0;
    }
}
