using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Enemies.Metroids;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

/// <summary>Checks Metroid death explosion and special-drop ordering for fresh and already-marked missiles.</summary>
internal static class MetroidDeathDropsVerification
{
    /// <summary>Checks top-down projectile slot ordering when a Metroid dies from either a fresh or previously marked shot.</summary>
    /// <remarks>#1269: native $A3:EF31/$EF4B spawns the explosion before $A3:EF74 requests five drops; $A0:A184 accepts a shot marked by an earlier enemy in the same pass.</remarks>
    internal static void VerifyMetroidDeathDrops()
    {
        VerifyMetroidDeathDrops(alreadyMarked: false);
        VerifyMetroidDeathDrops(alreadyMarked: true);
        Console.WriteLine("Metroid death drops: the explosion precedes the five special drops, even for an already-marked missile.");
    }

    /// <summary>Runs the reported Metroid collision through production projectile and death dispatch.</summary>
    private static void VerifyMetroidDeathDrops(bool alreadyMarked)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Metroids1);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.LockOutPlayer();
        RoomEnemyPlace metroid = runtime.Enemies.Places.InUpdateOrder.First(place =>
            place.Holds(EnemyDefinitionId.Metroid));
        // The shot pass rejects the empty spritemap sentinel; install the frame the movie's
        // Metroid showed when it was shot ($F137).
        metroid.SpritemapPointer = 0xf137;
        // A frozen Metroid takes the frozen branch of its shot AI; one missile finishes it.
        metroid.Freeze.Restart(100);
        metroid.Health = 1;
        runtime.Enemies.PrepareEnemyProcessingList(new((ushort)(metroid.Position.X.Whole - 0x80), (ushort)(metroid.Position.Y.Whole - 0x70)));

        var shot = runtime.Projectiles.Shots[0];
        shot.Occupy(SamusProjectileType.Of(SamusProjectileFamily.Missile)); shot.Damage = 100; shot.Direction = SamusProjectileDirection.Right; if (alreadyMarked) shot.Spend();
        shot.Position = shot.Position.WithWhole(metroid.Position.Whole);
        shot.Radius = new(4, 4);
        shot.Animation = ProjectileFixturePlaybacks.Active();
        runtime.Enemies.ResolveShotsOnInteractiveEnemies(runtime.Projectiles, runtime.BombProjectiles, runtime.Room.LoadedRoom.Area, samus);
        AssertEqual(0, metroid.Health, $"the missile kills the frozen Metroid (already marked: {alreadyMarked})");

        var projectiles = runtime.Enemies.Projectiles;
        int explosion = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileEngine.PlaceCount - 1;
        while (explosion >= 0 && projectiles.LatestInPlace(explosion)?.Kind != RoomEnemyProjectileKind.EnemyDeathExplosion)
            explosion--;
        AssertTrue(explosion >= 0, "EnemyDeath spawns the death explosion");
        for (int slot = explosion + 1; slot < SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileEngine.PlaceCount; slot++)
            AssertTrue(projectiles.LatestInPlace(slot)?.Kind != RoomEnemyProjectileKind.EnemyDeathPickup,
                $"no drop sits above the explosion (slot {slot})");
        AssertEqual(5, runtime.Enemies.ActiveEnemyDropPickups, "the Metroid requests its five special drops");
    }

    /// <summary>Runs the focused Metroid death-drop verification command.</summary>
    [VerificationCommand("--metroid-death-drops")]
    internal static int MetroidDeathDropsCommand()
    {
        VerifyMetroidDeathDrops();
        return 0;
    }
}
