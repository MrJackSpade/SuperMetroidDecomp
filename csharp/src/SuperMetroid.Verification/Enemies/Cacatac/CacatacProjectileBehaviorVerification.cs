using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>#1292: the Cacatac's spikes run the typed program of their launch direction.</summary>
internal static class CacatacProjectileBehaviorVerification
{
    /// <summary>Every launch direction spawns a spike on that direction's program, which matches its native list update for update.</summary>
    internal static void VerifyCacatacProjectileBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, CacatacProjectileNativeListAddresses.Extent, "Cacatac spike");
        foreach (CacatacSpikeDirection direction in Enum.GetValues<CacatacSpikeDirection>())
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            var source = new RoomEnemySlot(0, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)) };
            enemies.CacatacBehavior.SpawnCacatacSpike(source, (ushort)direction);
            RoomEnemyProjectileSlot spike = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike);
            CacatacProjectileProgram program = CacatacProjectileDefinitions.Program(direction);
            AssertTrue(spike.Animation is CacatacProjectileAnimation animation && animation.Cursor.Program == program, $"a {direction} spike starts {program}");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, spike, CacatacProjectileNativeListAddresses.Entry(program), 8, direction.ToString());
            AssertTrue(!deleted, $"the {direction} spike holds its pose");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the spikes' native list bytes");
        Console.WriteLine("  Cacatac spike behavior: all ten directions start their programs and match the native lists.");
    }

    /// <summary>Runs the Cacatac spike's program oracle and behavior suite.</summary>
    [VerificationCommand("--cacatac-projectile-programs")]
    internal static int CacatacProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(CacatacProjectileAnimationsVerification.VerifyCacatacProjectileAnimations), () => CacatacProjectileAnimationsVerification.VerifyCacatacProjectileAnimations(rom));
        Suite(nameof(VerifyCacatacProjectileBehavior), () => VerifyCacatacProjectileBehavior(rom));
        return 0;
    }
}
