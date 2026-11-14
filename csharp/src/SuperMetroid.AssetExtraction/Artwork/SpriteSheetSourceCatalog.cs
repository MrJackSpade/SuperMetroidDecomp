using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// What each installed sprite sheet composes from the stock content installed beside it (#1392), each family's inputs
/// loaded once when a sheet of it is first composed, and from the ending's colours, which the import decodes from the
/// cartridge (#1544). The escape timer composes from the cartridge and is not here.
/// </summary>
internal sealed class SpriteSheetSourceCatalog
{
    /// <summary>Samus's stock artwork.</summary>
    private readonly Lazy<SamusSheetInputs> samus;

    /// <summary>The menus' presentation.</summary>
    private readonly Lazy<ImportAreaMapPresentation> maps;
    /// <summary>The opening cinematic's artwork.</summary>
    private readonly Lazy<ImportIntroCinematicArtwork> intro;

    /// <summary>The ending's sprite artwork.</summary>
    private readonly Lazy<ImportEndingObjectArtwork> ending;

    /// <summary>The ending's colours, decoded from the cartridge.</summary>
    private readonly EndingPaletteSet endingPalettes;

    /// <summary>The ending's Mode 7 artwork.</summary>
    private readonly Lazy<EndingFloorCharacters> endingMode7;

    /// <summary>Reads inputs from installed stock content and the ending's decoded colours.</summary>
    /// <param name="content">Installed stock content.</param>
    /// <param name="endingColours">The ending's colours, which the import decodes from the cartridge (#1544).</param>
    internal SpriteSheetSourceCatalog(string content, EndingPaletteSet endingColours)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        endingPalettes = Ensure.NotNull(endingColours);
        samus = new(() => SamusSheetInputs.Load(content));
        // Each family composes from the documents extracted under content and the pages in their import data (#1636).
        string map = Path.Combine(content, GameInstallationLayout.MapDirectoryName);
        string introDirectory = Path.Combine(content, GameInstallationLayout.IntroCinematicDirectoryName);
        string endingImportData = ImportDataDirectory.Of(Path.Combine(content, GameInstallationLayout.EndingObjectDirectoryName));
        string endingMode7ImportData = ImportDataDirectory.Of(Path.Combine(content, GameInstallationLayout.EndingMode7DirectoryName));
        maps = new(() => MapPresentationFiles.LoadForImport(map, ImportDataDirectory.Of(map)));
        intro = new(() => IntroCinematicArtworkFiles.LoadForImport(introDirectory, ImportDataDirectory.Of(introDirectory)));
        ending = new(() => EndingObjectArtworkFiles.LoadForImport(endingImportData));
        endingMode7 = new(() => EndingMode7ArtworkFiles.LoadForImport(endingMode7ImportData));
    }

    /// <summary>The OBJ palette row and priority of parts that choose neither, as the families' draws leave them.</summary>
    private const int DefaultPalette = 0, DefaultPriority = 0;

    /// <summary>Composes sheet <paramref name="sheet"/>.</summary>
    internal ComposedSpriteSheet Compose(SpriteSheetId sheet) => sheet == SpriteSheetId.PauseHud ? HudImageSheet.ComposePause(maps.Value)
        : sheet == SpriteSheetId.MenuPictures ? MenuPictureSheet.Compose(maps.Value)
        : sheet == SpriteSheetId.RoomMapPictures ? MapPictureSheets.ComposeRoomMap(maps.Value)
        : sheet == SpriteSheetId.PausePictures ? PausePictureSheet.Compose(maps.Value)
        : ScreenPictureSheetDefinitions.AreaMapForegrounds.Values.Contains(sheet)
            ? MapPictureSheets.ComposeWorldForeground(maps.Value, ScreenPictureSheetDefinitions.AreaMapForegrounds.Single(entry => entry.Value == sheet).Key)
        : ScreenPictureSheetDefinitions.AreaMapBackgrounds.Values.Contains(sheet)
            ? MapPictureSheets.ComposeWorldBackground(maps.Value, ScreenPictureSheetDefinitions.AreaMapBackgrounds.Single(entry => entry.Value == sheet).Key)
        : FrontendFloorSheetComposer.Sheets.Contains(sheet) ? FrontendFloorSheetComposer.Compose(sheet, ScreenTiles())
        : EndingPictureSheetComposer.Sheets.Contains(sheet) ? EndingPictureSheetComposer.Compose(sheet, ScreenTiles())
        : sheet == SpriteSheetId.CeresCinematicPictures ? CeresCinematicPictureSheetComposer.Compose(intro.Value)
        : sheet == SpriteSheetId.IntroScenePictures ? IntroScenePictureSheetComposer.Compose(intro.Value, maps.Value.RoomPaletteFx)
        : sheet == SpriteSheetId.IntroNarrationPictures ? IntroNarrationPictureSheetComposer.Compose(intro.Value, maps.Value.IntroFont)
        : SpriteSheetComposer.Compose(For(sheet), DefaultPalette, DefaultPriority);

    /// <summary>The stock artwork the screen tile sheets compose from.</summary>
    private ScreenTileSources ScreenTiles() =>
        new(() => maps.Value, () => intro.Value, () => ending.Value, () => endingPalettes, () => endingMode7.Value);

    /// <summary>What sheet <paramref name="sheet"/> composes.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The sheet is the escape timer, which composes from the cartridge, or the pause HUD, which composes from the HUD's images.
    /// </exception>
    internal SpriteSheetSources For(SpriteSheetId sheet) => sheet switch
    {
        SpriteSheetId.EscapeTimer => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The escape timer composes from the cartridge."),
        SpriteSheetId.PauseHud => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The pause HUD composes from the HUD's images, not sprite parts."),
        SpriteSheetId.MenuPictures or SpriteSheetId.RoomMapPictures or SpriteSheetId.PausePictures => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The menus' pictures compose from their pages, not sprite parts."),
        SpriteSheetId.GameOver => GameOverSheetComposer.Sources(maps.Value),
        SpriteSheetId.FileSelect => FileSelectSheetComposer.Sources(maps.Value),
        SpriteSheetId.GameOptions => GameOptionsSheetComposer.Sources(maps.Value),
        SpriteSheetId.MapSprites => MapSpriteSheetComposer.Sources(maps.Value),
        SpriteSheetId.PauseSprites => PauseSheetComposer.Sources(maps.Value),
        SpriteSheetId.Title => TitleSheetComposer.Sources(maps.Value),
        SpriteSheetId.IntroSprites => IntroSpriteSheetComposer.Sources(intro.Value),
        SpriteSheetId.CeresFlightSprites => IntroSpriteSheetComposer.CeresFlightSources(intro.Value.FlightCharacters,
            intro.Value.FlightPalette, maps.Value.RoomPaletteFx),
        SpriteSheetId.CeresDestructionSprites => IntroSpriteSheetComposer.CeresDestructionSources(intro.Value),
        SpriteSheetId.ZebesRevealSprites => IntroSpriteSheetComposer.ZebesRevealSources(intro.Value, maps.Value.RoomPaletteFx),
        SpriteSheetId.EndingCloudSprites or SpriteSheetId.EndingExplosionSprites or
            SpriteSheetId.EndingSuitedRewardSprites or SpriteSheetId.EndingSuitlessRewardSprites or
            SpriteSheetId.EndingShootingSprites =>
            EndingSpriteSheetComposer.Sources(sheet, ending.Value, endingPalettes, maps.Value.EndingFont, maps.Value.RoomPaletteFx),
        SpriteSheetId.Grapple or SpriteSheetId.SamusBody or
            SpriteSheetId.SamusOverlays or SpriteSheetId.SamusAtmosphere or SpriteSheetId.SamusDeath or
            SpriteSheetId.IntroProjectiles or SpriteSheetId.ProjectilesPower or SpriteSheetId.ProjectilesWave or
            SpriteSheetId.ProjectilesIce or SpriteSheetId.ProjectilesIceWave or SpriteSheetId.ProjectilesSpazer or
            SpriteSheetId.ProjectilesSpazerWave or SpriteSheetId.ProjectilesSpazerIce or
            SpriteSheetId.ProjectilesSpazerIceWave or SpriteSheetId.ProjectilesPlasma or
            SpriteSheetId.ProjectilesPlasmaWave or SpriteSheetId.ProjectilesPlasmaIce or
            SpriteSheetId.ProjectilesPlasmaIceWave or SpriteSheetId.ProjectilesSpazerPlasmaWave or
            SpriteSheetId.ProjectilesSpazerPlasmaIce or
            SpriteSheetId.AreaMapForegroundCrateria or
            SpriteSheetId.AreaMapForegroundBrinstar or SpriteSheetId.AreaMapForegroundNorfair or
            SpriteSheetId.AreaMapForegroundWreckedShip or SpriteSheetId.AreaMapForegroundMaridia or
            SpriteSheetId.AreaMapForegroundTourian or SpriteSheetId.AreaMapBackgroundCrateria or
            SpriteSheetId.AreaMapBackgroundBrinstar or SpriteSheetId.AreaMapBackgroundNorfair or
            SpriteSheetId.AreaMapBackgroundWreckedShip or SpriteSheetId.AreaMapBackgroundMaridia or
            SpriteSheetId.AreaMapBackgroundTourian or SpriteSheetId.IntroScenePictures or
            SpriteSheetId.IntroNarrationPictures or SpriteSheetId.CeresCinematicPictures or
            SpriteSheetId.EndingCreditsRows or SpriteSheetId.EndingPostCreditsPictures or
            SpriteSheetId.EndingWavySamusPicture or SpriteSheetId.EndingExplosionPictures or
            SpriteSheetId.TitleFloor or SpriteSheetId.CeresFloor or
            SpriteSheetId.EndingSideCloudEscapeFloor or SpriteSheetId.EndingTopAndBottomCloudEscapeFloor or
            SpriteSheetId.EndingPlanetExplosionFloor or SpriteSheetId.EndingFlyawayFloor or
            SpriteSheetId.EndingRewardIconFloor =>
            SamusSheetSources.For(samus.Value, sheet),
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
    };
}
