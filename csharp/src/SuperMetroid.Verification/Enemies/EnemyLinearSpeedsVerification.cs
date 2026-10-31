using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the shared linear speed rows and direct family initialization against cartridge data.</summary>
internal static class EnemyLinearSpeedsVerification
{
    /// <summary>Compares all 65 rows with both cartridge copies and optionally verifies them through Cacatac, Owtch, Stoke, and Ripper.</summary>
    internal static void VerifyCompiledLinearEnemySpeeds(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        for (int row = 0; row < EnemyLinearSpeedDefinitions.RowCount; row++)
        {
            AssertEqual(EnemySpeedTableCodec.LinearRow(cartridge, EnemySpeedTableCodec.LinearTable, row), EnemyLinearSpeeds.Row(row), "linear speed row");
            AssertEqual(EnemySpeedTableCodec.LinearRow(cartridge, EnemySpeedTableCodec.LinearRipperCopy, row), EnemyLinearSpeeds.Row(row), "Ripper bank copy row");
        }
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeeds.Row(-1), "negative linear row");
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeeds.Row(EnemyLinearSpeedDefinitions.RowCount), "linear row past the table");

        if (definitionsOnly) return;

        var enemies = new RoomEnemySystem(new EnemySession(), definitions: new TestAddressSpace());
        // Only family-specific distance/timer data can be read. No speed bytes
        // are present; Stoke and Ripper do not need any bus for their speed setup.
        var cacatac = new Action<RoomEnemySlot>(CacatacBehavior.InitializeCacatac);
        var owtch = new Action<RoomEnemySlot>(OwtchBehavior.InitializeOwtch);
        var stoke = new Action<RoomEnemySlot>(StokeBehavior.InitializeStoke);
        Action<RipperEnemyState, bool> ripper = RipperBehavior.LoadRipperVelocity;
        var slot = enemies.SlotTable.Slots[0];
        for (int index = 0; index < EnemyLinearSpeedDefinitions.RowCount; index++)
        {
            EnemyLinearSpeedRow expected = EnemySpeedTableCodec.LinearRow(cartridge, EnemySpeedTableCodec.LinearTable, index);
            slot.SetSpawnParameter1(0);
            slot.SetSpawnParameter2(LittleEndianBytes.Word(0, unchecked((byte)index)));
            slot.Bind(EnemyDefinitionId.Cacatac, slot.Definition);
            cacatac(slot);
            var c = enemies.CacatacStates[0] ?? throw new InvalidOperationException("Verification requires c.");
            AssertEqual(expected, new EnemyLinearSpeedRow(c.RightVelocity, c.LeftVelocity), "Cacatac native speed setup");
            slot.SetSpawnParameter2((ushort)index);
            slot.Bind(EnemyDefinitionId.Owtch, slot.Definition);
            owtch(slot);
            var o = enemies.OwtchStates[0] ?? throw new InvalidOperationException("Verification requires o.");
            AssertEqual(expected, new EnemyLinearSpeedRow(o.RightVelocity, o.LeftVelocity), "Owtch native speed setup");
            slot.Bind(EnemyDefinitionId.Stoke, slot.Definition);
            stoke(slot);
            var s = enemies.StokeStates[0] ?? throw new InvalidOperationException("Verification requires s.");
            AssertEqual(expected, new EnemyLinearSpeedRow(s.RightVelocity, s.LeftVelocity), "Stoke native speed setup");
            var ripperState = new RipperEnemyState(new RipperExtensionWords()) { Speeds = EnemyLinearSpeeds.Row(index) };
            ripper(ripperState, true);
            AssertEqual(expected.Positive, ripperState.XVelocity, "Ripper right speed");
            ripper(ripperState, false);
            AssertEqual(expected.Negative, ripperState.XVelocity, "Ripper reversal speed");
        }
        Console.WriteLine("Compiled linear speeds: all 65 rows match both cartridge copies and four direct family callers without speed-table ROM reads.");
    }
}
