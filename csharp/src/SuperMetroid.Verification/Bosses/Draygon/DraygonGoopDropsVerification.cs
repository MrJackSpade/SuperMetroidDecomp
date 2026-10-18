using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks Draygon goop's two-frame burst and critical-energy pickup handoff.</summary>
internal static class DraygonGoopDropsVerification
{
    /// <summary>Confirms the drop spawns after both eight-frame burst images at the destroyed goop position, before parent deletion in the descending slot pool.</summary>
    internal static void VerifyDraygonGoopDrops()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Draygon);
        runtime.Player.IntroducedSamus.Health = 10;
        var enemies = runtime.Enemies;
        var goop = enemies.Projectiles.EnemyProjectiles[^1];
        goop.Kind = RoomEnemyProjectileKind.DraygonGoop;
        goop.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_8684FB;
        goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileProgram.GoopShot));
        goop.Position = goop.Position.WithWhole(new(120, 240));
        var burstMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 16; frame++)
        {
            enemies.StepEnemyProjectiles(runtime.Room.LoadedLevelData, runtime.Player.Samus);
            AssertTrue(goop.IsActive, "goop survives both eight-frame burst images before drop callback");
            AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive),
                "drop is not allocated before the burst completes");
            // Installed-presentation frames keep a blank spritemap and name the drawn image by
            // its instruction operand; compiled frames name it by spritemap.
            burstMaps.Add(goop.PresentationOperandAddress != 0 ? goop.PresentationOperandAddress : goop.SpritemapPointer);
        }
        AssertEqual(2, burstMaps.Count, "retail goop burst displays both spritemaps");
        enemies.StepEnemyProjectiles(runtime.Room.LoadedLevelData, runtime.Player.Samus);
        AssertTrue(!goop.IsActive, "drop callback returns into native goto/delete tail");
        var pickup = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.EnemyHeaderPointer == EnemyDefinitionId.DraygonEye);
        AssertTrue(pickup.IsActive && pickup.SpritemapPointer != 0,
            "critical-energy drop becomes a live rendered pickup in the same handler pass");
        AssertEqual(120, pickup.Position.X.Whole, "drop uses destroyed goop world X");
        AssertEqual(240, pickup.Position.Y.Whole, "drop uses destroyed goop world Y");
        AssertEqual(goop.SlotIndex - 1, pickup.SlotIndex,
            "drop allocation occurs before parent deletion in the descending shared pool");
        enemies.StepEnemyProjectiles(runtime.Room.LoadedLevelData, runtime.Player.Samus);
        AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.EnemyHeaderPointer == EnemyDefinitionId.DraygonEye),
            "deleted goop cannot repeatedly spawn its drop");
        Console.WriteLine("Draygon goop: two burst images, native drop timing/header/position/slot, and deletion pass.");
    }

    /// <summary>Runs the focused Draygon goop burst and pickup allocation check.</summary>
    [VerificationCommand("--draygon-goop-drops")]
    internal static int DraygonGoopDropsCommand()
    {
        Suite(nameof(VerifyDraygonGoopDrops), () => VerifyDraygonGoopDrops());
        return 0;
    }
}
