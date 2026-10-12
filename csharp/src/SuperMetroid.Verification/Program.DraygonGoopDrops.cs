using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

/// <summary>Represents draygon goop drops verification.</summary>
internal static class DraygonGoopDropsVerification
{
    /// <summary>Verifies draygon goop drops.</summary>
    internal static void VerifyDraygonGoopDrops()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xda60);
        runtime.Player.Samus!.Health = 10;
        var enemies = runtime.Enemies;
        var goop = enemies.Projectiles.EnemyProjectiles[^1];
        goop.Kind = RoomEnemyProjectileKind.DraygonGoop;
        goop.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_8684FB;
        goop.InstructionPointer = DraygonProjectileInstructionProgramDefinitions.GoopShot;
        goop.InstructionTimer = 1;
        goop.Position = goop.Position.WithWhole(new(120, 240));
        var burstMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 16; frame++)
        {
            enemies.StepEnemyProjectiles(runtime.Room.LevelData!, runtime.Player.Samus);
            AssertTrue(goop.IsActive, "goop survives both eight-frame burst images before drop callback");
            AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive),
                "drop is not allocated before the burst completes");
            // Installed-presentation frames keep a blank spritemap and name the drawn image by
            // its instruction operand; compiled frames name it by spritemap.
            burstMaps.Add(goop.PresentationOperandAddress != 0 ? goop.PresentationOperandAddress : goop.SpritemapPointer);
        }
        AssertEqual(2, burstMaps.Count, "retail goop burst displays both spritemaps");
        enemies.StepEnemyProjectiles(runtime.Room.LevelData!, runtime.Player.Samus);
        AssertTrue(!goop.IsActive, "drop callback returns into native goto/delete tail");
        var pickup = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.EnemyHeaderPointer == EnemyDefinitionId.DraygonEye);
        AssertTrue(pickup.IsActive && pickup.SpritemapPointer != 0,
            "critical-energy drop becomes a live rendered pickup in the same handler pass");
        AssertEqual(120, pickup.Position.X.Whole, "drop uses destroyed goop world X");
        AssertEqual(240, pickup.Position.Y.Whole, "drop uses destroyed goop world Y");
        AssertEqual(goop.SlotIndex - 1, pickup.SlotIndex,
            "drop allocation occurs before parent deletion in the descending shared pool");
        enemies.StepEnemyProjectiles(runtime.Room.LevelData!, runtime.Player.Samus);
        AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.EnemyHeaderPointer == EnemyDefinitionId.DraygonEye),
            "deleted goop cannot repeatedly spawn its drop");
        Console.WriteLine("Draygon goop: two burst images, native drop timing/header/position/slot, and deletion pass.");
    }

    /// <summary>Draygon goop drops command.</summary>
    [VerificationCommand("--draygon-goop-drops")]
    internal static int DraygonGoopDropsCommand()
    {
        Suite(nameof(VerifyDraygonGoopDrops), () => VerifyDraygonGoopDrops());
        return 0;
    }
}
