using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed ordinary enemy artwork: checking that every sheet has its palette and that installed sheets are
/// complete, an enemy's tiles and palette, and the identity of the whole selected presentation.
/// </summary>
internal static class EnemyTileArtwork
{
    /// <summary>
    /// Hashes each sheet with its palette in definition order, then each special catalog's identity. Gameplay definitions
    /// are excluded.
    /// </summary>
    internal static string ContentIdentity(EnemyTileArtworkCatalog artwork) =>
        SelectedPresentationHash.Create("enemy-bundle-v1", content =>
        {
            foreach ((EnemyDefinitionId definition, RoomCharacterAtlas sheet, EnemyPaletteSheet palette) in artwork.SheetsInDefinitionOrder)
            {
                content.Append("definition", System.Text.Encoding.UTF8.GetBytes(definition.ToString()));
                content.Append("tiles", sheet.Characters.Span);
                content.AppendIdentity("palette", palette.ContentIdentity);
            }
            content.AppendIdentity("Spritemaps", artwork.Spritemaps?.ContentIdentity);
            content.AppendIdentity("ExtendedFrames", artwork.ExtendedFrames?.ContentIdentity);
            content.AppendIdentity("GunshipLiftoff", artwork.GunshipLiftoff?.ContentIdentity);
            content.AppendIdentity("ProjectileSpritemaps", artwork.ProjectileSpritemaps?.ContentIdentity);
            content.AppendIdentity("MagdollitePaletteCycle", artwork.MagdollitePaletteCycle?.ContentIdentity);
            content.AppendIdentity("WorkRobotPaletteCycle", artwork.WorkRobotPaletteCycle?.ContentIdentity);
            content.AppendIdentity("ChozoAndTubeColors", artwork.ChozoAndTubeColors?.ContentIdentity);
            content.AppendIdentity("DachoraColors", artwork.DachoraColors?.ContentIdentity);
            content.AppendIdentity("AuxiliaryColors", artwork.AuxiliaryColors?.ContentIdentity);
        });

    /// <summary>The installed tiles of <paramref name="enemy"/>, as its room's graphics set shows them (#1406).</summary>
    /// <exception cref="InvalidDataException">The installed artwork holds no tile set for the enemy.</exception>
    internal static ReadOnlyMemory<byte> TilesOf(EnemyTileArtworkCatalog artwork, EnemyDefinitionId enemy) =>
        artwork.TryGetSheet(enemy, out RoomCharacterAtlas? atlas) ? atlas.Characters
            : throw new InvalidDataException($"Installed enemy artwork holds no {enemy} tile set.");

    /// <summary>Loads the sixteen indexed colors selected by a room graphics-set record.</summary>
    /// <exception cref="InvalidDataException">The enemy has no installed palette.</exception>
    internal static void LoadPaletteTo(EnemyTileArtworkCatalog artwork, EnemyDefinitionId definitionId, ColourRun destination)
    {
        if (!artwork.TryGetPalette(definitionId, out EnemyPaletteSheet? palette))
            throw new InvalidDataException($"Enemy {definitionId} has no installed palette.");
        palette.LoadTo(destination);
    }

    /// <summary>Publishes complete ordinary enemy sheets and palettes.</summary>
    internal static EnemyTileArtworkCatalog FromInstalledArtwork(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.Sheets);
        Ensure.NotNull(parts.Palettes);
        var installedSheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(parts.Sheets);
        var installedPalettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes);
        ValidateInstalledSheets(installedSheets, installedPalettes);
        return new EnemyTileArtworkCatalog(parts with
        {
            Sheets = installedSheets,
            Palettes = installedPalettes,
        });
    }

    /// <summary>Returns <paramref name="parts"/> after rejecting a sheet without its palette.</summary>
    /// <exception cref="InvalidDataException">A sheet has no colour sheet, or the counts differ.</exception>
    internal static EnemyTileArtworkParts Paired(EnemyTileArtworkParts parts)
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
