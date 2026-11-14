using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the shared linear speed table and direct family initialization against cartridge data.</summary>
internal static class EnemyLinearSpeedsVerification
{
    /// <summary>
    /// Compares all 65 rows with both cartridge copies, the stored negative speed with the positive speed negated at use
    /// (#1804), and optionally verifies them through Cacatac, Owtch and Ripper.
    /// </summary>
    internal static void VerifyCompiledLinearEnemySpeeds(CartridgeImportAddressSpace rom, bool definitionsOnly = false)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        for (int row = 0; row < EnemyLinearSpeedTable.RowCount; row++)
        {
            SignedSixteenSixteen speed = EnemyLinearSpeedTable.Speed(row);
            foreach (int table in (int[])[EnemySpeedTableCodec.LinearTable, EnemySpeedTableCodec.LinearRipperCopy])
            {
                (SignedSixteenSixteen positive, SignedSixteenSixteen negative) = EnemySpeedTableCodec.LinearRow(cartridge, table, row);
                AssertEqual(speed, positive, $"linear speed row {row} at {table:X6}");
                AssertEqual(new EnemyLinearSpeed(speed).Negative, negative, $"linear speed row {row} at {table:X6}: stored negation is the speed negated at use");
            }
        }
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeedTable.Speed(-1), "negative linear row");
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeedTable.Speed(EnemyLinearSpeedTable.RowCount), "linear row past the table");

        if (definitionsOnly) return;

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: new TestAddressSpace());
        // Only family-specific distance/timer data can be read. No speed bytes
        // are present; Ripper does not need any bus for its speed setup.
        var cacatac = new Action<RoomEnemySlot>(CacatacBehavior.InitializeCacatac);
        var owtch = new Action<RoomEnemySlot>(OwtchBehavior.InitializeOwtch);
        Action<RipperEnemyState, bool> ripper = RipperBehavior.LoadRipperVelocity;
        var slot = enemies.SlotTable.Slots[0];
        for (int index = 0; index < EnemyLinearSpeedTable.RowCount; index++)
        {
            (SignedSixteenSixteen positive, SignedSixteenSixteen negative) = EnemySpeedTableCodec.LinearRow(cartridge, EnemySpeedTableCodec.LinearTable, index);
            slot.SetSpawnParameter1(EnemyDefinitionId.Cacatac, 0);
            slot.SetSpawnParameter2(EnemyDefinitionId.Cacatac, LittleEndianBytes.Word(0, unchecked((byte)index)));
            slot.Bind(EnemyDefinitionId.Cacatac, slot.Definition);
            cacatac(slot);
            var c = enemies.CacatacStates[0] ?? throw new InvalidOperationException("Verification requires c.");
            AssertEqual((positive, negative), (c.Speed, -c.Speed), "Cacatac native speed setup, negated for walking left");
            slot.SetSpawnParameter2(EnemyDefinitionId.Owtch, (ushort)index);
            slot.Bind(EnemyDefinitionId.Owtch, slot.Definition);
            owtch(slot);
            var o = enemies.OwtchStates[0] ?? throw new InvalidOperationException("Verification requires o.");
            AssertEqual((positive, negative), (o.RightVelocity, o.LeftVelocity), "Owtch native speed setup");
            var ripperState = new RipperEnemyState() { Speeds = new EnemyLinearSpeed(EnemyLinearSpeedTable.Speed(index)) };
            ripper(ripperState, true);
            AssertEqual(positive, ripperState.XVelocity, "Ripper right speed");
            ripper(ripperState, false);
            AssertEqual(negative, ripperState.XVelocity, "Ripper reversal speed");
        }
        Console.WriteLine("Compiled linear speeds: all 65 rows match both cartridge copies and four direct family callers without speed-table ROM reads.");
    }
}
