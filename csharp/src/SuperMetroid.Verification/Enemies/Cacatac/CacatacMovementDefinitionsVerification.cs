using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Enemies.Cacatac;
/// <summary>Compares compiled Cacatac travel distances and initialized movement bounds with the pinned retail table.</summary>
internal static class CacatacMovementDefinitionsVerification
{
    /// <summary>Checks six native distances and the min/max positions they set while a guard forbids the migrated table read in production initialization.</summary>
    internal static void VerifyCacatacMovementDefinitions(CartridgeImportAddressSpace rom)
    {
        const int distanceTable = 0xa29f36;

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemyPlace> initialize = fixturePlace => CacatacBehavior.InitializeCacatac(fixturePlace.HeldOccupant(), fixturePlace);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Cacatac, place.Definition);

        // Spawn columns from the shipped placements' range: the left edge's nearest, a middle one and the farthest right.
        foreach (int spawnX in new[] { 72, 0x100, 3240 })
        {
            for (byte distanceIndex = 0; distanceIndex < 6; distanceIndex++)
            {
                int address = distanceTable + distanceIndex * 2;
                ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                place.SetSpawnParameter1(EnemyDefinitionId.Cacatac, 0);
                place.SetSpawnParameter2(EnemyDefinitionId.Cacatac, distanceIndex);
                AssertEqual(native, place.HeldOccupant().SpawnSettings<CacatacSpawnSettings>().TravelDistance,
                    $"imported Cacatac travel distance {distanceIndex}");
                place.Position = place.Position.WithWholeX(spawnX);
                initialize(place);
                CacatacEnemyState state = (enemies.CacatacStates[0] ?? throw new InvalidOperationException("Verification requires enemies.CacatacStates[0]."));
                AssertEqual(spawnX - native, state.LeftPatrolX,
                    $"Cacatac minimum X {spawnX}/{distanceIndex}");
                AssertEqual(spawnX + native, state.RightPatrolX,
                    $"Cacatac maximum X {spawnX}/{distanceIndex}");
            }
        }

        AssertThrows<InvalidDataException>(() => place.SetSpawnParameter2(EnemyDefinitionId.Cacatac, 6),
            "Cacatac import rejects the travel distance past its table");
        AssertThrows<InvalidDataException>(() => place.SetSpawnParameter2(EnemyDefinitionId.Cacatac, byte.MaxValue),
            "Cacatac import rejects a travel selector that would read adjacent code");

        Console.WriteLine(
            "Cacatac travel distances: six native words and 18 production initializers pass with table reads forbidden.");
    }

}
