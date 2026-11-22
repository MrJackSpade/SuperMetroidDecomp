using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Finds the tile and colour sources the compiled retail graphics sets require.</summary>
internal static partial class EnemyTileSources
{
    /// <summary>Deduplicates graphics-set enemy definitions with their tile set sizes; the installation checks them against its pinned identity set.</summary>
    internal static System.Collections.ObjectModel.ReadOnlyCollection<EnemyTileSourceDefinition> Build()
    {
        HashSet<EnemyDefinitionId> loaded = [.. Records().Select(record => record.DefinitionId).OfType<EnemyDefinitionId>()];
        EnemyTileSourceDefinition[] definitions = Ensure.EveryMemberOnce(EnemyDefinitionIdKeys.ListedOrder).Where(loaded.Contains)
            .Select(definition => new EnemyTileSourceDefinition(definition, EnemyTileSetDefinitions.ByteCounts[definition])).ToArray();
        if (definitions.Length != EnemyTileArtworkFormat.RetailDefinitionCount)
            throw new InvalidDataException("Compiled enemy artwork sources differ from the installation's definition count.");
        return Array.AsReadOnly(definitions);
    }

    /// <summary>Deduplicates the artwork the graphics sets load for no enemy (#1686).</summary>
    internal static System.Collections.ObjectModel.ReadOnlyCollection<UnplacedEnemyArtwork> BuildUnplaced()
    {
        HashSet<UnplacedEnemyArtwork> loaded = [.. Records().Select(record => record.Unplaced).OfType<UnplacedEnemyArtwork>()];
        UnplacedEnemyArtwork[] artworks = [.. Ensure.EveryMemberOnce(UnplacedEnemyArtworkKeys.ListedOrder).Where(loaded.Contains)];
        if (artworks.Length != EnemyTileArtworkFormat.RetailUnplacedArtworkCount)
            throw new InvalidDataException("Compiled unplaced enemy artwork differs from the installation's count.");
        return Array.AsReadOnly(artworks);
    }

    /// <summary>Every record of every compiled retail graphics set.</summary>
    private static IEnumerable<RoomEnemyGraphicsSetMember> Records() =>
        RoomEnemyGraphicsSetLists.Retail.SelectMany(set => set.Records.ToArray());
}
