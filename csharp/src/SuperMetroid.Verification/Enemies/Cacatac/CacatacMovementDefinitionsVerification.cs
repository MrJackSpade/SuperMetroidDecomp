using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Cacatac;
/// <summary>Compares compiled Cacatac travel distances and initialized movement bounds with the pinned retail table.</summary>
internal static class CacatacMovementDefinitionsVerification
{
    /// <summary>Checks six native distances and wrapped min/max positions while a guard forbids the migrated table read in production initialization.</summary>
    internal static void VerifyCacatacMovementDefinitions(CartridgeImportAddressSpace rom)
    {
        const int distanceTable = 0xa29f36;

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = CacatacBehavior.InitializeCacatac;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Cacatac, slot.Definition);

        foreach (ushort spawnX in new ushort[] { 0, 0x100, ushort.MaxValue })
        {
            for (byte distanceIndex = 0; distanceIndex < 6; distanceIndex++)
            {
                int address = distanceTable + distanceIndex * 2;
                ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                slot.SetSpawnParameter1(EnemyDefinitionId.Cacatac, 0);
                slot.SetSpawnParameter2(EnemyDefinitionId.Cacatac, distanceIndex);
                AssertEqual(native, slot.SpawnSettings<CacatacSpawnSettings>().TravelDistance,
                    $"imported Cacatac travel distance {distanceIndex}");
                slot.Position = slot.Position.WithWholeX(spawnX);
                initialize(slot);
                CacatacEnemyState state = (enemies.CacatacStates[0] ?? throw new InvalidOperationException("Verification requires enemies.CacatacStates[0]."));
                AssertEqual(unchecked((ushort)(spawnX - native)), state.LeftPatrolLimit.X,
                    $"Cacatac minimum X {spawnX:X4}/{distanceIndex}");
                AssertEqual(unchecked((ushort)(spawnX + native)), state.RightPatrolLimit.X,
                    $"Cacatac maximum X {spawnX:X4}/{distanceIndex}");
            }
        }

        AssertThrows<InvalidDataException>(() => slot.SetSpawnParameter2(EnemyDefinitionId.Cacatac, 6),
            "Cacatac import rejects the travel distance past its table");
        AssertThrows<InvalidDataException>(() => slot.SetSpawnParameter2(EnemyDefinitionId.Cacatac, byte.MaxValue),
            "Cacatac import rejects a travel selector that would read adjacent code");

        Console.WriteLine(
            "Cacatac travel distances: six native words and 18 wrapped production initializers pass with table reads forbidden.");
    }

}
