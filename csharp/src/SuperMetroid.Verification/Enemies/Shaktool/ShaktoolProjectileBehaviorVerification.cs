using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>#1292: Shaktool's unused attack circles run their typed programs.</summary>
internal static class ShaktoolProjectileBehaviorVerification
{
    /// <summary>Each circle's definition starts its typed program, which matches the native list update for update, movement routine included.</summary>
    internal static void VerifyShaktoolProjectileBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, ShaktoolProjectileNativeListAddresses.Extent, "Shaktool attack circle");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        foreach ((RoomEnemyProjectileKind kind, ShaktoolProjectileSegment program) in new VerifyShaktoolProjectileBehaviorCase[]
                 {
                     new VerifyShaktoolProjectileBehaviorCase(RoomEnemyProjectileKind.ShaktoolAttackFrontCircle, ShaktoolProjectileSegment.Front),
                     new VerifyShaktoolProjectileBehaviorCase(RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle, ShaktoolProjectileSegment.Middle),
                     new VerifyShaktoolProjectileBehaviorCase(RoomEnemyProjectileKind.ShaktoolAttackBackCircle, ShaktoolProjectileSegment.Back),
                 })
        {
            EnemyProjectile circle = enemies.Projectiles.EnemyProjectiles[0];
            circle.Delete();
            FamilyProjectileStarts.Initialize(enemies, circle, kind, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard);
            AssertTrue(circle.Animation is ShaktoolProjectileAnimation animation && animation.PlayingSegment == program, $"{kind} starts {program}");
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
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(ShaktoolProjectileAnimationsVerification.VerifyShaktoolProjectileAnimations), () => ShaktoolProjectileAnimationsVerification.VerifyShaktoolProjectileAnimations(rom));
        Suite(nameof(VerifyShaktoolProjectileBehavior), () => VerifyShaktoolProjectileBehavior(rom));
        return 0;
    }
}
