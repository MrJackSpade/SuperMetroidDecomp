using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: EnemyShot_Metroid records the drop origin ($A3:EF31), runs EnemyDeath ($A3:EF4B),
    // which spawns the death explosion, and only then MetroidDeathItemDropRoutine ($A3:EF74).
    // Enemy projectiles allocate from the top slot down, so the explosion takes the highest
    // free slot and the drops follow below it, as in the 100% movie's first Metroid room.
    // #1269: $A0:A184 also does not reject a projectile an earlier enemy marked for removal
    // this pass; the movie's Super Missile hits a respawning Rinka and then the Metroid.
    /// <summary>
    /// Runs the Metroid death-drop scenario with and without an earlier same-pass projectile
    /// removal mark to verify the explosion is allocated before all five special drops.
    /// </summary>
    private static void VerifyMetroidDeathDrops()
    {
        VerifyMetroidDeathDrops(alreadyMarked: false);
        VerifyMetroidDeathDrops(alreadyMarked: true);
        Console.WriteLine("Metroid death drops: the explosion precedes the five special drops, even for an already-marked missile.");
    }

    /// <summary>
    /// Reproduces a Super Missile kill on a frozen Metroid and checks the resulting explosion
    /// allocation and five-drop request ordering in a retail-room runtime fixture.
    /// </summary>
    /// <param name="alreadyMarked">Whether the missile was marked for removal earlier in the same projectile pass.</param>
    private static void VerifyMetroidDeathDrops(bool alreadyMarked)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.TourianMetroids1);
        var samus = runtime.Samus!;
        samus.InputLocked = true;
        RoomEnemySlot metroid = runtime.Enemies.Slots.First(slot =>
            slot.EnemyDefinitionPointer == RoomEnemySystem.MetroidDefinition);
        // The shot pass rejects the empty spritemap sentinel; install the frame the movie's
        // Metroid showed when it was shot ($F137).
        metroid.SpritemapPointer = 0xf137;
        // A frozen Metroid takes the frozen branch of its shot AI; one missile finishes it.
        metroid.FrozenTimer = 100;
        metroid.Health = 1;
        runtime.Enemies.PrepareEnemyProcessingList((ushort)(metroid.XPosition - 0x80), (ushort)(metroid.YPosition - 0x70));

        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x0100; shot.Damage = 100; shot.Direction = (ushort)(alreadyMarked ? 0x12 : 0x02);
        shot.XPosition = metroid.XPosition; shot.YPosition = metroid.YPosition;
        shot.XRadius = shot.YRadius = 4;
        shot.InstructionPointer = 0x9000; shot.InstructionTimer = 1;
        runtime.Enemies.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, samus);
        AssertEqual((ushort)0, metroid.Health, $"the missile kills the frozen Metroid (already marked: {alreadyMarked})");

        var projectiles = runtime.Enemies.EnemyProjectiles;
        int explosion = projectiles.Count - 1;
        while (explosion >= 0 && projectiles[explosion].Kind != RoomEnemyProjectileKind.EnemyDeathExplosion)
            explosion--;
        AssertTrue(explosion >= 0, "EnemyDeath spawns the death explosion");
        for (int slot = explosion + 1; slot < projectiles.Count; slot++)
            AssertTrue(projectiles[slot].Kind != RoomEnemyProjectileKind.EnemyDeathPickup,
                $"no drop sits above the explosion (slot {slot})");
        AssertEqual(5, runtime.Enemies.MetroidDropRequests.Count, "the Metroid requests its five special drops");
    }
}
