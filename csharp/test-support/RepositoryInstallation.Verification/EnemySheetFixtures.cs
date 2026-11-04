using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
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

    /// <summary>Sheets already composed, by enemy artwork.</summary>
    private static readonly ConditionalWeakTable<EnemyTileSourceArtwork, ComposedOnDemand> composed = new();

    /// <summary>The sheets enemies drawn with <paramref name="enemies"/> draw from.</summary>
    internal static IEnemySheetSource For(EnemyTileSourceArtwork enemies)
    {
        ArgumentNullException.ThrowIfNull(enemies);
        return composed.GetValue(enemies, key => new ComposedOnDemand(inputs.Value with { Enemies = key },
            key.ContentIdentity == inputs.Value.Enemies.ContentIdentity ? InstalledFixtures.EnemySheets : null));
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
            return Set(set).Enemies.TryGetValue(enemy, out SpriteSheet? sheet) ? sheet
                : throw new InvalidDataException($"Enemy graphics set {set.Name} composes no sheet for {enemy}.");
        }

        /// <inheritdoc/>
        public SpawnedObjectSheet Spawned(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner)
        {
            if (installed is not null && RoomEnemyGraphicsSetLists.DrawnUnder.Contains(set))
                return installed.Spawned(set, owner);
            return Set(set).Spawned.TryGetValue(owner, out SpawnedObjectSheet? sheet) ? sheet
                : throw new InvalidDataException($"Enemy graphics set {set.Name} composes no spawned-object sheet for {owner}.");
        }

        /// <inheritdoc/>
        public EncounterLooks Looks => SuperMetroid.AssetExtraction.Enemies.EncounterCharacterPlacements.Catalog.Looks;

        /// <inheritdoc/>
        public SetLookCoverage Coverage(RoomEnemyGraphicsSetDefinition set) =>
            installed is not null && RoomEnemyGraphicsSetLists.DrawnUnder.Contains(set) ? installed.Coverage(set) : Set(set).Coverage;

        /// <inheritdoc/>
        public IReadOnlyList<LookRegion[]> Layers(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame)
        {
            if (installed is not null && RoomEnemyGraphicsSetLists.DrawnUnder.Contains(set))
                return installed.Layers(set, enemy, frame);
            return Set(set).Pictures.TryGetValue(enemy, out IReadOnlyDictionary<string, LookRegion[][]>? frames)
                && frames.TryGetValue(frame, out LookRegion[][]? layers) ? layers : [];
        }

        /// <inheritdoc/>
        public IReadOnlyList<LookRegion[]> SpawnedLayers(RoomEnemyGraphicsSetDefinition set, SpawnedObjectOwner owner, string frame)
        {
            if (installed is not null && RoomEnemyGraphicsSetLists.DrawnUnder.Contains(set))
                return installed.SpawnedLayers(set, owner, frame);
            return Set(set).SpawnedPictures.TryGetValue(owner, out IReadOnlyDictionary<string, LookRegion[][]>? frames)
                && frames.TryGetValue(frame, out LookRegion[][]? layers) ? layers : [];
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

        /// <summary>Composes and registers one graphics set's sheets.</summary>
        private ComposedSet Compose(RoomEnemyGraphicsSetDefinition set)
        {
            GraphicsSetSheetSources setSources = EnemySheetComposer.Sources(inputs, set);
            // The set's regions, as the installed sheets' loader divides them for the corpses its drawn enemies leave.
            var regions = new SetRegions(EncounterCharacterPlacements.Catalog.Of(set), [.. CorpseRotSimulation.Of(inputs.Enemies, inputs.Bosses,
                [.. setSources.Enemies.Select(source => source.Enemy)]).Where(corpse => corpse.Kind is null)]);
            // A name of its own keeps its frames apart from the installed sheets' in the frame-source registry.
            string owner = $"fixture-{RuntimeHelpers.GetHashCode(inputs.Enemies):X8}-{set.Name}-{RuntimeHelpers.GetHashCode(set):X8}";
            var enemies = new Dictionary<EnemyDefinitionId, SpriteSheet>();
            var pictures = new Dictionary<EnemyDefinitionId, IReadOnlyDictionary<string, LookRegion[][]>>();
            var rotPictures = new Dictionary<EnemyDefinitionId, IReadOnlyDictionary<string, string>>();
            foreach ((EnemyDefinitionId enemy, SpriteSheetSources sources, IReadOnlyDictionary<string, SuperMetroid.AssetExtraction.Enemies.CharacterRange[][]> framePictures,
                         IReadOnlyDictionary<string, string> frameRots) in setSources.Enemies)
            {
                enemies[enemy] = Load(sources, $"{enemy}-{owner}");
                pictures[enemy] = SuperMetroid.AssetExtraction.Installation.EnemySheetFiles.Named(framePictures, regions);
                rotPictures[enemy] = frameRots;
            }
            var spawned = new Dictionary<SpawnedObjectOwner, SpawnedObjectSheet>();
            var spawnedPictures = new Dictionary<SpawnedObjectOwner, IReadOnlyDictionary<string, LookRegion[][]>>();
            var spawnedRotPictures = new Dictionary<SpawnedObjectOwner, IReadOnlyDictionary<string, string>>();
            foreach ((SpawnedObjectSheetPlan plan, SpriteSheetSources sources, IReadOnlyDictionary<string, SuperMetroid.AssetExtraction.Enemies.CharacterRange[][]> framePictures,
                         IReadOnlyDictionary<string, string> frameRots) in setSources.Spawned)
            {
                var sheet = new SpawnedObjectSheet(Load(sources, $"Spawned-{plan.Owner}-{owner}"));
                IReadOnlyDictionary<string, LookRegion[][]> named = SuperMetroid.AssetExtraction.Installation.EnemySheetFiles.Named(framePictures, regions);
                foreach (SpawnedObjectOwner spawnedOwner in plan.AlsoOwnedBy.Prepend(plan.Owner))
                {
                    spawned[spawnedOwner] = sheet;
                    spawnedPictures[spawnedOwner] = named;
                    spawnedRotPictures[spawnedOwner] = frameRots;
                }
            }
            return new ComposedSet(enemies, spawned, pictures, spawnedPictures, rotPictures, spawnedRotPictures, regions.Coverage(set));
        }

        /// <summary>Composes, loads and registers one sheet under <paramref name="name"/>.</summary>
        private static SpriteSheet Load(SpriteSheetSources sources, string name)
        {
            ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
            SpriteSheet loaded = SpriteSheetFile.Load(new MemoryStream(SpriteSheetManifestWriter.Write(sheet.Manifest), writable: false), name, sheet.Size,
                sheet.Pixels, sheet.PaletteIndices, sheet.Priorities);
            SheetFrameSources.Register(loaded, sources);
            return loaded;
        }
    }

    /// <summary>One composed graphics set's sheets.</summary>
    /// <param name="Enemies">Each enemy's sheet.</param>
    /// <param name="Spawned">Each spawned-object sheet by tile base.</param>
    /// <param name="Pictures">Each enemy sheet's frames drawn over uploaded characters, and the regions they are drawn over.</param>
    /// <param name="SpawnedPictures">The same for each spawned-object sheet, by tile base.</param>
    /// <param name="RotPictures">Each enemy sheet's frames drawn over a rotting corpse's rows, and the kind of corpse each shows.</param>
    /// <param name="SpawnedRotPictures">The same for each spawned-object sheet, by tile base.</param>
    /// <param name="Coverage">What covers each region of the set's characters.</param>
    private sealed record ComposedSet(IReadOnlyDictionary<EnemyDefinitionId, SpriteSheet> Enemies,
        IReadOnlyDictionary<SpawnedObjectOwner, SpawnedObjectSheet> Spawned,
        IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyDictionary<string, LookRegion[][]>> Pictures,
        IReadOnlyDictionary<SpawnedObjectOwner, IReadOnlyDictionary<string, LookRegion[][]>> SpawnedPictures,
        IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyDictionary<string, string>> RotPictures,
        IReadOnlyDictionary<SpawnedObjectOwner, IReadOnlyDictionary<string, string>> SpawnedRotPictures, SetLookCoverage Coverage);
}
