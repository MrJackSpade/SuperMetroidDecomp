using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Verification access at the import boundary (#1599): the installed artwork of the rooms that draw under one of the
/// import's graphics sets, for checks that compare the installed variants against what the import composes for a set.
/// Gameplay knows no set; it draws each population in the variants installed for it.
/// </summary>
internal static class InstalledEnemySheetsAccess
{
    /// <summary>The installed artwork by the import's graphics sets.</summary>
    extension(InstalledEnemySheets self)
    {
        /// <summary>
        /// The installed artwork of the rooms that draw under <paramref name="set"/>: a retail population's that draws under
        /// it, or the standard artwork for the session start's.
        /// </summary>
        /// <exception cref="InvalidDataException">No retail room draws under the set, so nothing is installed for it.</exception>
        internal PopulationArtwork Under(RoomEnemyGraphicsSetDefinition set) =>
            set == RoomEnemyGraphicsSetDefinitions.SessionStart ? self.Standard
                : FixturePopulationSets.RetailDrawing(set) is { } population ? self.For(population)
                : throw new InvalidDataException($"No retail room draws under graphics set {set.Name}, so no artwork is installed for it.");

        /// <summary>The installed sheet of the variant <paramref name="enemy"/> draws in under <paramref name="set"/>.</summary>
        internal SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy) => self.Under(set).Enemy(enemy).Sheet;

        /// <summary>The installed pictures of the variant <paramref name="owner"/>'s spawned objects draw in under <paramref name="set"/>.</summary>
        internal SpawnedObjectSheet Spawned(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner) => self.Under(set).Spawned(owner).Sheet;

        /// <summary>
        /// The graphics sets rooms draw under whose artwork the installation holds, each with the artwork drawing under it:
        /// the session start's standard artwork and each set a retail population draws under.
        /// </summary>
        internal IEnumerable<KeyValuePair<RoomEnemyGraphicsSetDefinition, PopulationArtwork>> BySet =>
            RoomEnemyGraphicsSetLists.DrawnUnder.Select(set => KeyValuePair.Create(set, self.Under(set)));

        /// <summary>Every installed enemy variant once, with the enemy it shows, in the order of the variants' names.</summary>
        internal IEnumerable<KeyValuePair<EnemyDefinitionId, EnemyArtworkVariant<SpriteSheet>>> EnemyVariants =>
            self.populations.Values.Prepend(self.Standard).SelectMany(artwork => artwork.enemies)
                .DistinctBy(entry => entry.Value.Name, StringComparer.Ordinal).OrderBy(entry => entry.Value.Name, StringComparer.Ordinal);
    }
}
