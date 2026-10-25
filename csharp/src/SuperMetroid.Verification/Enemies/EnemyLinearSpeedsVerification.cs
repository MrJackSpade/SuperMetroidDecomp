using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks shared enemy linear-speed windows and direct family initialization against cartridge data.</summary>
internal static class EnemyLinearSpeedsVerification
{
    /// <summary>Compares all 517 byte windows and optionally verifies 65 speeds through Cacatac, Owtch, Stoke, and Ripper.</summary>
    internal static void VerifyCompiledLinearEnemySpeeds(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        Func<int, SignedSixteenSixteen> shared = EnemyMath.ReadLinearEnemySpeed;
        // Every possible four-byte window, including odd offsets and windows
        // crossing record boundaries, matches independent cartridge bytes.
        for (int offset = 0; offset <= 516; offset++)
        {
            var expected = SignedSixteenSixteen.FromParts(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08187 + offset))), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08189 + offset)));
            AssertEqual(expected, shared((ushort)offset), "compiled linear native byte window");
            AssertEqual(expected, EnemyLinearSpeedDefinitions.Read(offset), "linear catalog window");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08187 + offset)), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa28187 + offset)), "Ripper bank mirror whole");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08189 + offset)), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa28189 + offset)), "Ripper bank mirror fraction");
        }
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeedDefinitions.Read(-1), "negative linear offset");
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeedDefinitions.Read(517), "partial linear pair");
        AssertThrows<InvalidDataException>(() => shared(ushort.MaxValue), "unclassified linear overread");

        if (definitionsOnly) return;

        var enemies = new RoomEnemySystem(new EnemySession(), definitions: new TestAddressSpace());
        // Only family-specific distance/timer data can be read. No speed bytes
        // are present; Stoke and Ripper do not need any bus for their speed setup.
        var cacatac = new Action<RoomEnemySlot>(CacatacBehavior.InitializeCacatac);
        var owtch = new Action<RoomEnemySlot>(OwtchBehavior.InitializeOwtch);
        var stoke = new Action<RoomEnemySlot>(StokeBehavior.InitializeStoke);
        Action<RipperEnemyState, bool> ripper = RipperBehavior.LoadRipperVelocity;
        var slot = enemies.SlotTable.Slots[0];
        for (int index = 0; index < 65; index++)
        {
            ushort right = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08187 + index * 8)), rightFraction = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa08189 + index * 8));
            ushort left = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa0818b + index * 8)), leftFraction = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa0818d + index * 8));
            var expected = new VerifyCompiledLinearEnemySpeedsCase(right, rightFraction, left, leftFraction);
            slot.SetSpawnParameter1(0);
            slot.SetSpawnParameter2(SnesLittleEndian.Word(0, unchecked((byte)index)));
            slot.Bind(EnemyDefinitionId.Cacatac, slot.Definition);
            cacatac(slot);
            var c = enemies.CacatacStates[0];
            AssertEqual(expected, new VerifyCompiledLinearEnemySpeedsCase((c ?? throw new InvalidOperationException("Verification requires c.")).RightVelocity.WholeWord, c.RightVelocity.Fraction, c.LeftVelocity.WholeWord, c.LeftVelocity.Fraction), "Cacatac native speed setup");
            slot.SetSpawnParameter2((ushort)index);
            slot.Bind(EnemyDefinitionId.Owtch, slot.Definition);
            owtch(slot);
            var o = enemies.OwtchStates[0];
            AssertEqual(expected, new VerifyCompiledLinearEnemySpeedsCase((o ?? throw new InvalidOperationException("Verification requires o.")).RightVelocity.WholeWord, o.RightVelocity.Fraction, o.LeftVelocity.WholeWord, o.LeftVelocity.Fraction), "Owtch native speed setup");
            slot.Bind(EnemyDefinitionId.Stoke, slot.Definition);
            stoke(slot);
            var s = enemies.StokeStates[0];
            AssertEqual(expected, new VerifyCompiledLinearEnemySpeedsCase((s ?? throw new InvalidOperationException("Verification requires s.")).RightVelocity.WholeWord, s.RightVelocity.Fraction, s.LeftVelocity.WholeWord, s.LeftVelocity.Fraction), "Stoke native speed setup");
            var ripperState = new RipperEnemyState(new RipperExtensionWords()) { SpeedTableByteOffset = index * 8 };
            ripper(ripperState, true);
            AssertEqual(new EnemyLinearSpeedsVerifyCompiledLinearEnemySpeedsCase(right, rightFraction), new EnemyLinearSpeedsVerifyCompiledLinearEnemySpeedsCase(ripperState.XVelocity.WholeWord, ripperState.XVelocity.Fraction), "Ripper right speed");
            ripper(ripperState, false);
            AssertEqual(new EnemyLinearSpeedsVerifyCompiledLinearEnemySpeedsCase(left, leftFraction), new EnemyLinearSpeedsVerifyCompiledLinearEnemySpeedsCase(ripperState.XVelocity.WholeWord, ripperState.XVelocity.Fraction), "Ripper reversal speed");
        }
        Console.WriteLine("Compiled linear speeds: 517 byte windows, bank mirror and all 65 speeds in four direct family callers match the cartridge without speed-table ROM reads.");
    }
}
