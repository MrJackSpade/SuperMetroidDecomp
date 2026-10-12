using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

/// <summary>Represents ridley shot health stage verification.</summary>
internal static class RidleyShotHealthStageVerification
{
    /// <summary>
    /// Ridley's shot reaction ($A6:DF8A) is the common no-death-check damage only. His health
    /// stage ($7E:7820 accelerationIndex) changes in the health-palette handler ($A6:D474) that
    /// his main and hurt AI run after moving, so the frame a hit crosses 9000 still accelerates
    /// with the old stage. In the 13% movie a Super Missile and its link take him from 9340 to
    /// 8740 while he dodges a power bomb.
    /// </summary>
    internal static void VerifyRidleyShotHealthStage()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xb32e);
        SamusState samus = runtime.Player.Samus!;
        samus.InputLocked = true;
        RoomEnemySlot ridley = runtime.Enemies.SlotTable.Slots[0];
        AssertEqual(EnemyDefinitionId.Ridley, ridley.EnemyDefinitionPointer, "slot zero is Norfair Ridley");
        var state = (RidleyEnemyState)runtime.Enemies.RidleyEncounter.RequireNorfairRidley(ridley);

        // Native update 56046: Ridley on frame $E983, both Super Missile slots on his body.
        ridley.Position = ridley.Position.WithWhole(new(0x00a4, 0x0148));
        ridley.SpritemapPointer = 0xe983;
        ridley.Properties = 0x3800;
        ridley.InvincibilityTimer = 0;
        ridley.InstructionTimer = 0x7fff;
        ridley.Health = 9340;
        state.FightMode = RidleyFightMode.PowerBombDodge;
        state.HealthStage = 0;
        runtime.Enemies.ProcessingWindow.PrepareEnemyProcessingList(ridley.Position.Whole);
        foreach (int index in new[] { 0, 1 })
        {
            SamusProjectileSlot shot = runtime.Projectiles.Slots[index];
            shot.Type = 0x8200; shot.Damage = 300; shot.Direction = 2;
            shot.Position = shot.Position.WithWhole(new(0x00c5, 0x0164));
            shot.Radius = new(8, 8);
            shot.InstructionPointer = 0x9000; shot.InstructionTimer = 1;
        }
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, samus,
            onlyNativeEnemyIndex: ridley.NativeIndex);

        AssertTrue(ridley.Health < 9000, "the hits cross 9000");
        AssertEqual((ushort)0, state.HealthStage, "the shot leaves the health stage for his AI to update");
        Console.WriteLine("  Ridley shot health stage: a hit crossing 9000 leaves the stage to the AI's palette handler.");
    }

    /// <summary>Ridley shot health stage command.</summary>
    [VerificationCommand("--ridley-shot-health-stage")]
    internal static int RidleyShotHealthStageCommand()
    {
        Suite(nameof(VerifyRidleyShotHealthStage), () => VerifyRidleyShotHealthStage());
        return 0;
    }
}
