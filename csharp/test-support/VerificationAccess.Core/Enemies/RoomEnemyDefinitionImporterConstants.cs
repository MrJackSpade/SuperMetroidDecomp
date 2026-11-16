using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="RoomEnemyDefinitionImporter"/> that only verification reads.</summary>
internal static class RoomEnemyDefinitionImporterConstants
{
    /// <summary>
    /// The bytes of the tile set a header names (#1513): the size the import lays sheets out by. Which members upload at a
    /// fixed block is the import's tile set catalogue's.
    /// </summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="pointer">The enemy definition, whose header the import's header table locates.</param>
    internal static EnemyHeaderTileSet TileSet(ISnesAddressSpace bus, EnemyDefinitionId pointer) =>
        RoomEnemyDefinitionImporter.TileSetAt(bus, EnemyDefinitionIds.HeaderPointer(pointer));
    /// <summary>The long address of the planar tile data a header names: where the import reads the enemy's tile set.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="pointer">The enemy definition, whose header the import's header table locates.</param>
    internal static int TileDataSource(ISnesAddressSpace bus, EnemyDefinitionId pointer) =>
        RoomEnemyDefinitionImporter.TileDataSourceAt(bus, EnemyDefinitionIds.HeaderPointer(pointer));
    /// <summary>The long address of the sixteen-color palette a header names in its enemy's bank.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="pointer">The enemy definition, whose header the import's header table locates.</param>
    internal static int PaletteAddress(ISnesAddressSpace bus, EnemyDefinitionId pointer) =>
        RoomEnemyDefinitionImporter.PaletteAddressAt(bus, EnemyDefinitionIds.HeaderPointer(pointer));
}
