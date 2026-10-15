using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>#1292: the eye door's shot runs typed flight and impact programs.</summary>
internal static class EyeDoorProjectileBehaviorVerification
{
    /// <summary>
    /// The shot's definition starts its typed flight, which aims at Samus and loops as the native list does; its impact
    /// program plays out and deletes the shot on the native update.
    /// </summary>
    internal static void VerifyEyeDoorProjectileBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, EyeDoorProjectileNativeListAddresses.Extent, "eye-door shot");
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        RoomEnemyProjectileSlot shot = enemies.Projectiles.EnemyProjectiles[0];
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(shot, RoomEnemyProjectileKind.EyeDoorProjectile, 0);
        shot.Position = shot.Position.WithWhole(new(64, 32));
        AssertTrue(shot.Animation is EyeDoorProjectileAnimation { Cursor.Program: EyeDoorProjectileProgram.Initial }, "the shot starts its flight");
        (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, shot,
            EyeDoorProjectileNativeListAddresses.Entry(EyeDoorProjectileProgram.Initial), 60, "eye-door flight");
        AssertTrue(!deleted, "the flight loops");
        AssertTrue(shot.Velocity.X.RawValue != 0 || shot.Velocity.Y.RawValue != 0, "the aim cue launches the shot toward Samus");

        shot.StartAnimation(new EyeDoorProjectileAnimation(EyeDoorProjectileProgram.Impact));
        (deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, shot,
            EyeDoorProjectileNativeListAddresses.Entry(EyeDoorProjectileProgram.Impact), 30, "eye-door impact");
        AssertTrue(deleted, "the impact deletes the shot");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the shot's native list bytes");
        Console.WriteLine("  Eye-door shot behavior: the aimed flight loop and the impact match their native lists.");
    }

    /// <summary>Runs the eye-door shot's program oracle and behavior suite.</summary>
    [VerificationCommand("--eye-door-projectile-programs")]
    internal static int EyeDoorProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(EyeDoorProjectileAnimationsVerification.VerifyEyeDoorProjectileAnimations), () => EyeDoorProjectileAnimationsVerification.VerifyEyeDoorProjectileAnimations(rom));
        Suite(nameof(VerifyEyeDoorProjectileBehavior), () => VerifyEyeDoorProjectileBehavior(rom));
        return 0;
    }
}
