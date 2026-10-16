using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>#1291: the Magdollite's thrown lava runs typed programs, and its shot program spawns the lava's drop.</summary>
internal static class MagdolliteLavaBehaviorVerification
{
    /// <summary>
    /// Each facing's lava starts its program and holds its frame update for update beside the native list; the shot
    /// program requests the drop at the lava's position and deletes the lava on the same pass, as the native list does.
    /// </summary>
    internal static void VerifyMagdolliteLavaBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, MagdolliteLavaNativeListAddresses.Extent, "Magdollite-lava");
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 1);
        enemies.DeathsAndDrops._samusForEnemyDrops = new SamusState { Health = 99, MaxHealth = 99 };
        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Position = source.Position.WithWhole(new(288, 128));
        source.VramTilesIndex = 512;
        source.PaletteIndex = 3072;
        enemies.MagdolliteBehavior.SpawnMagdolliteLava(source, 0);
        enemies.MagdolliteBehavior.SpawnMagdolliteLava(source, 1);
        foreach ((ushort direction, MagdolliteLavaProgram program) in new (ushort, MagdolliteLavaProgram)[] { (0, MagdolliteLavaProgram.Left), (1, MagdolliteLavaProgram.Right) })
        {
            RoomEnemyProjectileSlot lava = Find(direction);
            AssertTrue(lava.Animation is MagdolliteLavaAnimation animation && animation.Cursor.Program == program, $"direction {direction} starts {program}");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, lava, MagdolliteLavaNativeListAddresses.Entry(program), 6, program.ToString());
            AssertTrue(!deleted, $"{program} holds its frame");
        }

        RoomEnemyProjectileSlot shot = Find(0);
        shot.StartAnimation(new MagdolliteLavaAnimation(MagdolliteLavaProgram.Shot));
        (bool removed, int updates) = NativeEnemyProjectileParity.Compare(cartridge, enemies, shot, MagdolliteLavaNativeListAddresses.Entry(MagdolliteLavaProgram.Shot), 4, "shot lava");
        AssertTrue(removed && updates == 1, "the shot program deletes the lava on its first pass");
        // $86:DFEA allocates its pickup at the shot lava's position.
        RoomEnemyProjectileSlot drop = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup);
        AssertEqual(source.Position.X.Whole, drop.Position.X.Whole, "the drop keeps the lava's X");
        AssertEqual(unchecked((ushort)(source.Position.Y.Whole + 2)), drop.Position.Y.Whole, "the drop keeps the lava's Y");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the lava's native list bytes");
        Console.WriteLine("  Magdollite lava behavior: both facings hold their frames and the shot program drops and deletes as natively.");

        RoomEnemyProjectileSlot Find(ushort direction) => enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite && projectile.DirectionParameter == direction);
    }

    /// <summary>Runs the Magdollite lava's program oracle and behavior suite.</summary>
    [VerificationCommand("--magdollite-lava-programs")]
    internal static int MagdolliteLavaProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(MagdolliteLavaAnimationsVerification.VerifyMagdolliteLavaAnimations), () => MagdolliteLavaAnimationsVerification.VerifyMagdolliteLavaAnimations(rom));
        Suite(nameof(VerifyMagdolliteLavaBehavior), () => VerifyMagdolliteLavaBehavior(rom));
        return 0;
    }
}
