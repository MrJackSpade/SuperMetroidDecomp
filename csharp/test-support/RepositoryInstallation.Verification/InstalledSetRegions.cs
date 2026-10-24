using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// A graphics set's regions as the installed sheets' loader divides them (#1406), for checks that compare a region's
/// selected picture with the character bytes it spans.
/// </summary>
internal static class InstalledSetRegions
{
    /// <summary>The regions of <paramref name="set"/>: its encounters' uploads and its rotting Mother Brain's rows divide them.</summary>
    internal static SetRegions Of(RoomEnemyGraphicsSetDefinition set)
    {
        EnemyDefinitionId[] drawn = [.. set.Records.ToArray().Select(record => record.DefinitionId).Distinct()];
        IReadOnlyList<CorpseRotSimulation> corpses = CorpseRotSimulation.Of(InstalledFixtures.EnemyArtwork.Enemies, InstalledFixtures.EnemyArtwork.Bosses, drawn);
        return new SetRegions(EncounterCharacterPlacements.Catalog.Of(set), [.. corpses.Where(corpse => corpse.Kind is null)]);
    }
}
