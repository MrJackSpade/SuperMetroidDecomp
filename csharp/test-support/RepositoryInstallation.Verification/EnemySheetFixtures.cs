using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): the enemy sheets a fixture's enemies draw from. The installation's enemy artwork draws
/// from the installed sheets; a fixture's own artwork gets sheets composed from it, a graphics set at a time as its
/// rooms draw under them, as an installation of that artwork would hold. Any set composes, a fixture's own among them. Composed frames are registered so OAM comparisons
/// still apply to them.
/// </summary>
internal static class EnemySheetFixtures
{
    /// <summary>The installation's stock artwork, whose enemies a fixture's replace.</summary>
    private static readonly Lazy<EnemySheetInputs> inputs = new(() => SharedFixtureTiming.Load("Fixture: enemy sheet inputs",
        () => EnemySheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory)));

    /// <summary>The installation's stock artwork the import composes enemy sheets from, loaded once per process.</summary>
    internal static EnemySheetInputs Inputs => inputs.Value;

    /// <summary>Sheets already composed, by enemy artwork.</summary>
    private static readonly ConditionalWeakTable<EnemyTileSourceArtwork, ComposedOnDemand> composed = new();

    /// <summary>
    /// Sheets already composed, by the content of the artwork they were composed from: artwork loaded again with the same
    /// content draws from the sheets composed for the first, for as long as artwork of that content lives.
    /// </summary>
    private static readonly Dictionary<string, WeakReference<ComposedOnDemand>> composedByContent = new(StringComparer.Ordinal);

    /// <summary>The content of the installation's enemy artwork, whose enemies draw from the installed sheets.</summary>
    private static readonly Lazy<string> stockContent = new(() => inputs.Value.Enemies.ContentIdentity);

    /// <summary>Each graphics set's characters as the import composes its pictures from them, by set, composed once.</summary>
    private static readonly Dictionary<RoomEnemyGraphicsSetDefinition, SetCharacters> characters = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// The OBJ characters graphics set <paramref name="set"/> leaves once its encounters show each look of
    /// <paramref name="shown"/> (#1538): the room load's characters with every look of each up to the one shown copied in
    /// order, from the installation's artwork, which a frame's picture of those looks paints like.
    /// </summary>
    internal static SpriteArtwork LivePage(RoomEnemyGraphicsSetDefinition set, IReadOnlyList<EncounterLook> shown) =>
        Characters(set).Page(shown);

    /// <summary>
    /// The OBJ characters graphics set <paramref name="set"/> leaves once <paramref name="encounter"/> shows its look at
    /// <paramref name="look"/> (#1538), whose looks gameplay may never show: the room load's characters with every look up
    /// to it copied in order, which a frame drawn in that look paints like.
    /// </summary>
    internal static SpriteArtwork LivePage(RoomEnemyGraphicsSetDefinition set, ComposedEncounter encounter, int look) =>
        Characters(set).Page(encounter, look);

    /// <summary>The characters the import composes graphics set <paramref name="set"/>'s pictures from, composed once.</summary>
    private static SetCharacters Characters(RoomEnemyGraphicsSetDefinition set)
    {
        ArgumentNullException.ThrowIfNull(set);
        lock (characters)
        {
            if (!characters.TryGetValue(set, out SetCharacters? composedCharacters))
                characters.Add(set, composedCharacters = EnemySheetComposer.Load(
                    inputs.Value with { SetMembers = FixtureGraphicsSetMembers.CatalogueFor(set) }, set).Characters);
            return composedCharacters;
        }
    }

    /// <summary>The sheets enemies drawn with <paramref name="enemies"/> draw from.</summary>
    internal static IEnemySheetSource For(EnemyTileSourceArtwork enemies)
    {
        ArgumentNullException.ThrowIfNull(enemies);
        return composed.GetValue(enemies, key =>
        {
            // The shared installed artwork holds the installation's stock content by construction (fixtures that author
            // their own content load artwork of their own), so it draws a retail set from the installed sheets without its
            // content being hashed to show that.
            if (InstalledFixtures.IsSharedEnemyTiles(key))
                return new ComposedOnDemand(inputs.Value with { Enemies = key }, InstalledFixtures.EnemySheets);
            string content = key.ContentIdentity;
            lock (composedByContent)
            {
                if (composedByContent.TryGetValue(content, out WeakReference<ComposedOnDemand>? earlier) &&
                    earlier.TryGetTarget(out ComposedOnDemand? sheets))
                    return sheets;
                sheets = new ComposedOnDemand(inputs.Value with { Enemies = key },
                    content == stockContent.Value ? InstalledFixtures.EnemySheets : null);
                composedByContent[content] = new WeakReference<ComposedOnDemand>(sheets);
                return sheets;
            }
        });
    }

    /// <summary>
    /// A fixture artwork's sheets, composed a graphics set at a time as its rooms draw under them (#1599). The installation's
    /// artwork draws a retail population's enemies from the installed variants, and a set some retail population draws
    /// under from that population's; it composes only a set no retail room draws under, a fixture's own among them.
    /// </summary>
    private sealed class ComposedOnDemand(EnemySheetInputs inputs, InstalledEnemySheets? installed) : IEnemySheetSource
    {
        /// <summary>Each composed set's artwork, keyed by the set's definition, which lives as long as the process.</summary>
        private readonly Dictionary<RoomEnemyGraphicsSetDefinition, PopulationArtwork> sets = new(ReferenceEqualityComparer.Instance);

        /// <inheritdoc/>
        public PopulationArtwork Standard => installed?.Standard ?? Set(RoomEnemyGraphicsSetDefinitions.SessionStart);

        /// <summary>The artwork each population a room drew from was resolved to, by the population's definition.</summary>
        private readonly ConditionalWeakTable<RoomEnemyPopulationDefinition, PopulationArtwork> resolved = new();

        /// <inheritdoc/>
        public PopulationArtwork For(RoomEnemyPopulationDefinition population)
        {
            ArgumentNullException.ThrowIfNull(population);
            return resolved.GetValue(population, Resolve);
        }

        /// <summary>The artwork <paramref name="population"/>'s enemies draw from, resolved once.</summary>
        private PopulationArtwork Resolve(RoomEnemyPopulationDefinition population)
        {
            // A retail population draws the variants the installation names for it.
            if (installed is not null && installed.populations.ContainsKey(population.Name)
                && SuperMetroid.Core.World.RoomStates.RoomEnemyPopulationDefinitions.Definitions.Contains(population))
                return installed.For(population);
            RoomEnemyGraphicsSetDefinition set = FixturePopulationSets.SetOf(population);
            if (installed is not null)
            {
                if (FixturePopulationSets.RetailDrawing(set) is { } retail)
                    return installed.For(retail);
                if (set == RoomEnemyGraphicsSetDefinitions.SessionStart)
                    return installed.Standard;
            }
            return Set(set);
        }

        /// <summary>The artwork of <paramref name="set"/>, composed the first time a room draws under it.</summary>
        private PopulationArtwork Set(RoomEnemyGraphicsSetDefinition set)
        {
            lock (sets)
            {
                if (!sets.TryGetValue(set, out PopulationArtwork? composedSet))
                    sets.Add(set, composedSet = Compose(set));
                return composedSet;
            }
        }

        /// <summary>
        /// Composes one graphics set's artwork: each enemy drawn under it in a variant of its own, and each owner's spawned
        /// objects, every sheet composed, loaded and registered.
        /// </summary>
        private PopulationArtwork Compose(RoomEnemyGraphicsSetDefinition set)
        {
            // A fixture's own set is coloured in the rows its records name, which the fixture recorded.
            GraphicsSetSheetSources setSources = EnemySheetComposer.Sources(inputs with { SetPaletteRows = FixtureGraphicsSetRows.CatalogueFor(set), SetMembers = FixtureGraphicsSetMembers.CatalogueFor(set) }, set);
            // A name of its own keeps its frames apart from the installed sheets' in the frame-source registry.
            string owner = $"fixture-{RuntimeHelpers.GetHashCode(inputs.Enemies):X8}-{set.Name}-{RuntimeHelpers.GetHashCode(set):X8}";
            var enemies = new Dictionary<EnemyDefinitionId, EnemyArtworkVariant<SpriteSheet>>();
            foreach ((EnemyDefinitionId enemy, SpriteSheetSources sources, IReadOnlyDictionary<string, IReadOnlyList<FrameLookChange>> frameChanges,
                         IReadOnlyDictionary<string, CorpsePicturePlacement> frameCorpses) in setSources.Enemies)
            {
                string name = $"{enemy}-{owner}";
                enemies[enemy] = new EnemyArtworkVariant<SpriteSheet>(name, Load(sources, name), Encounters(frameChanges), frameCorpses);
            }
            var spawned = new Dictionary<SpawnedObjectOwner, EnemyArtworkVariant<SpawnedObjectSheet>>();
            foreach ((SpawnedObjectSheetPlan plan, SpriteSheetSources sources, IReadOnlyDictionary<string, IReadOnlyList<FrameLookChange>> frameChanges,
                         IReadOnlyDictionary<string, CorpsePicturePlacement> frameCorpses, IReadOnlySet<string> shownNothing) in setSources.Spawned)
            {
                string name = $"Spawned-{plan.Owner}-{owner}";
                // A fixture's set holds each of its spawned objects' pictures on its own sheet: none is common to another set.
                var variant = new EnemyArtworkVariant<SpawnedObjectSheet>(name,
                    new SpawnedObjectSheet(Load(sources, name), new Dictionary<string, SpriteSheet>(), shownNothing), Encounters(frameChanges), frameCorpses);
                foreach (SpawnedObjectOwner spawnedOwner in plan.AlsoOwnedBy.Prepend(plan.Owner))
                    spawned[spawnedOwner] = variant;
            }
            return new PopulationArtwork(enemies, spawned);
        }

        /// <summary>The encounters whose looks change each frame <paramref name="frameChanges"/> names, in encounter order.</summary>
        private static Dictionary<string, EncounterArtwork[]> Encounters(IReadOnlyDictionary<string, IReadOnlyList<FrameLookChange>> frameChanges) =>
            frameChanges.ToDictionary(frame => frame.Key, frame => frame.Value.Select(change => change.Encounter).Order().ToArray(), StringComparer.Ordinal);

        /// <summary>Composes, loads and registers one sheet under <paramref name="name"/>.</summary>
        private static SpriteSheet Load(SpriteSheetSources sources, string name)
        {
            ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
            SpriteSheet loaded = SpriteSheetFileConstants.Load(sheet.Manifest, name, sheet.Size,
                sheet.Pixels, sheet.PaletteIndices, sheet.FrontTiles);
            SheetFrameSources.Register(loaded, sources);
            return loaded;
        }
    }
}
