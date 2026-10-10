using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: the Gold Ninja Pirate's shot tail calls EnemyDeath ($B2:87B4), which spawns the
    // death explosion, and only then MetalNinjaPirateDeathItemDropRoutine ($B2:87B8). Enemy
    // projectiles allocate from the top slot down, so the explosion takes the highest free
    // slot and the five drops follow below it, as in the 100% movie's Metal Pirates fight.
    /// <summary>Verifies a Gold Ninja death allocates its explosion before the five pickup projectiles in descending projectile-slot order.</summary>
    private static void VerifyGoldNinjaDeathDrops()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MetalPirates);
        var samus = runtime.Samus!;
        RoomEnemySlot pirate = runtime.Enemies.Slots.First(slot =>
            slot.EnemyDefinitionPointer == RoomEnemySystem.GoldNinjaSpacePirateDefinition);
        samus.InputLocked = true;
        // The Gold Ninja is invincible in almost every frame. Install
        // ExtendedSpritemaps_PirateNinja_28 ($B2:8EA2), whose lower piece
        // (Hitboxes_PirateNinja_1A: X -7..6, Y 0..30) uses EnemyShot_SpacePirate_Normal
        // ($B2:8779), the callback the movie's kill entered, and hold that frame.
        const ushort normalShotFrame = 0x8ea2;
        pirate.SpritemapPointer = normalShotFrame;
        pirate.InstructionTimer = 0x7fff;
        pirate.Health = 1;
        runtime.Enemies.PrepareEnemyProcessingList(pirate.XPosition, pirate.YPosition);
        (ushort X, ushort Y) target = (pirate.XPosition, (ushort)(pirate.YPosition + 15));

        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x0200; shot.Damage = 300; shot.Direction = 2;
        shot.XPosition = target.X; shot.YPosition = target.Y;
        shot.XRadius = shot.YRadius = 4;
        shot.InstructionPointer = 0x9000; shot.InstructionTimer = 1;
        runtime.Enemies.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, samus,
            onlyNativeEnemyIndex: pirate.NativeIndex);
        AssertEqual((ushort)0, pirate.Health, "the shot kills the Gold Ninja Pirate");

        var projectiles = runtime.Enemies.EnemyProjectiles;
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
}
