using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Checks sixteen Bomb Torizo statue fragments against native physical tables and real projectile allocation.</summary>
internal static class BombTorizoStatueFragmentDefinitionsVerification
{
    /// <summary>Compares five fields, rejects odd or out-of-range parameters, and checks spawned fragment position and motion.</summary>
    internal static void VerifyBombTorizoStatueFragmentDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyStatueFragmentProgramField), () => VerifyStatueFragmentProgramField(rom));
        Suite(nameof(VerifyStatueFragmentXField), () => VerifyStatueFragmentXField(rom));
        Suite(nameof(VerifyStatueFragmentYField), () => VerifyStatueFragmentYField(rom));
        Suite(nameof(VerifyStatueFragmentVelocityField), () => VerifyStatueFragmentVelocityField(rom));
        Suite(nameof(VerifyStatueFragmentAccelerationField), () => VerifyStatueFragmentAccelerationField(rom));
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (ushort parameter = 1; parameter < 32; parameter += 2)
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoStatueFragmentDefinitions.ForParameter(parameter), "statue rejects every odd parameter in the native range");
        foreach (ushort parameter in new ushort[] {32,33,0x100,0x101,0x7ffe,0x8000,0xfffe,0xffff})
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoStatueFragmentDefinitions.ForParameter(parameter), "statue rejects parameters beyond the sixteen fragments");
        var guarded = NewBombTorizoStatueFragmentReadGuard(rom);
        for (ushort index = 0; index < 16; index++)
        {
            ushort parameter = unchecked((ushort)(index * 2));
            BombTorizoStatueFragmentDefinition expected =
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter);
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            RoomEnemyProjectileSlot projectile =
                enemies.BombTorizoBehavior.SpawnBombTorizoStatueBreakingProjectile(
                    new BombTorizoStatueProjectileRequest(
                        BombTorizoStatueFragmentDefinitions.ProjectileDefinition,
                        parameter,
                        PlmBlock: new(10, 20))) ??
                throw new InvalidDataException(
                    $"Bomb Torizo statue fragment {index} failed to allocate.");

            AssertEqual(RoomEnemyProjectileKind.BombTorizoStatueBreaking,
                projectile.Kind,
                $"Bomb Torizo statue fragment {index} projectile kind");
            AssertEqual<TorizoProjectileProgram?>(expected.Program, (projectile.Animation as TorizoProjectileAnimation)?.Cursor.Program,
                $"Bomb Torizo statue fragment {index} production program");
            AssertEqual(unchecked((ushort)(10 * 16 + expected.Offset.X)),
                projectile.Position.X.Whole,
                $"Bomb Torizo statue fragment {index} production X");
            AssertEqual(unchecked((ushort)(20 * 16 + expected.Offset.Y)),
                projectile.Position.Y.Whole,
                $"Bomb Torizo statue fragment {index} production Y");
            AssertEqual(expected.YVelocity, projectile.Velocity.Y.RawValue,
                $"Bomb Torizo statue fragment {index} production velocity");
            AssertEqual(expected.Acceleration, projectile.Variable1,
                $"Bomb Torizo statue fragment {index} production acceleration");
        }

        Console.WriteLine(
            "Bomb Torizo statue fragments: 56 native source words and all sixteen real room-graphics projectile allocations pass with the five physical tables forbidden.");
    }

    /// <summary>Checks the instruction list selected by each native even fragment parameter.</summary>
    private static void VerifyStatueFragmentProgramField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86a7ab + parameter)),
                TorizoNativeListAddresses.ProjectileEntry(BombTorizoStatueFragmentDefinitions.ForParameter(parameter).Program), "statue native program field");
    }
    /// <summary>Checks signed horizontal offsets, including the mirrored fragment set.</summary>
    private static void VerifyStatueFragmentXField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86a7cb + parameter))),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).Offset.X, "statue native signed X field including mirrored set");
    }
    /// <summary>Checks the eight-row-wrapped vertical offset table.</summary>
    private static void VerifyStatueFragmentYField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86a7eb + (parameter & 15)))),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).Offset.Y, "statue native signed Y field with eight-row wrap");
    }
    /// <summary>Checks the repeated native vertical launch speed entries.</summary>
    private static void VerifyStatueFragmentVelocityField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86a7fb + (parameter & 15))),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).YVelocity, "statue native constant launch velocity field");
    }
    /// <summary>Checks the repeated native vertical acceleration entries.</summary>
    private static void VerifyStatueFragmentAccelerationField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86a80b + (parameter & 15))),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).Acceleration, "statue native constant acceleration field");
    }
    /// <summary>Forbids production rereads of the five compiled statue-fragment physical tables.</summary>
    private static CartridgeReadGuard NewBombTorizoStatueFragmentReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x86a7ab and < 0x86a81b,
            address => $"Bomb Torizo statue fragment attempted migrated table read ${address:X6}.");
}
