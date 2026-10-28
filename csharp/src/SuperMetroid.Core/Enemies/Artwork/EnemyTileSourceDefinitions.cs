namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Immutable required tile/color identities from all compiled retail graphics sets.</summary>
public static class EnemyTileSourceDefinitions
{
    /// <summary>Every distinct graphics-set definition, with the bytes of its tile set.</summary>
    public static IReadOnlyList<EnemyTileSourceDefinition> All { get; } = Build();

    /// <summary>Deduplicates graphics-set enemy definitions with their tile set sizes; the installation checks them against its pinned identity set.</summary>
    private static System.Collections.ObjectModel.ReadOnlyCollection<EnemyTileSourceDefinition> Build()
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
