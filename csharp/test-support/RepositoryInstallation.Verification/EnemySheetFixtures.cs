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
/// rooms load them, as an installation of that artwork would hold. Any set composes, a fixture's own among them. Composed frames are registered so OAM comparisons
/// still apply to them.
/// </summary>
internal static class EnemySheetFixtures
{
    /// <summary>The installation's stock artwork, whose enemies a fixture's replace.</summary>
    private static readonly Lazy<EnemySheetInputs> inputs = new(() => EnemySheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory));

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
    private static readonly Dictionary<RoomEnemyGraphicsSetDefinition, SetCharacters> characters = [];

    /// <summary>
    /// The OBJ characters graphics set <paramref name="set"/> leaves once its encounters have played each sequence
    /// <paramref name="reached"/> names through the step it names (#1513): the room load's characters with every step up to
    /// it copied in order, from the installation's artwork, which a frame's picture of those steps paints like.
    /// </summary>
    internal static SpriteArtwork LivePage(RoomEnemyGraphicsSetDefinition set, IReadOnlyList<EncounterArtworkStep> reached)
    {
        ArgumentNullException.ThrowIfNull(set);
        SetCharacters? composedCharacters;
        lock (characters)
        {
            if (!characters.TryGetValue(set, out composedCharacters))
                characters.Add(set, composedCharacters = EnemySheetComposer.Load(inputs.Value, set).Characters);
        }
        return composedCharacters.Page(reached);
    }

    /// <summary>The sheets enemies drawn with <paramref name="enemies"/> draw from.</summary>
    internal static IEnemySheetSource For(EnemyTileSourceArtwork enemies)
    {
        ArgumentNullException.ThrowIfNull(enemies);
        return composed.GetValue(enemies, key =>
        {
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
    /// A fixture artwork's sheets, composed a graphics set at a time as its rooms load them. The installation's artwork
    /// draws a retail set from the installed sheets and composes only a fixture's own set.
    /// </summary>
    private sealed class ComposedOnDemand(EnemySheetInputs inputs, InstalledEnemySheets? installed) : IEnemySheetSource
    {
        /// <summary>Each composed set's sheets, keyed by the set's definition, which lives as long as the process.</summary>
        private readonly Dictionary<RoomEnemyGraphicsSetDefinition, ComposedSet> sets = [];

        /// <summary>The sheets of <paramref name="set"/>, composed the first time a room loads it.</summary>
        private ComposedSet Set(RoomEnemyGraphicsSetDefinition set)
        {
            lock (sets)
            {
                if (!sets.TryGetValue(set, out ComposedSet? composedSet))
                    sets.Add(set, composedSet = Compose(set));
                return composedSet;
            }
        }

        /// <inheritdoc/>
        public SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy)
        {
            if (installed is not null && RoomEnemyGraphicsSetLists.DrawnUnder.Contains(set))
                return installed.For(set, enemy);
            return Set(set).Enemies.TryGetValue(enemy, out Lazy<SpriteSheet>? sheet) ? sheet.Value
                : throw new InvalidDataException($"Enemy graphics set {set.Name} composes no sheet for {enemy}.");
        }

        /// <inheritdoc/>
        public SpawnedObjectSheet Spawned(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner)
        {
            if (installed is not null && RoomEnemyGraphicsSetLists.DrawnUnder.Contains(set))
                return installed.Spawned(set, owner);
            return Set(set).Spawned.TryGetValue(owner, out Lazy<SpawnedObjectSheet>? sheet) ? sheet.Value
                : throw new InvalidDataException($"Enemy graphics set {set.Name} composes no spawned-object sheet for {owner}.");
        }

        /// <inheritdoc/>
        public IReadOnlyList<FrameArtworkVariation> Variations(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame)
        {
            if (installed is not null && RoomEnemyGraphicsSetLists.DrawnUnder.Contains(set))
                return installed.Variations(set, enemy, frame);
            return Set(set).Variations.TryGetValue(enemy, out IReadOnlyDictionary<string, FrameArtworkVariation[]>? frames)
                && frames.TryGetValue(frame, out FrameArtworkVariation[]? followed) ? followed : [];
        }

        /// <inheritdoc/>
        public IReadOnlyList<FrameArtworkVariation> SpawnedVariations(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string frame)
        {
            if (installed is not null && RoomEnemyGraphicsSetLists.DrawnUnder.Contains(set))
                return installed.SpawnedVariations(set, owner, frame);
            return Set(set).SpawnedVariations.TryGetValue(owner, out IReadOnlyDictionary<string, FrameArtworkVariation[]>? frames)
                && frames.TryGetValue(frame, out FrameArtworkVariation[]? followed) ? followed : [];
        }

        /// <inheritdoc/>
        public string? RotCorpse(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame)
        {
            if (installed is not null && RoomEnemyGraphicsSetLists.DrawnUnder.Contains(set))
                return installed.RotCorpse(set, enemy, frame);
            return Set(set).RotPictures.TryGetValue(enemy, out IReadOnlyDictionary<string, string>? frames)
                && frames.TryGetValue(frame, out string? corpse) ? corpse : null;
        }

        /// <inheritdoc/>
        public string? SpawnedRotCorpse(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string frame)
        {
            if (installed is not null && RoomEnemyGraphicsSetLists.DrawnUnder.Contains(set))
                return installed.SpawnedRotCorpse(set, owner, frame);
            return Set(set).SpawnedRotPictures.TryGetValue(owner, out IReadOnlyDictionary<string, string>? frames)
                && frames.TryGetValue(frame, out string? corpse) ? corpse : null;
        }

        /// <summary>
        /// Gathers one graphics set's sheet sources; each sheet is composed and registered the first time it is drawn, so a
        /// check reading one enemy's sheet composes that sheet alone.
        /// </summary>
        private ComposedSet Compose(RoomEnemyGraphicsSetDefinition set)
        {
            // A fixture's own set is coloured in the rows its records name, which the fixture recorded.
            GraphicsSetSheetSources setSources = EnemySheetComposer.Sources(inputs with { SetPaletteRows = FixtureGraphicsSetRows.CatalogueFor(set) }, set);
            // A name of its own keeps its frames apart from the installed sheets' in the frame-source registry.
            string owner = $"fixture-{RuntimeHelpers.GetHashCode(inputs.Enemies):X8}-{set.Name}-{RuntimeHelpers.GetHashCode(set):X8}";
            var enemies = new Dictionary<EnemyDefinitionId, Lazy<SpriteSheet>>();
            var variations = new Dictionary<EnemyDefinitionId, IReadOnlyDictionary<string, FrameArtworkVariation[]>>();
            var rotPictures = new Dictionary<EnemyDefinitionId, IReadOnlyDictionary<string, string>>();
            foreach ((EnemyDefinitionId enemy, SpriteSheetSources sources, IReadOnlyDictionary<string, FrameArtworkVariation[]> frameVariations,
                         IReadOnlyDictionary<string, string> frameRots) in setSources.Enemies)
            {
                string name = $"{enemy}-{owner}";
                enemies[enemy] = new Lazy<SpriteSheet>(() => Load(sources, name));
                variations[enemy] = frameVariations;
                rotPictures[enemy] = frameRots;
            }
            var spawned = new Dictionary<SpawnedObjectOwner, Lazy<SpawnedObjectSheet>>();
            var spawnedVariations = new Dictionary<SpawnedObjectOwner, IReadOnlyDictionary<string, FrameArtworkVariation[]>>();
            var spawnedRotPictures = new Dictionary<SpawnedObjectOwner, IReadOnlyDictionary<string, string>>();
            foreach ((SpawnedObjectSheetPlan plan, SpriteSheetSources sources, IReadOnlyDictionary<string, FrameArtworkVariation[]> frameVariations,
                         IReadOnlyDictionary<string, string> frameRots) in setSources.Spawned)
            {
                string name = $"Spawned-{plan.Owner}-{owner}";
                var sheet = new Lazy<SpawnedObjectSheet>(() => new SpawnedObjectSheet(Load(sources, name)));
                foreach (SpawnedObjectOwner spawnedOwner in plan.AlsoOwnedBy.Prepend(plan.Owner))
                {
                    spawned[spawnedOwner] = sheet;
                    spawnedVariations[spawnedOwner] = frameVariations;
                    spawnedRotPictures[spawnedOwner] = frameRots;
                }
            }
            return new ComposedSet(enemies, spawned, variations, spawnedVariations, rotPictures, spawnedRotPictures);
        }

        /// <summary>Composes, loads and registers one sheet under <paramref name="name"/>.</summary>
        private static SpriteSheet Load(SpriteSheetSources sources, string name)
        {
            ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
            SpriteSheet loaded = SpriteSheetFileConstants.Load(new MemoryStream(SpriteSheetManifestWriter.Write(sheet.Manifest), writable: false), name, sheet.Size,
                sheet.Pixels, sheet.PaletteIndices, sheet.Priorities);
            SheetFrameSources.Register(loaded, sources);
            return loaded;
        }
    }

    /// <summary>One composed graphics set's sheets.</summary>
    /// <param name="Enemies">Each enemy's sheet, composed when first drawn.</param>
    /// <param name="Spawned">Each spawned-object sheet by tile base, composed when first drawn.</param>
    /// <param name="Variations">Each enemy sheet's frames encounter artwork sequences change, and how each follows them.</param>
    /// <param name="SpawnedVariations">The same for each spawned-object sheet, by tile base.</param>
    /// <param name="RotPictures">Each enemy sheet's frames drawn over a rotting corpse's rows, and the kind of corpse each shows.</param>
    /// <param name="SpawnedRotPictures">The same for each spawned-object sheet, by tile base.</param>
    private sealed record ComposedSet(IReadOnlyDictionary<EnemyDefinitionId, Lazy<SpriteSheet>> Enemies,
        IReadOnlyDictionary<SpawnedObjectOwner, Lazy<SpawnedObjectSheet>> Spawned,
        IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyDictionary<string, FrameArtworkVariation[]>> Variations,
        IReadOnlyDictionary<SpawnedObjectOwner, IReadOnlyDictionary<string, FrameArtworkVariation[]>> SpawnedVariations,
        IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyDictionary<string, string>> RotPictures,
        IReadOnlyDictionary<SpawnedObjectOwner, IReadOnlyDictionary<string, string>> SpawnedRotPictures);
}
