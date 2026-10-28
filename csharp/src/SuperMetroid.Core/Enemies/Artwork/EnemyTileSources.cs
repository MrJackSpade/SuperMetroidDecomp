namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Finds the tile and colour sources the compiled retail graphics sets require.</summary>
internal static class EnemyTileSources
{
    /// <summary>Deduplicates graphics-set enemy definitions with their tile set sizes; the installation checks them against its pinned identity set.</summary>
    internal static System.Collections.ObjectModel.ReadOnlyCollection<EnemyTileSourceDefinition> Build()
    {
        EnemyTileSourceDefinition[] definitions = RoomEnemyGraphicsSetDefinitions.All
            .SelectMany(set => set.Records.ToArray())
            .Select(record => record.DefinitionId).Distinct().Order()
            .Select(definition => {
                RoomEnemyDefinition header = RoomEnemyHeaders.Get(definition);
                return new EnemyTileSourceDefinition(definition, header.TileByteCount);
            }).ToArray();
        if (definitions.Length != EnemyTileArtworkFormat.RetailDefinitionCount)
            throw new InvalidDataException("Compiled enemy artwork sources differ from the installation's definition count.");
        return Array.AsReadOnly(definitions);
    }
}
