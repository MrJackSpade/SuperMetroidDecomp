using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class MetroidDeathDropsVerification
{
    // #1269: EnemyShot_Metroid records the drop origin ($A3:EF31), runs EnemyDeath ($A3:EF4B),
    // which spawns the death explosion, and only then MetroidDeathItemDropRoutine ($A3:EF74).
    // Enemy projectiles allocate from the top slot down, so the explosion takes the highest
    // free slot and the drops follow below it, as in the 100% movie's first Metroid room.
    // #1269: $A0:A184 also does not reject a projectile an earlier enemy marked for removal
    // this pass; the movie's Super Missile hits a respawning Rinka and then the Metroid.
    internal static void VerifyMetroidDeathDrops()
    {
        VerifyMetroidDeathDrops(alreadyMarked: false);
        VerifyMetroidDeathDrops(alreadyMarked: true);
        Console.WriteLine("Metroid death drops: the explosion precedes the five special drops, even for an already-marked missile.");
    }

    private static void VerifyMetroidDeathDrops(bool alreadyMarked)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.TourianMetroids1);
        var samus = runtime.Player.Samus!;
        samus.InputLocked = true;
        RoomEnemySlot metroid = runtime.Enemies.SlotTable.Slots.First(slot =>
            slot.EnemyDefinitionPointer == EnemyDefinitionId.Metroid);
        // The shot pass rejects the empty spritemap sentinel; install the frame the movie's
        // Metroid showed when it was shot ($F137).
        metroid.SpritemapPointer = 0xf137;
        // A frozen Metroid takes the frozen branch of its shot AI; one missile finishes it.
        metroid.FrozenTimer = NativeCountdown.Start(100);
        metroid.Health = 1;
        runtime.Enemies.ProcessingWindow.PrepareEnemyProcessingList(new((ushort)(metroid.Position.X.Whole - 0x80), (ushort)(metroid.Position.Y.Whole - 0x70)));

        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x0100; shot.Damage = 100; shot.Direction = (ushort)(alreadyMarked ? 0x12 : 0x02);
        shot.Position = shot.Position.WithWhole(metroid.Position.Whole);
        shot.Radius = new(4, 4);
        shot.InstructionPointer = 0x9000; shot.InstructionTimer = NativeCountdown.Start(1);
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, samus);
        AssertEqual((ushort)0, metroid.Health, $"the missile kills the frozen Metroid (already marked: {alreadyMarked})");

        var projectiles = runtime.Enemies.Projectiles.EnemyProjectiles;
        int explosion = projectiles.Count - 1;
        while (explosion >= 0 && projectiles[explosion].Kind != RoomEnemyProjectileKind.EnemyDeathExplosion)
            explosion--;
        AssertTrue(explosion >= 0, "EnemyDeath spawns the death explosion");
        for (int slot = explosion + 1; slot < projectiles.Count; slot++)
            AssertTrue(projectiles[slot].Kind != RoomEnemyProjectileKind.EnemyDeathPickup,
                $"no drop sits above the explosion (slot {slot})");
        AssertEqual(5, runtime.Enemies.MetroidDropRequests.Count, "the Metroid requests its five special drops");
    }

    [VerificationCommand("--metroid-death-drops")]
    internal static int MetroidDeathDropsCommand()
    {
        VerifyMetroidDeathDrops();
        return 0;
    }
}
