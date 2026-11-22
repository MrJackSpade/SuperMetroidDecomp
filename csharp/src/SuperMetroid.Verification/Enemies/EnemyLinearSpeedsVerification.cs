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
        IImportCartridgeSource cartridge = rom;
        for (int row = 0; row < EnemyLinearSpeedTable.RowCount; row++)
        {
            SignedSixteenSixteen speed = EnemyLinearSpeedTable.Speed(row);
            foreach (int table in (int[])[EnemySpeedTableCodec.LinearTable, EnemySpeedTableCodec.LinearRipperCopy])
            {
                (SignedSixteenSixteen positive, SignedSixteenSixteen negative) = EnemySpeedTableCodec.LinearRow(cartridge, table, row);
                AssertEqual(speed, positive, $"linear speed row {row} at {table:X6}");
                AssertEqual(speed.Toward(HorizontalDirection.Left), negative, $"linear speed row {row} at {table:X6}: stored negation is the speed taken leftward at use");
            }
        }
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeedTable.Speed(-1), "negative linear row");
        AssertThrows<InvalidDataException>(() => EnemyLinearSpeedTable.Speed(EnemyLinearSpeedTable.RowCount), "linear row past the table");

        if (definitionsOnly) return;

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: new TestAddressSpace());
        // Only family-specific distance/timer data can be read. No speed bytes
        // are present; Ripper does not need any bus for its speed setup.
        var cacatac = new Action<RoomEnemyPlace>(fixturePlace => CacatacBehavior.InitializeCacatac(fixturePlace.HeldOccupant().SpawnSettings<CacatacSpawnSettings>(), fixturePlace));
        var owtch = new Action<RoomEnemyPlace>(fixturePlace => OwtchBehavior.InitializeOwtch(fixturePlace.HeldOccupant().SpawnSettings<OwtchSpawnSettings>(), fixturePlace));
        var place = enemies.Places.InUpdateOrder[0];
        for (int index = 0; index < EnemyLinearSpeedTable.RowCount; index++)
        {
            (SignedSixteenSixteen positive, SignedSixteenSixteen negative) = EnemySpeedTableCodec.LinearRow(cartridge, EnemySpeedTableCodec.LinearTable, index);
            place.SetSpawnParameter1(EnemyDefinitionId.Cacatac, 0);
            place.SetSpawnParameter2(EnemyDefinitionId.Cacatac, LittleEndianBytes.Word(0, unchecked((byte)index)));
            place.Bind(EnemyDefinitionId.Cacatac, place.KeptDefinition);
            cacatac(place);
            SignedSixteenSixteen cacatacSpeed = place.HeldOccupant().SpawnSettings<CacatacSpawnSettings>().Speed;
            AssertEqual((positive, negative), (cacatacSpeed, -cacatacSpeed), "Cacatac spawn speed, negated for walking left");
            place.SetSpawnParameter2(EnemyDefinitionId.Owtch, (ushort)index);
            place.Bind(EnemyDefinitionId.Owtch, place.KeptDefinition);
            owtch(place);
            SignedSixteenSixteen owtchSpeed = place.HeldOccupant().SpawnSettings<OwtchSpawnSettings>().Speed;
            AssertEqual((positive, negative), (owtchSpeed, -owtchSpeed), "Owtch spawn speed, negated for moving left");
            // The Ripper flies its spawn speed toward its heading (#2450).
            place.SetSpawnSettings(new RipperSpawnSettings(EnemyLinearSpeedTable.Speed(index), HorizontalDirection.Right));
            var ripperState = new RipperEnemyState() { Heading = HorizontalDirection.Right };
            SignedSixteenSixteen ripperSpeed = place.HeldOccupant().SpawnSettings<RipperSpawnSettings>().Speed;
            AssertEqual(positive, RipperBehavior.FlyingVelocity(ripperSpeed, ripperState.Heading), "Ripper right speed");
            RipperBehavior.Reverse(ripperState);
            AssertEqual(negative, RipperBehavior.FlyingVelocity(ripperSpeed, ripperState.Heading), "Ripper reversal speed");
        }
        Console.WriteLine("Compiled linear speeds: all 65 rows match both cartridge copies and four direct family callers without speed-table ROM reads.");
    }
}
