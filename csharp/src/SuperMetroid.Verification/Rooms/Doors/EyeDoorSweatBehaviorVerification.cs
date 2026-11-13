using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>#1296: the eye door's sweat drop runs typed falling and floor-splash programs.</summary>
internal static class EyeDoorSweatBehaviorVerification
{
    /// <summary>
    /// The sweat's spawn starts its typed fall, which loops as the native list does; a real floor collision starts the
    /// splash, which stops the drop, applies the native four-pixel correction and deletes it on the native update.
    /// </summary>
    internal static void VerifyEyeDoorSweatBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, EyeDoorSweatNativeListAddresses.Extent, "eye-door sweat");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        const int roomWidth = 16;
        var foreground = new RoomTerrainBlock[roomWidth * 16];
        foreground[5 * roomWidth + 6] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
        RoomLevelData level = CreateRoom(new(roomWidth, 16), foreground, new RoomBlockBehavior[foreground.Length]);
        var request = new EyeDoorProjectileRequest(EyeDoorProjectileDefinition.Sweat, Facing: EyeDoorOrientation.Right, RoomObjectBlock: new(7, 4), Door: SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(0));
        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(request, new SharedGameState(new SharedRandom()));
        EnemyProjectile sweat = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat);
        AssertTrue(sweat.Animation is EyeDoorSweatAnimation { PlayingSegment: EyeDoorSweatSegment.Falling }, "the sweat starts its fall");
        (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, sweat,
            EyeDoorSweatNativeListAddresses.Entry(EyeDoorSweatSegment.Falling), 20, "eye-door sweat fall");
        AssertTrue(!deleted, "the fall loops");

        EyeDoorBehavior.RunEyeDoorSweatFrameBehavior(sweat, level);
        AssertTrue(sweat.Animation is EyeDoorSweatAnimation { PlayingSegment: EyeDoorSweatSegment.HitFloor }, "the floor collision starts the splash");
        AssertEqual(EnemyProjectileFrameBehavior.Stationary, sweat.FrameBehavior,
            "the splash stops the drop with the native inert pre-instruction");
        AssertEqual((ushort)76, sweat.Position.Y.Whole, "the splash applies the native four-pixel floor correction");
        (deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, sweat,
            EyeDoorSweatNativeListAddresses.Entry(EyeDoorSweatSegment.HitFloor), 30, "eye-door sweat splash");
        AssertTrue(deleted, "the splash deletes the drop");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the sweat's native list bytes");
        Console.WriteLine("  Eye-door sweat behavior: the falling loop and the floor splash match their native lists.");
    }

    /// <summary>Runs the eye-door sweat's program oracle and behavior suite.</summary>
    [VerificationCommand("--eye-door-sweat-programs")]
    internal static int EyeDoorSweatProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(EyeDoorSweatAnimationsVerification.VerifyEyeDoorSweatAnimations), () => EyeDoorSweatAnimationsVerification.VerifyEyeDoorSweatAnimations(rom));
        Suite(nameof(VerifyEyeDoorSweatBehavior), () => VerifyEyeDoorSweatBehavior(rom));
        return 0;
    }
}
