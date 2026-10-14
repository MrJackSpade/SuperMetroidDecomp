using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks shared enemy linear-speed windows and direct family initialization against cartridge data.</summary>
internal static class EnemyLinearSpeedsVerification
{
    /// <summary>Compares all 517 byte windows and optionally verifies 65 speeds through Cacatac, Owtch, Stoke, and Ripper.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled linear enemy speeds")]
    internal static void VerifyCompiledLinearEnemySpeeds(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        Func<ushort, (short Whole, ushort Fraction)> shared = EnemyMath.ReadLinearEnemySpeed;
        // Every possible four-byte window, including odd offsets and windows
        // crossing record boundaries, matches independent cartridge bytes.
        for (int offset = 0; offset <= 516; offset++)
        {
            var expected = (unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08187 + offset))), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08189 + offset)));
            AssertEqual(expected, shared((ushort)offset), "compiled linear native byte window");
            AssertEqual(expected, EnemyLinearSpeedDefinitions.Read(offset), "linear catalog window");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08187 + offset)), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa28187 + offset)), "Ripper bank mirror whole");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08189 + offset)), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa28189 + offset)), "Ripper bank mirror fraction");
        }
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeedDefinitions.Read(-1), "negative linear offset");
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeedDefinitions.Read(517), "partial linear pair");
        AssertThrows<InvalidDataException>(() => shared(ushort.MaxValue), "unclassified linear overread");

        if (definitionsOnly) return;

        var enemies = new RoomEnemySystem();
        // Only family-specific distance/timer data can be read. No speed bytes
        // are present; Stoke and Ripper do not need any bus for their speed setup.
        enemies.BindFixtureBus(new TestAddressSpace());
        var cacatac = new Action<RoomEnemySlot>(enemies.CacatacBehavior.InitializeCacatac);
        var owtch = new Action<RoomEnemySlot>(enemies.OwtchBehavior.InitializeOwtch);
        var stoke = new Action<RoomEnemySlot>(enemies.StokeBehavior.InitializeStoke);
        Action<RoomEnemySlot, bool> ripper = RipperBehavior.LoadRipperVelocity;
        var slot = enemies.SlotTable.Slots[0];
        for (int index = 0; index < 65; index++)
        {
            ushort right = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08187 + index * 8)), rightFraction = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08189 + index * 8));
            ushort left = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa0818b + index * 8)), leftFraction = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa0818d + index * 8));
            var expected = (right, rightFraction, left, leftFraction);
            slot.Parameter1 = 0;
            slot.Parameter2 = SnesLittleEndian.Word(0, unchecked((byte)index));
            cacatac(slot);
            var c = enemies.CacatacStates[0];
            AssertEqual(expected, ((c ?? throw new InvalidOperationException("Verification requires c.")).RightVelocity, c.RightSubvelocity, c.LeftVelocity, c.LeftSubvelocity), "Cacatac native speed setup");
            slot.Parameter2 = (ushort)index;
            owtch(slot);
            var o = enemies.OwtchStates[0];
            AssertEqual(expected, ((o ?? throw new InvalidOperationException("Verification requires o.")).RightVelocity, o.RightSubvelocity, o.LeftVelocity, o.LeftSubvelocity), "Owtch native speed setup");
            stoke(slot);
            var s = enemies.StokeStates[0];
            AssertEqual(expected, ((s ?? throw new InvalidOperationException("Verification requires s.")).RightVelocity, s.RightSubvelocity, s.LeftVelocity, s.LeftSubvelocity), "Stoke native speed setup");
            slot.VariableE = (ushort)(index * 8);
            ripper(slot, true);
            AssertEqual((right, rightFraction), (slot.VariableD, slot.VariableC), "Ripper right speed");
            ripper(slot, false);
            AssertEqual((left, leftFraction), (slot.VariableD, slot.VariableC), "Ripper reversal speed");
        }
        Console.WriteLine("Compiled linear speeds: 517 byte windows, bank mirror and all 65 speeds in four direct family callers match the cartridge without speed-table ROM reads.");
    }
}
