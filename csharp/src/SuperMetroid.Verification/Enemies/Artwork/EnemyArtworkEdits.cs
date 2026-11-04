using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Installed enemy artwork with part of its enemy catalog re-read from an installation's files and the rest the stock's
/// (#1532): a check of one edited component runs from the stock catalog with that component replaced, rather than reading
/// every enemy file of the installation again for each edit.
/// </summary>
internal static class EnemyArtworkEdits
{
    /// <summary><paramref name="stock"/> with its enemy catalog's parts replaced by <paramref name="edit"/>.</summary>
    internal static InstalledEnemyArtwork WithEnemyParts(InstalledEnemyArtwork stock, Func<EnemyTileArtworkParts, EnemyTileArtworkParts> edit) =>
        stock with { Enemies = EnemyTileSourceArtwork.FromInstalledArtwork(edit(PartsOf(stock.Enemies))) };

    /// <summary><paramref name="stock"/> with every enemy's characters and colours as <paramref name="files"/> select them.</summary>
    internal static InstalledEnemyArtwork WithSheets(InstalledEnemyArtwork stock, EnemyTileArtworkReader files)
    {
        EnemyTileArtworkParts sheets = files.EnemySheets();
        return WithEnemyParts(stock, parts => parts with { Sheets = sheets.Sheets, Palettes = sheets.Palettes });
    }

    /// <summary>The parts <paramref name="enemies"/> was built from.</summary>
    private static EnemyTileArtworkParts PartsOf(EnemyTileSourceArtwork enemies)
    {
        EnemyInstalledSheet[] sheets = [.. enemies.SheetsInDefinitionOrder];
        return new EnemyTileArtworkParts
        {
            Sheets = sheets.ToDictionary(sheet => sheet.Definition, sheet => sheet.Sheet),
            Palettes = sheets.ToDictionary(sheet => sheet.Definition, sheet => sheet.Palette),
            Spritemaps = enemies.Spritemaps,
            ExtendedFrames = enemies.ExtendedFrames,
            GunshipLiftoff = enemies.GunshipLiftoff,
            ProjectileSpritemaps = enemies.ProjectileSpritemaps,
            MagdollitePaletteCycle = enemies.MagdollitePaletteCycle,
            WorkRobotPaletteCycle = enemies.WorkRobotPaletteCycle,
            ChozoAndTubeColors = enemies.ChozoAndTubeColors,
            DachoraColors = enemies.DachoraColors,
            AuxiliaryColors = enemies.AuxiliaryColors,
        };
    }
}
