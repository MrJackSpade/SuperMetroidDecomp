namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Immutable required tile/color identities from all compiled retail graphics sets.</summary>
public static class EnemyTileSourceDefinitions
{
    /// <summary>Every distinct bank-$A0 graphics-set definition, with its native source and masked byte length.</summary>
    public static IReadOnlyList<EnemyTileSourceDefinition> All { get; } = Build();

    /// <summary>Deduplicates graphics-set enemy definitions and extracts masked tile lengths; the installation checks them against its pinned identity set.</summary>
    private static System.Collections.ObjectModel.ReadOnlyCollection<EnemyTileSourceDefinition> Build()
    {
        EnemyTileSourceDefinition[] definitions = RoomEnemyGraphicsSetDefinitions.All
            .SelectMany(set => set.Records.ToArray())
            .Select(record => record.DefinitionId).Distinct().Order()
            .Select(definition => {
                RoomEnemyDefinition header = RoomEnemyDefinitionCatalog.Get(definition);
                return new EnemyTileSourceDefinition(definition, header.TileDataAddress,
                    header.TileDataSize & EnemyTileArtworkFormat.TileByteCountMask);
            }).ToArray();
        if (definitions.Length != EnemyTileArtworkFormat.RetailDefinitionCount)
            throw new InvalidDataException("Compiled enemy artwork sources differ from the installation's definition count.");
        return Array.AsReadOnly(definitions);
    }
}
