using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyDraygonGoopDrops()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xda60);
        runtime.Samus!.Health = 10;
        var enemies = runtime.Enemies;
        var goop = enemies.EnemyProjectiles[^1];
        goop.Kind = RoomEnemyProjectileKind.DraygonGoop;
        goop.PreInstruction = EnemyProjectileCodePointers.RTS_8684FB;
        goop.InstructionPointer = 0x8c58; // Exact retail shot-list entry from report #389.
        goop.InstructionTimer = 1;
        goop.XPosition = 120;
        goop.YPosition = 240;
        var burstMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 16; frame++)
        {
            enemies.StepEnemyProjectiles(runtime.LevelData!, runtime.Samus);
            AssertTrue(goop.IsActive, "goop survives both eight-frame burst images before drop callback");
            AssertEqual(1, enemies.EnemyProjectiles.Count(projectile => projectile.IsActive),
                "drop is not allocated before the burst completes");
            burstMaps.Add(goop.SpritemapPointer);
        }
        AssertEqual(2, burstMaps.Count, "retail goop burst displays both spritemaps");
        enemies.StepEnemyProjectiles(runtime.LevelData!, runtime.Samus);
        AssertTrue(!goop.IsActive, "drop callback returns into native goto/delete tail");
        var pickup = enemies.EnemyProjectiles.Single(projectile => projectile.EnemyHeaderPointer == 0xde7f);
        AssertTrue(pickup.IsActive && pickup.SpritemapPointer != 0,
            "critical-energy drop becomes a live rendered pickup in the same handler pass");
        AssertEqual(120, pickup.XPosition, "drop uses destroyed goop world X");
        AssertEqual(240, pickup.YPosition, "drop uses destroyed goop world Y");
        AssertEqual(goop.SlotIndex - 1, pickup.SlotIndex,
            "drop allocation occurs before parent deletion in the descending shared pool");
        enemies.StepEnemyProjectiles(runtime.LevelData!, runtime.Samus);
        AssertEqual(1, enemies.EnemyProjectiles.Count(projectile => projectile.EnemyHeaderPointer == 0xde7f),
            "deleted goop cannot repeatedly spawn its drop");
        Console.WriteLine("Draygon goop: two burst images, native drop timing/header/position/slot, and deletion pass.");
    }
}
