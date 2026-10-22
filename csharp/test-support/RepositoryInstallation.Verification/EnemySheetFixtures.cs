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

    /// <summary>Sheets already composed, by enemy artwork.</summary>
    private static readonly ConditionalWeakTable<EnemyTileArtworkCatalog, ComposedOnDemand> composed = new();

    /// <summary>The sheets enemies drawn with <paramref name="enemies"/> draw from.</summary>
    internal static IEnemySheetSource For(EnemyTileArtworkCatalog enemies)
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
        /// <summary>Each composed set's sheets.</summary>
        private readonly ConditionalWeakTable<RoomEnemyGraphicsSetDefinition, ComposedSet> sets = new();

        /// <inheritdoc/>
        public SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy)
        {
            if (installed is not null && RoomEnemyGraphicsSetDefinitions.DrawnUnder.Contains(set))
                return installed.For(set, enemy);
            return sets.GetValue(set, Compose).Enemies.TryGetValue(enemy, out SpriteSheet? sheet) ? sheet
                : throw new InvalidDataException($"Enemy graphics set {set.Name} composes no sheet for {enemy}.");
        }

        /// <inheritdoc/>
        public bool SpawnsFromSheets(RoomEnemyGraphicsSetDefinition set) =>
            installed is not null && RoomEnemyGraphicsSetDefinitions.DrawnUnder.Contains(set) ? installed.SpawnsFromSheets(set)
                : sets.GetValue(set, Compose).SpawnsFromSheets;

        /// <inheritdoc/>
        public SpawnedObjectSheet Spawned(RoomEnemyGraphicsSetDefinition set, int tileBase)
        {
            if (installed is not null && RoomEnemyGraphicsSetDefinitions.DrawnUnder.Contains(set))
                return installed.Spawned(set, tileBase);
            return sets.GetValue(set, Compose).Spawned.TryGetValue(tileBase, out SpawnedObjectSheet? sheet) ? sheet
                : throw new InvalidDataException($"Enemy graphics set {set.Name} composes no spawned-object sheet for tile base ${tileBase:X3}.");
        }

        /// <inheritdoc/>
        public RuntimeCharacterUploadCatalog Uploads => SuperMetroid.Core.World.Artwork.RuntimeCharacterUploadDefinitions.Catalog;

        /// <inheritdoc/>
        public IReadOnlyList<CharacterRegion> Regions(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy, string frame)
        {
            if (installed is not null && RoomEnemyGraphicsSetDefinitions.DrawnUnder.Contains(set))
                return installed.Regions(set, enemy, frame);
            return sets.GetValue(set, Compose).Pictures.TryGetValue(enemy, out IReadOnlyDictionary<string, CharacterRegion[]>? frames)
                && frames.TryGetValue(frame, out CharacterRegion[]? regions) ? regions : [];
        }

        /// <inheritdoc/>
        public IReadOnlyList<CharacterRegion> SpawnedRegions(RoomEnemyGraphicsSetDefinition set, int tileBase, string frame)
        {
            if (installed is not null && RoomEnemyGraphicsSetDefinitions.DrawnUnder.Contains(set))
                return installed.SpawnedRegions(set, tileBase, frame);
            return sets.GetValue(set, Compose).SpawnedPictures.TryGetValue(tileBase, out IReadOnlyDictionary<string, CharacterRegion[]>? frames)
                && frames.TryGetValue(frame, out CharacterRegion[]? regions) ? regions : [];
        }

        /// <summary>Composes and registers one graphics set's sheets.</summary>
        private ComposedSet Compose(RoomEnemyGraphicsSetDefinition set)
        {
            GraphicsSetSheetSources setSources = EnemySheetComposer.Sources(inputs, set);
            // A name of its own keeps its frames apart from the installed sheets' in the frame-source registry.
            string owner = $"fixture-{RuntimeHelpers.GetHashCode(inputs.Enemies):X8}-{set.Name}-{RuntimeHelpers.GetHashCode(set):X8}";
            var enemies = new Dictionary<EnemyDefinitionId, SpriteSheet>();
            var pictures = new Dictionary<EnemyDefinitionId, IReadOnlyDictionary<string, CharacterRegion[]>>();
            foreach ((EnemyDefinitionId enemy, SpriteSheetSources sources, IReadOnlyDictionary<string, CharacterRegion[]> framePictures) in setSources.Enemies)
            {
                enemies[enemy] = Load(sources, $"{enemy}-{owner}");
                pictures[enemy] = framePictures;
            }
            var spawned = new Dictionary<int, SpawnedObjectSheet>();
            var spawnedPictures = new Dictionary<int, IReadOnlyDictionary<string, CharacterRegion[]>>();
            foreach ((SpawnedObjectSheetPlan plan, SpriteSheetSources sources, IReadOnlyDictionary<string, CharacterRegion[]> framePictures) in setSources.Spawned)
            {
                spawned[plan.TileBase] = new SpawnedObjectSheet(Load(sources, $"Spawned-{plan.TileBase:X3}-{owner}"), plan.Palette);
                spawnedPictures[plan.TileBase] = framePictures;
            }
            return new ComposedSet(enemies, setSources.SpawnsFromSheets, spawned, pictures, spawnedPictures);
        }

        /// <summary>Composes, loads and registers one sheet under <paramref name="name"/>.</summary>
        private static SpriteSheet Load(SpriteSheetSources sources, string name)
        {
            ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
            SpriteSheet loaded = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(sheet.Manifest), writable: false), name, sheet.Size,
                sheet.Pixels, sheet.PaletteIndices, sheet.Priorities, sheet.PartRows);
            SheetFrameSources.Register(loaded, sources);
            return loaded;
        }
    }

    /// <summary>One composed graphics set's sheets.</summary>
    /// <param name="Enemies">Each enemy's sheet.</param>
    /// <param name="SpawnsFromSheets">Whether its spawned objects draw from sheets.</param>
    /// <param name="Spawned">Each spawned-object sheet by tile base.</param>
    /// <param name="Pictures">Each enemy sheet's frames drawn over uploaded characters, and the regions they are drawn over.</param>
    /// <param name="SpawnedPictures">The same for each spawned-object sheet, by tile base.</param>
    private sealed record ComposedSet(IReadOnlyDictionary<EnemyDefinitionId, SpriteSheet> Enemies, bool SpawnsFromSheets,
        IReadOnlyDictionary<int, SpawnedObjectSheet> Spawned,
        IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyDictionary<string, CharacterRegion[]>> Pictures,
        IReadOnlyDictionary<int, IReadOnlyDictionary<string, CharacterRegion[]>> SpawnedPictures);
}
