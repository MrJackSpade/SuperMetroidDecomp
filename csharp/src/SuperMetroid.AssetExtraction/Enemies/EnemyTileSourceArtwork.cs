using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy artwork as the import pipeline holds it (#1463): beside the palettes and colour catalogs gameplay
/// reads, each enemy's characters, the enemy and enemy-projectile compositions and the gunship's takeoff pages that the
/// sheet composers compose the pre-coloured enemy sheets from, and the identity of the whole selected presentation.
/// </summary>
public sealed class EnemyTileSourceArtwork : EnemyTileArtworkCatalog
{
    /// <summary>Each enemy's installed characters, by definition.</summary>
    private readonly Dictionary<EnemyDefinitionId, RoomCharacterAtlas> sheets;

    /// <summary>Copies installed artwork, rejecting a sheet without its palette.</summary>
    internal EnemyTileSourceArtwork(EnemyTileArtworkParts parts) : base(Paired(parts).Gameplay)
    {
        sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(parts.Sheets);
        Spritemaps = parts.Spritemaps;
        GunshipLiftoff = parts.GunshipLiftoff;
        ProjectileSpritemaps = parts.ProjectileSpritemaps;
    }

    /// <inheritdoc/>
    public override string ContentIdentity =>
        SelectedPresentationHash.Create("enemy-bundle-v1", content =>
        {
            foreach ((EnemyDefinitionId definition, RoomCharacterAtlas sheet, EnemyPaletteSheet palette) in SheetsInDefinitionOrder)
            {
                content.Append("definition", System.Text.Encoding.UTF8.GetBytes(definition.ToString()));
                content.Append("tiles", sheet.Characters.Span);
                content.AppendIdentity("palette", palette.ContentIdentity);
            }
            content.AppendIdentity("Spritemaps", Spritemaps?.ContentIdentity);
            content.AppendIdentity("ExtendedFrames", ExtendedFrames?.ContentIdentity);
            content.AppendIdentity("GunshipLiftoff", GunshipLiftoff?.ContentIdentity);
            content.AppendIdentity("ProjectileSpritemaps", ProjectileSpritemaps?.ContentIdentity);
            content.AppendIdentity("MagdollitePaletteCycle", MagdollitePaletteCycle?.ContentIdentity);
            content.AppendIdentity("WorkRobotPaletteCycle", WorkRobotPaletteCycle?.ContentIdentity);
            content.AppendIdentity("ChozoAndTubeColors", ChozoAndTubeColors?.ContentIdentity);
            content.AppendIdentity("DachoraColors", DachoraColors?.ContentIdentity);
            content.AppendIdentity("AuxiliaryColors", AuxiliaryColors?.ContentIdentity);
        });

    /// <summary>Each installed sheet with its palette, in definition order.</summary>
    internal IEnumerable<EnemyInstalledSheet> SheetsInDefinitionOrder =>
        sheets.OrderBy(pair => pair.Key).Select(pair => new EnemyInstalledSheet(pair.Key, pair.Value,
            TryGetPalette(pair.Key, out EnemyPaletteSheet? palette) ? palette
                : throw new InvalidDataException($"Installed enemy {pair.Key} has no palette.")));

    /// <summary>Finds the installed tile sheet of <paramref name="enemy"/>.</summary>
    internal bool TryGetSheet(EnemyDefinitionId enemy, [NotNullWhen(true)] out RoomCharacterAtlas? sheet) =>
        sheets.TryGetValue(enemy, out sheet);

    /// <summary>Installed visual-only OAM frames; null for constructed legacy fixtures.</summary>
    public EnemySpritemapCatalog? Spritemaps { get; internal set; }

    /// <summary>Five editable gunship takeoff character uploads; null for constructed fixtures.</summary>
    public GunshipLiftoffArtworkCatalog? GunshipLiftoff { get; }

    /// <summary>Installed bank-$8D projectile compositions; null for constructed fixtures.</summary>
    public EnemyProjectileSpritemapCatalog? ProjectileSpritemaps { get; }

    /// <summary>The installed tiles of <paramref name="enemy"/>, as its room's graphics set shows them (#1406).</summary>
    /// <exception cref="InvalidDataException">The installed artwork holds no tile set for the enemy.</exception>
    public ReadOnlyMemory<byte> TilesOf(EnemyDefinitionId enemy) =>
        TryGetSheet(enemy, out RoomCharacterAtlas? atlas) ? atlas.Characters
            : throw new InvalidDataException($"Installed enemy artwork holds no {enemy} tile set.");

    /// <summary>Publishes complete ordinary enemy sheets and palettes.</summary>
    /// <exception cref="InvalidDataException">A retail definition lacks its complete tile sheet or palette.</exception>
    public static EnemyTileSourceArtwork FromInstalledArtwork(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.Sheets);
        Ensure.NotNull(parts.Palettes);
        var installedSheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(parts.Sheets);
        var installedPalettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes);
        ValidateInstalledSheets(installedSheets, installedPalettes);
        return new EnemyTileSourceArtwork(parts with
        {
            Sheets = installedSheets,
            Palettes = installedPalettes,
        });
    }

    /// <summary>Returns <paramref name="parts"/> after rejecting a sheet without its palette.</summary>
    /// <exception cref="InvalidDataException">A sheet has no colour sheet, or the counts differ.</exception>
    private static EnemyTileArtworkParts Paired(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.Sheets);
        Ensure.NotNull(parts.Palettes);
        if (parts.Sheets.Count != parts.Palettes.Count || parts.Sheets.Keys.Any(definition => !parts.Palettes.ContainsKey(definition)))
            throw new InvalidDataException("Enemy artwork requires one color sheet per tile sheet.");
        return parts;
    }

    /// <summary>Requires a complete, correctly sized tile sheet and palette for every retail definition.</summary>
    private static void ValidateInstalledSheets(Dictionary<EnemyDefinitionId, RoomCharacterAtlas> sheets,
        Dictionary<EnemyDefinitionId, EnemyPaletteSheet> palettes)
    {
        Ensure.NotNull(sheets);
        Ensure.NotNull(palettes);
        if (sheets.Count != EnemyTileSourceDefinitions.All.Count ||
            palettes.Count != EnemyTileSourceDefinitions.All.Count)
            throw new InvalidDataException("Installed enemy artwork must contain every required sheet and palette.");
        foreach (EnemyTileSourceDefinition definition in EnemyTileSourceDefinitions.All)
            if (!sheets.TryGetValue(definition.DefinitionId, out RoomCharacterAtlas? sheet) || sheet is null ||
                sheet.Characters.Length != definition.ByteCount ||
                !palettes.TryGetValue(definition.DefinitionId, out EnemyPaletteSheet? palette) || palette is null)
                throw new InvalidDataException(
                    $"Installed enemy {definition.DefinitionId} lacks its complete tile sheet or palette.");
    }
}
