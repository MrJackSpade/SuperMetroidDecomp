using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>#1292: Shaktool's unused attack circles run their typed programs.</summary>
internal static class ShaktoolProjectileBehaviorVerification
{
    /// <summary>Each circle's definition starts its typed program, which matches the native list update for update, movement routine included.</summary>
    internal static void VerifyShaktoolProjectileBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, ShaktoolProjectileNativeListAddresses.Extent, "Shaktool attack circle");
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        foreach ((RoomEnemyProjectileKind kind, ShaktoolProjectileProgram program) in new (RoomEnemyProjectileKind, ShaktoolProjectileProgram)[]
                 {
                     (RoomEnemyProjectileKind.ShaktoolAttackFrontCircle, ShaktoolProjectileProgram.Front),
                     (RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle, ShaktoolProjectileProgram.Middle),
                     (RoomEnemyProjectileKind.ShaktoolAttackBackCircle, ShaktoolProjectileProgram.Back),
                 })
        {
            RoomEnemyProjectileSlot circle = enemies.Projectiles.EnemyProjectiles[0];
            circle.Clear();
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(circle, kind, 0);
            AssertTrue(circle.Animation is ShaktoolProjectileAnimation animation && animation.Cursor.Program == program, $"{kind} starts {program}");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, circle, ShaktoolProjectileNativeListAddresses.Entry(program), 300, kind.ToString());
            AssertTrue(!deleted, $"{kind} loops on its final pose");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the circles' native list bytes");
        Console.WriteLine("  Shaktool attack-circle behavior: all three circles match their native lists.");
    }

    /// <summary>Runs the attack circles' program oracle and behavior suite.</summary>
    [VerificationCommand("--shaktool-projectile-programs")]
    internal static int ShaktoolProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(ShaktoolProjectileAnimationsVerification.VerifyShaktoolProjectileAnimations), () => ShaktoolProjectileAnimationsVerification.VerifyShaktoolProjectileAnimations(rom));
        Suite(nameof(VerifyShaktoolProjectileBehavior), () => VerifyShaktoolProjectileBehavior(rom));
        return 0;
    }
}
