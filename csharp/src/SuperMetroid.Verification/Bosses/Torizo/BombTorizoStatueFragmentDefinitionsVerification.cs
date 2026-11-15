using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Checks the eight Bomb Torizo statue fragments against native physical tables and real projectile allocation.</summary>
internal static class BombTorizoStatueFragmentDefinitionsVerification
{
    /// <summary>Compares five fields, rejects fragments outside the eight, and checks spawned fragment position and motion.</summary>
    internal static void VerifyBombTorizoStatueFragmentDefinitions(
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyStatueFragmentProgramField), () => VerifyStatueFragmentProgramField(rom));
        Suite(nameof(VerifyStatueFragmentXField), () => VerifyStatueFragmentXField(rom));
        Suite(nameof(VerifyStatueFragmentYField), () => VerifyStatueFragmentYField(rom));
        Suite(nameof(VerifyStatueFragmentVelocityField), () => VerifyStatueFragmentVelocityField(rom));
        Suite(nameof(VerifyStatueFragmentAccelerationField), () => VerifyStatueFragmentAccelerationField(rom));
        AssertEqual(8, BombTorizoStatueFragmentDefinitions.FragmentCount, "the hand's crumble drops eight fragments");
        foreach (int fragment in new[] { -1, 8, 16, 256, int.MaxValue, int.MinValue })
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoStatueFragments.ForFragment(fragment), "statue rejects fragments outside the eight");
        var guarded = NewBombTorizoStatueFragmentReadGuard(rom);
        for (int index = 0; index < BombTorizoStatueFragmentDefinitions.FragmentCount; index++)
        {
            BombTorizoStatueFragmentDefinition expected =
                BombTorizoStatueFragments.ForFragment(index);
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            EnemyProjectile projectile =
                enemies.TorizoBehavior.Attacks.SpawnBombTorizoStatueBreakingProjectile(
                    new BombTorizoStatueProjectileRequest(
                        index,
                        RoomObjectBlock: new(10, 20))) ??
                throw new InvalidDataException(
                    $"Bomb Torizo statue fragment {index} failed to allocate.");

            AssertEqual(RoomEnemyProjectileKind.BombTorizoStatueBreaking,
                projectile.Kind,
                $"Bomb Torizo statue fragment {index} projectile kind");
            AssertEqual<TorizoProjectileSegment?>(expected.Animation, (projectile.Animation as TorizoProjectileAnimation)?.PlayingSegment,
                $"Bomb Torizo statue fragment {index} production program");
            AssertEqual(unchecked((ushort)(10 * 16 + expected.Offset.X)),
                projectile.Position.X.Whole,
                $"Bomb Torizo statue fragment {index} production X");
            AssertEqual(unchecked((ushort)(20 * 16 + expected.Offset.Y)),
                projectile.Position.Y.Whole,
                $"Bomb Torizo statue fragment {index} production Y");
            AssertEqual(expected.InitialFallSpeed, projectile.Velocity.Y,
                $"Bomb Torizo statue fragment {index} production velocity");
            AssertEqual(expected.Acceleration, projectile.StateOf<BombTorizoStatueFragmentState>().Acceleration,
                $"Bomb Torizo statue fragment {index} production acceleration");
        }

        Console.WriteLine(
            "Bomb Torizo statue fragments: 40 native source words and all eight real room-graphics projectile allocations pass with the five physical tables forbidden.");
    }

    /// <summary>Checks the instruction list of each fragment against the native table its two-byte offset selects.</summary>
    private static void VerifyStatueFragmentProgramField(ISnesAddressSpace rom)
    {
        for (int fragment = 0, parameter = 0; fragment < BombTorizoStatueFragmentDefinitions.FragmentCount; fragment++, parameter += 2)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86a7ab + parameter)),
                TorizoNativeListAddresses.ProjectileEntry(BombTorizoStatueFragments.ForFragment(fragment).Animation), "statue native program field");
    }
    /// <summary>Checks signed horizontal offsets.</summary>
    private static void VerifyStatueFragmentXField(ISnesAddressSpace rom)
    {
        for (int fragment = 0, parameter = 0; fragment < BombTorizoStatueFragmentDefinitions.FragmentCount; fragment++, parameter += 2)
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86a7cb + parameter))),
                BombTorizoStatueFragments.ForFragment(fragment).Offset.X, "statue native signed X field");
    }
    /// <summary>Checks the eight-row vertical offset table.</summary>
    private static void VerifyStatueFragmentYField(ISnesAddressSpace rom)
    {
        for (int fragment = 0, parameter = 0; fragment < BombTorizoStatueFragmentDefinitions.FragmentCount; fragment++, parameter += 2)
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86a7eb + parameter))),
                BombTorizoStatueFragments.ForFragment(fragment).Offset.Y, "statue native signed Y field");
    }
    /// <summary>Checks the repeated native vertical launch speed entries.</summary>
    private static void VerifyStatueFragmentVelocityField(ISnesAddressSpace rom)
    {
        for (int fragment = 0, parameter = 0; fragment < BombTorizoStatueFragmentDefinitions.FragmentCount; fragment++, parameter += 2)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86a7fb + parameter)),
                BombTorizoStatueFragments.ForFragment(fragment).InitialFallSpeed.RawValue, "statue native constant launch velocity field");
    }
    /// <summary>Checks the repeated native vertical acceleration entries.</summary>
    private static void VerifyStatueFragmentAccelerationField(ISnesAddressSpace rom)
    {
        for (int fragment = 0, parameter = 0; fragment < BombTorizoStatueFragmentDefinitions.FragmentCount; fragment++, parameter += 2)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86a80b + parameter)),
                BombTorizoStatueFragments.ForFragment(fragment).Acceleration.RawValue, "statue native constant acceleration field");
    }
    /// <summary>Forbids production rereads of the five compiled statue-fragment physical tables.</summary>
    private static CartridgeReadGuard NewBombTorizoStatueFragmentReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x86a7ab and < 0x86a81b,
            address => $"Bomb Torizo statue fragment attempted migrated table read ${address:X6}.");
}
