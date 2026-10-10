using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: an onion ring that hits the Baby ($86:C381) subtracts $50 from the Baby slot's
    // Enemy.health inside the projectile pass. The port charged only its Baby actor and
    // published the health on the Baby's next AI turn, so in the 100% movie the slot still
    // read $0C80 at the end of the frame where native read $0C30.
    /// <summary>Verifies that a Mother Brain onion-ring hit updates both the Baby actor's health and its enemy slot during the same projectile pass.</summary>
    private static void VerifyMotherBrainRingBabyHealth()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        state.RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence();
        typeof(RoomEnemySystem).GetMethod("SpawnMotherBrainBabyMetroid", flags)!.Invoke(runtime.Enemies, [state]);
        RoomEnemySlot babySlot = state.BabyMetroidSlot!;
        AssertEqual((ushort)0x0c80, babySlot.Health, "the Baby spawns with 3200 health");

        // The movie's hit: the ring reaches ($AE,$8C), exactly $24 above the Baby at ($CE,$B0).
        RoomEnemyProjectileSlot ring = runtime.Enemies.EnemyProjectiles[16];
        ring.Kind = RoomEnemyProjectileKind.MotherBrainOnionRing;
        ring.XPosition = 0x00ae;
        ring.YPosition = 0x008c;
        ring.XRadius = ring.YRadius = 1;
        ring.XVelocity = ring.YVelocity = 0;
        ring.Variable0 = 0;
        typeof(BabyMetroidCutsceneState).GetProperty(nameof(BabyMetroidCutsceneState.XPosition))!
            .SetValue(state.BabyMetroid, (ushort)0x00ce);
        typeof(BabyMetroidCutsceneState).GetProperty(nameof(BabyMetroidCutsceneState.YPosition))!
            .SetValue(state.BabyMetroid, (ushort)0x00b0);

        typeof(RoomEnemySystem).GetMethod("RunMotherBrainOnionRingPreInstruction", flags)!
            .Invoke(runtime.Enemies, [ring, runtime.Samus, (ushort)0]);
        AssertEqual((ushort)0x0c30, state.BabyMetroid!.Health, "the ring charges the Baby $50");
        AssertEqual((ushort)0x0c30, babySlot.Health, "the Baby slot's health drops within the projectile pass");
        Console.WriteLine("Mother Brain ring Baby health: the hit writes the Baby slot's health immediately.");
    }
}
