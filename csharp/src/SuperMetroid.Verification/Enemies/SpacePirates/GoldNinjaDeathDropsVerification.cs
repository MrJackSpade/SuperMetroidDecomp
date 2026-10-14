using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Gold Ninja Pirate death projectile order observed in the Metal Pirates fight.</summary>
internal static class GoldNinjaDeathDropsVerification
{
    /// <summary>
    /// Confirms the shot tail calls EnemyDeath ($B2:87B4) before the five item drops
    /// from $B2:87B8. Reverse projectile allocation must place the explosion above
    /// the drops, as observed in the 100% movie's Metal Pirates fight (#1269).
    /// </summary>
    internal static void VerifyGoldNinjaDeathDrops()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MetalPirates);
        var samus = runtime.Player.IntroducedSamus;
        RoomEnemySlot pirate = runtime.Enemies.SlotTable.Slots.First(slot =>
            slot.EnemyDefinitionPointer == EnemyDefinitionId.PirateGoldNinja);
        samus.InputLocked = true;
        // The Gold Ninja is invincible in almost every frame. Install
        // ExtendedSpritemaps_PirateNinja_28 ($B2:8EA2), whose lower piece
        // (Hitboxes_PirateNinja_1A: X -7..6, Y 0..30) uses EnemyShot_SpacePirate_Normal
        // ($B2:8779), the callback the movie's kill entered, and hold that frame.
        const ushort normalShotFrame = 0x8ea2;
        pirate.SpritemapPointer = normalShotFrame;
        pirate.InstructionTimer = NativeCountdown.Start(0x7fff);
        pirate.Health = 1;
        runtime.Enemies.ProcessingWindow.PrepareEnemyProcessingList(pirate.Position.Whole);
        (ushort X, ushort Y) target = (pirate.Position.X.Whole, (ushort)(pirate.Position.Y.Whole + 15));

        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x0200; shot.Damage = 300; shot.Direction = 2;
        shot.Position = shot.Position.WithWhole(new(target.X, target.Y));
        shot.Radius = new(4, 4);
        shot.InstructionPointer = 0x9000; shot.InstructionTimer = NativeCountdown.Start(1);
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, samus,
            onlyNativeEnemyIndex: pirate.NativeIndex);
        AssertEqual((ushort)0, pirate.Health, "the shot kills the Gold Ninja Pirate");

        var projectiles = runtime.Enemies.Projectiles.EnemyProjectiles;
        int explosion = projectiles.Count - 1;
        while (explosion >= 0 && projectiles[explosion].Kind != RoomEnemyProjectileKind.EnemyDeathExplosion)
            explosion--;
        AssertTrue(explosion >= 0, "EnemyDeath spawns the death explosion");
        for (int slot = explosion + 1; slot < projectiles.Count; slot++)
            AssertTrue(projectiles[slot].Kind != RoomEnemyProjectileKind.EnemyDeathPickup,
                $"no drop sits above the explosion (slot {slot})");
        AssertEqual(5, projectiles.Take(explosion).Count(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathPickup),
            "MetalNinjaPirateDeathItemDropRoutine spawns its five drops below the explosion");
        Console.WriteLine("Gold Ninja death drops: the explosion is spawned before the five drops.");
    }

    /// <summary>Runs the Metal Pirates death-order reproduction from the verification command host.</summary>
    [VerificationCommand("--gold-ninja-death-drops")]
    internal static int GoldNinjaDeathDropsCommand()
    {
        VerifyGoldNinjaDeathDrops();
        return 0;
    }
}
