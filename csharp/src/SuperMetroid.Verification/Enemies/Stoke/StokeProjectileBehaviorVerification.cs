using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>#1292: the Stoke's projectile runs its typed loop.</summary>
internal static class StokeProjectileBehaviorVerification
{
    /// <summary>The projectile's definition starts the typed loop, which matches the native list update for update.</summary>
    internal static void VerifyStokeProjectileBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, StokeProjectileNativeListAddresses.Extent, "Stoke projectile");
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[0];
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.StokeProjectile, 0);
        AssertTrue(projectile.Animation is StokeProjectileAnimation { Cursor.Program: StokeProjectileProgram.Initial }, "the projectile starts its loop");
        (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, projectile,
            StokeProjectileNativeListAddresses.Entry(StokeProjectileProgram.Initial), 60, "Stoke projectile");
        AssertTrue(!deleted, "the projectile loops");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the projectile's native list bytes");
        Console.WriteLine("  Stoke projectile behavior: the loop matches the native list.");
    }

    /// <summary>Runs the Stoke projectile's program oracle and behavior suite.</summary>
    [VerificationCommand("--stoke-projectile-programs")]
    internal static int StokeProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(StokeProjectileAnimationsVerification.VerifyStokeProjectileAnimations), () => StokeProjectileAnimationsVerification.VerifyStokeProjectileAnimations(rom));
        Suite(nameof(VerifyStokeProjectileBehavior), () => VerifyStokeProjectileBehavior(rom));
        return 0;
    }
}
