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

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: new TestAddressSpace());
        // Only family-specific distance/timer data can be read. No speed bytes
        // are present; Ripper does not need any bus for its speed setup.
        var cacatac = new Action<RoomEnemyPlace>(CacatacBehavior.InitializeCacatac);
        var owtch = new Action<RoomEnemyPlace>(OwtchBehavior.InitializeOwtch);
        var place = enemies.Places.InUpdateOrder[0];
        for (int index = 0; index < EnemyLinearSpeedTable.RowCount; index++)
        {
            (SignedSixteenSixteen positive, SignedSixteenSixteen negative) = EnemySpeedTableCodec.LinearRow(cartridge, EnemySpeedTableCodec.LinearTable, index);
            place.SetSpawnParameter1(EnemyDefinitionId.Cacatac, 0);
            place.SetSpawnParameter2(EnemyDefinitionId.Cacatac, LittleEndianBytes.Word(0, unchecked((byte)index)));
            place.Bind(EnemyDefinitionId.Cacatac, place.Definition);
            cacatac(place);
            var c = enemies.CacatacStates[0] ?? throw new InvalidOperationException("Verification requires c.");
            AssertEqual((positive, negative), (c.Speed, -c.Speed), "Cacatac native speed setup, negated for walking left");
            place.SetSpawnParameter2(EnemyDefinitionId.Owtch, (ushort)index);
            place.Bind(EnemyDefinitionId.Owtch, place.Definition);
            owtch(place);
            SignedSixteenSixteen owtchSpeed = place.SpawnSettings<OwtchSpawnSettings>().Speed;
            AssertEqual((positive, negative), (owtchSpeed, -owtchSpeed), "Owtch spawn speed, negated for moving left");
            // The Ripper flies its spawn speed toward its heading (#2450).
            place.SetSpawnSettings(new RipperSpawnSettings(EnemyLinearSpeedTable.Speed(index), HorizontalDirection.Right));
            var ripperState = new RipperEnemyState() { Heading = HorizontalDirection.Right };
            AssertEqual(positive, RipperBehavior.FlyingVelocity(place, ripperState), "Ripper right speed");
            RipperBehavior.Reverse(ripperState);
            AssertEqual(negative, RipperBehavior.FlyingVelocity(place, ripperState), "Ripper reversal speed");
        }
        Console.WriteLine("Compiled linear speeds: all 65 rows match both cartridge copies and four direct family callers without speed-table ROM reads.");
    }
}
