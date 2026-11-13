using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>#1292: the eye door's shot runs typed flight and impact programs.</summary>
internal static class EyeDoorProjectileBehaviorVerification
{
    /// <summary>
    /// The shot's definition starts its typed flight, which aims at Samus and loops as the native list does; its impact
    /// program plays out and deletes the shot on the native update.
    /// </summary>
    internal static void VerifyEyeDoorProjectileBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, EyeDoorProjectileNativeListAddresses.Extent, "eye-door shot");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The eye's attack spawns the shot with the door bit its typed state keeps; the aim cue reads that state.
        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(EyeDoorProjectileDefinition.Projectile, Facing: EyeDoorOrientation.Left, RoomObjectBlock: new(7, 4), Door: SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.Door(0)),
            new SharedGameState(new SharedRandom()));
        EnemyProjectile shot = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorProjectile);
        shot.Position = shot.Position.WithWhole(new(64, 32));
        AssertTrue(shot.Animation is EyeDoorProjectileAnimation { PlayingSegment: EyeDoorProjectileSegment.Initial }, "the shot starts its flight");
        (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, shot,
            EyeDoorProjectileNativeListAddresses.Entry(EyeDoorProjectileSegment.Initial), 60, "eye-door flight");
        AssertTrue(!deleted, "the flight loops");
        AssertTrue(shot.Velocity.X.RawValue != 0 || shot.Velocity.Y.RawValue != 0, "the aim cue launches the shot toward Samus");

        shot.StartAnimation(new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Impact));
        (deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, shot,
            EyeDoorProjectileNativeListAddresses.Entry(EyeDoorProjectileSegment.Impact), 30, "eye-door impact");
        AssertTrue(deleted, "the impact deletes the shot");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the shot's native list bytes");
        Console.WriteLine("  Eye-door shot behavior: the aimed flight loop and the impact match their native lists.");
    }

    /// <summary>Runs the eye-door shot's program oracle and behavior suite.</summary>
    [VerificationCommand("--eye-door-projectile-programs")]
    internal static int EyeDoorProjectileProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(EyeDoorProjectileAnimationsVerification.VerifyEyeDoorProjectileAnimations), () => EyeDoorProjectileAnimationsVerification.VerifyEyeDoorProjectileAnimations(rom));
        Suite(nameof(VerifyEyeDoorProjectileBehavior), () => VerifyEyeDoorProjectileBehavior(rom));
        return 0;
    }
}
