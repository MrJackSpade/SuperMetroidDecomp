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
        /// <summary>Each composed set's sheets by enemy.</summary>
        private readonly ConditionalWeakTable<RoomEnemyGraphicsSetDefinition, IReadOnlyDictionary<EnemyDefinitionId, SpriteSheet>> sets = new();

        /// <inheritdoc/>
        public SpriteSheet For(RoomEnemyGraphicsSetDefinition set, EnemyDefinitionId enemy)
        {
            if (installed is not null && RoomEnemyGraphicsSetDefinitions.All.Contains(set))
                return installed.For(set, enemy);
            return sets.GetValue(set, Compose).TryGetValue(enemy, out SpriteSheet? sheet) ? sheet
                : throw new InvalidDataException($"Enemy graphics set {set.Name} composes no sheet for {enemy}.");
        }

        /// <summary>Composes and registers one graphics set's sheets.</summary>
        private IReadOnlyDictionary<EnemyDefinitionId, SpriteSheet> Compose(RoomEnemyGraphicsSetDefinition set)
        {
            var sheets = new Dictionary<EnemyDefinitionId, SpriteSheet>();
            foreach ((RoomEnemyGraphicsSetEntry entry, SpriteSheetSources sources) in EnemySheetComposer.Sources(inputs, set))
            {
                ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(sources, 0, 0);
                // A name of its own keeps its frames apart from the installed sheets' in the frame-source registry.
                SpriteSheet loaded = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(sheet.Manifest), writable: false),
                    $"{entry.DefinitionId}-fixture-{RuntimeHelpers.GetHashCode(inputs.Enemies):X8}-{set.Name}-{RuntimeHelpers.GetHashCode(set):X8}", sheet.Size, sheet.Pixels,
                    sheet.PaletteIndices, sheet.Priorities, sheet.PartRows);
                SheetFrameSources.Register(loaded, sources);
                sheets[entry.DefinitionId] = loaded;
            }
            return sheets;
        }
    }
}
