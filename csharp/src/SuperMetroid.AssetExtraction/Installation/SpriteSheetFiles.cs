using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured sprite sheets (#1392): for each sheet an RGBA PNG and its frame manifest, with a provenance
/// manifest of their hashes. A player override of either file replaces the stock one.
/// </summary>
public static class SpriteSheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ManifestFileName = "sprite-sheets.json";

    /// <summary>Composes and writes every sheet, then validates the result.</summary>
    /// <param name="bus">Cartridge source for frames no installed artwork holds.</param>
    /// <param name="content">Installed stock content holding the characters, compositions and colours the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string content, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ManifestFileName);
        var sources = new SpriteSheetSourceCatalog(content);
        foreach (SpriteSheetId sheet in SpriteSheetDefinitions.Sheets)
            store.Write(SpriteSheetDefinitions.FileName(sheet), Compose(bus, sources, sheet));
        store.Finish(sourceCartridgeSha256);
        _ = Load(directory, null);
    }

    /// <summary>Checks the stock files against their manifest and loads every sheet, preferring a player override of each file.</summary>
    /// <exception cref="InvalidDataException">The manifest, a stock hash, an image or a frame manifest is invalid.</exception>
    public static InstalledSpriteSheets Load(string stockDirectory, string? overrideDirectory)
    {
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ManifestFileName, SpriteSheetDefinitions.Sheets.Count);
        var sheets = new Dictionary<SpriteSheetId, SpriteSheet>();
        // The body sheets are named from the stock body tables installed beside them.
        string content = Path.GetDirectoryName(Path.GetFullPath(stockDirectory))
            ?? throw new ArgumentException($"Stock directory {stockDirectory} has no installation above it.", nameof(stockDirectory));
        var body = new Lazy<SamusBodyArtworkCatalog>(() => SamusBodyArtworkFiles.Load(
            Path.Combine(content, GameInstallationLayout.SamusBodyDirectoryName), null));
        // The pause sheet is named from the stock selector presentation installed beside it.
        var maps = new Lazy<AreaMapPresentationCatalog>(() => MapPresentationFiles.Load(
            Path.Combine(content, GameInstallationLayout.MapDirectoryName), null));
        // The intro sheet is named from the stock intro compositions installed beside it.
        var intro = new Lazy<IntroCinematicArtwork>(() => IntroCinematicArtworkFiles.Load(
            Path.Combine(content, GameInstallationLayout.IntroCinematicDirectoryName), null));
        // The ending sheets are named from the stock ending compositions installed beside them.
        var ending = new Lazy<EndingObjectArtwork>(() => EndingObjectArtworkFiles.Load(
            Path.Combine(content, GameInstallationLayout.EndingObjectDirectoryName), null));
        foreach (SpriteSheetId sheet in SpriteSheetDefinitions.Sheets)
        {
            SpriteSheet loaded = store.Read(SpriteSheetDefinitions.FileName(sheet));
            if (!loaded.HoldsExactly(FrameNames(sheet, body, maps, intro, ending)))
                throw new InvalidDataException($"Sprite sheet {FrameFile(sheet)} does not hold exactly the frames its family draws.");
            sheets.Add(sheet, loaded);
        }
        var installed = new InstalledSpriteSheets(sheets);
        // Every screen tile sheet is whole blocks of tiles coloured for its characters' depth (#1392).
        foreach (SpriteSheetId sheet in ScreenTileSheetDefinitions.Depths.Keys)
            _ = ScreenTileSheets.Of(installed, sheet);
        foreach (SpriteSheetId sheet in ScreenTileSheetDefinitions.FloorSheets)
            _ = ScreenTileSheets.Floor(installed, sheet);
        return installed;
    }

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Composes one sheet.</summary>
    private static ComposedSpriteSheet Compose(ISnesAddressSpace bus, SpriteSheetSourceCatalog sources, SpriteSheetId sheet) =>
        sheet == SpriteSheetId.EscapeTimer ? EscapeTimerSheetComposer.Compose(bus) : sources.Compose(sheet);

    /// <summary>The frames a sheet must hold: the ones its family draws.</summary>
    /// <param name="sheet">The sheet.</param>
    /// <param name="body">Samus's body tables, which name the body sheets' frames.</param>
    /// <param name="maps">The menus' presentation, whose selector frames name the pause sheet's.</param>
    /// <param name="intro">The opening cinematic's artwork, whose compositions name the intro sheet's frames.</param>
    /// <param name="ending">The ending's sprite artwork, whose compositions name the ending sheets' frames.</param>
    internal static IEnumerable<string> FrameNames(SpriteSheetId sheet, Lazy<SamusBodyArtworkCatalog> body,
        Lazy<AreaMapPresentationCatalog> maps, Lazy<IntroCinematicArtwork> intro, Lazy<EndingObjectArtwork> ending) => sheet switch
    {
        SpriteSheetId.MenuForegroundTiles or SpriteSheetId.MenuMapTiles or
            SpriteSheetId.AreaMapForegroundTilesCrateria or SpriteSheetId.AreaMapForegroundTilesBrinstar or
            SpriteSheetId.AreaMapForegroundTilesNorfair or SpriteSheetId.AreaMapForegroundTilesWreckedShip or
            SpriteSheetId.AreaMapForegroundTilesMaridia or SpriteSheetId.AreaMapForegroundTilesTourian or
            SpriteSheetId.AreaMapBackgroundTilesCrateria or SpriteSheetId.AreaMapBackgroundTilesBrinstar or
            SpriteSheetId.AreaMapBackgroundTilesNorfair or SpriteSheetId.AreaMapBackgroundTilesWreckedShip or
            SpriteSheetId.AreaMapBackgroundTilesMaridia or SpriteSheetId.AreaMapBackgroundTilesTourian or
            SpriteSheetId.PauseMapTiles or SpriteSheetId.PauseInterfaceTiles or SpriteSheetId.PauseHudTiles or
            SpriteSheetId.IntroSceneTiles or SpriteSheetId.IntroFontTiles or SpriteSheetId.CeresFlightObjectTiles or
            SpriteSheetId.CeresIntroObjectTiles or SpriteSheetId.ZebesRevealTiles or
            SpriteSheetId.EndingCreditsFontTiles or SpriteSheetId.EndingTextFontTiles or
            SpriteSheetId.EndingSubtitleTiles or SpriteSheetId.EndingLogoTiles or SpriteSheetId.EndingWaitingTiles or
            SpriteSheetId.EndingWavySamusTiles or SpriteSheetId.EndingExplosionTiles =>
            [ScreenTileSheet.FrameName],
        SpriteSheetId.TitleFloorTiles or SpriteSheetId.TitleBabyTiles or SpriteSheetId.CeresFlightFloorTiles or
            SpriteSheetId.EndingEscapeAFloorTiles or SpriteSheetId.EndingEscapeBFloorTiles or
            SpriteSheetId.EndingPlanetExplosionFloorTiles or SpriteSheetId.EndingFlyawayFloorTiles or
            SpriteSheetId.EndingRewardIconFloorTiles =>
            [FloorCharacterSheet.FrameName],
        SpriteSheetId.EscapeTimer => EscapeTimerPresentationDefinitions.Glyphs.Select(EscapeTimerComposition.FrameName),
        SpriteSheetId.Grapple => GrappleSheetDefinitions.Frames,
        SpriteSheetId.SamusBodyTop or SpriteSheetId.SamusBodyBottom => SamusBodySheetComposer.FrameNames(body.Value, sheet),
        SpriteSheetId.SamusOverlays => SamusOverlaySheetDefinitions.Frames,
        SpriteSheetId.SamusAtmosphere => SamusAtmosphereSheetDefinitions.Frames,
        SpriteSheetId.SamusDeath => SamusDeathSheetDefinitions.Frames,
        SpriteSheetId.GameOver => GameOverPresentationDefinitions.SpriteNames.ToArray(),
        SpriteSheetId.FileSelect => FileSelectPresentationDefinitions.SheetFrames,
        SpriteSheetId.GameOptions => GameOptionsPresentationDefinitions.SheetFrames,
        SpriteSheetId.MapSprites => MapSpriteDefinitions.SheetFrames,
        SpriteSheetId.PauseSprites => PauseSheetComposer.FrameNames(maps.Value),
        SpriteSheetId.Title => TitleSpriteDefinitions.All.Select(TitleSpriteDefinitions.FrameName),
        SpriteSheetId.IntroSprites => IntroSpriteSheetComposer.FrameNames(intro.Value),
        SpriteSheetId.CeresFlightSprites => IntroSpriteSheetComposer.CeresFlightFrameNames(intro.Value.CeresFlight),
        SpriteSheetId.CeresDestructionSprites or SpriteSheetId.ZebesRevealSprites => IntroSpriteSheetComposer.CeresDestructionFrameNames(intro.Value),
        SpriteSheetId.EndingCloudSprites or SpriteSheetId.EndingExplosionSprites or
            SpriteSheetId.EndingSuitedRewardSprites or SpriteSheetId.EndingSuitlessRewardSprites or
            SpriteSheetId.EndingShootingSprites or SpriteSheetId.EndingShootingLogoQuarterSprites or
            SpriteSheetId.EndingShootingLogoHalfSprites or SpriteSheetId.EndingShootingLogoThreeQuartersSprites or
            SpriteSheetId.EndingLogoSprites =>
            EndingSpriteSheetComposer.FrameNames(sheet, ending.Value),
        SpriteSheetId.IntroProjectiles or SpriteSheetId.ProjectilesPower or SpriteSheetId.ProjectilesWave or
            SpriteSheetId.ProjectilesIce or SpriteSheetId.ProjectilesIceWave or SpriteSheetId.ProjectilesSpazer or
            SpriteSheetId.ProjectilesSpazerWave or SpriteSheetId.ProjectilesSpazerIce or
            SpriteSheetId.ProjectilesSpazerIceWave or SpriteSheetId.ProjectilesPlasma or
            SpriteSheetId.ProjectilesPlasmaWave or SpriteSheetId.ProjectilesPlasmaIce or
            SpriteSheetId.ProjectilesPlasmaIceWave or SpriteSheetId.ProjectilesSpazerPlasmaWave or
            SpriteSheetId.ProjectilesSpazerPlasmaIce =>
            ProjectileSheetDefinitions.Frames,
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
    };

    /// <summary>A sheet's frame manifest file name.</summary>
    internal static string FrameFile(SpriteSheetId sheet) => SpriteSheetStore.FrameFile(SpriteSheetDefinitions.FileName(sheet));
}
