using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Ridley's shot reaction ($A6:DF8A) is the common no-death-check damage only. His health
    /// stage ($7E:7820 accelerationIndex) changes in the health-palette handler ($A6:D474) that
    /// his main and hurt AI run after moving, so the frame a hit crosses 9000 still accelerates
    /// with the old stage. In the 13% movie a Super Missile and its link take him from 9340 to
    /// 8740 while he dodges a power bomb.
    /// </summary>
    private static void VerifyRidleyShotHealthStage()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xb32e);
        SamusState samus = runtime.Samus!;
        samus.InputLocked = true;
        RoomEnemySlot ridley = runtime.Enemies.Slots[0];
        AssertEqual(EnemyDefinitionId.Ridley, ridley.EnemyDefinitionPointer, "slot zero is Norfair Ridley");
        var state = (RidleyEnemyState)typeof(RoomEnemySystem)
            .GetMethod("RequireNorfairRidley", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(runtime.Enemies, [ridley])!;

        // Native update 56046: Ridley on frame $E983, both Super Missile slots on his body.
        ridley.XPosition = 0x00a4;
        ridley.YPosition = 0x0148;
        ridley.SpritemapPointer = 0xe983;
        ridley.Properties = 0x3800;
        ridley.InvincibilityTimer = 0;
        ridley.InstructionTimer = 0x7fff;
        ridley.Health = 9340;
        state.FightMode = 2;
        state.HealthStage = 0;
        runtime.Enemies.PrepareEnemyProcessingList(ridley.XPosition, ridley.YPosition);
        foreach (int index in new[] { 0, 1 })
        {
            SamusProjectileSlot shot = runtime.Projectiles.Slots[index];
            shot.Type = 0x8200; shot.Damage = 300; shot.Direction = 2;
            shot.XPosition = 0x00c5; shot.YPosition = 0x0164;
            shot.XRadius = shot.YRadius = 8;
            shot.InstructionPointer = 0x9000; shot.InstructionTimer = 1;
        }
        runtime.Enemies.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, samus,
            onlyNativeEnemyIndex: ridley.NativeIndex);

        AssertTrue(ridley.Health < 9000, "the hits cross 9000");
        AssertEqual((ushort)0, state.HealthStage, "the shot leaves the health stage for his AI to update");
        Console.WriteLine("  Ridley shot health stage: a hit crossing 9000 leaves the stage to the AI's palette handler.");
    }
}
