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
using SuperMetroid.Core.Progression;

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
        _painted = PaintedSheets();
    }

    /// <summary>Each painted sheet's composer (<see cref="PaintedSheets"/>).</summary>
    private readonly Dictionary<SpriteSheetId, Func<ComposedSpriteSheet>> _painted;

    /// <summary>The OBJ palette row and priority of parts that choose neither, as the families' draws leave them.</summary>
    private const int DefaultPalette = 0, DefaultPriority = 0;

    /// <summary>
    /// Composes sheet <paramref name="sheet"/> with the composer bound to it: a painted sheet's own composer, or the sprite
    /// parts <see cref="For"/> names.
    /// </summary>
    internal ComposedSpriteSheet Compose(SpriteSheetId sheet) => _painted.TryGetValue(sheet, out Func<ComposedSpriteSheet>? paint)
        ? paint()
        : SpriteSheetComposer.Compose(For(sheet), DefaultPalette, DefaultPriority);

    /// <summary>
    /// Each sheet painted from the menus' pages, the HUD's images, the screen floors and pictures, bound to its own
    /// composer (#2571); every other sheet composes from the sprite parts <see cref="For"/> names.
    /// </summary>
    /// <exception cref="ArgumentException">Two composers paint one sheet.</exception>
    private Dictionary<SpriteSheetId, Func<ComposedSpriteSheet>> PaintedSheets()
    {
        var painted = new Dictionary<SpriteSheetId, Func<ComposedSpriteSheet>>
        {
            [SpriteSheetId.PauseHud] = () => HudImageSheet.ComposePause(maps.Value),
            [SpriteSheetId.MenuPictures] = () => MenuPictureSheet.Compose(maps.Value),
            [SpriteSheetId.RoomMapPictures] = () => MapPictureSheets.ComposeRoomMap(maps.Value),
            [SpriteSheetId.PausePictures] = () => PausePictureSheet.Compose(maps.Value),
            [SpriteSheetId.CeresCinematicPictures] = () => CeresCinematicPictureSheetComposer.Compose(intro.Value),
            [SpriteSheetId.IntroScenePictures] = () => IntroScenePictureSheetComposer.Compose(intro.Value, maps.Value.RoomPaletteFx),
            [SpriteSheetId.IntroNarrationPictures] = () => IntroNarrationPictureSheetComposer.Compose(intro.Value, maps.Value.IntroFont),
        };
        foreach ((AreaId area, SpriteSheetId foreground) in ScreenPictureSheetDefinitions.AreaMapForegrounds)
            painted.Add(foreground, () => MapPictureSheets.ComposeWorldForeground(maps.Value, area));
        foreach ((AreaId area, SpriteSheetId background) in ScreenPictureSheetDefinitions.AreaMapBackgrounds)
            painted.Add(background, () => MapPictureSheets.ComposeWorldBackground(maps.Value, area));
        foreach (SpriteSheetId floor in FrontendFloorSheetComposer.Sheets)
            painted.Add(floor, () => FrontendFloorSheetComposer.Compose(floor, ScreenTiles()));
        foreach (SpriteSheetId pictures in EndingPictureSheetComposer.Sheets)
            painted.Add(pictures, () => EndingPictureSheetComposer.Compose(pictures, ScreenTiles()));
        return painted;
    }

    /// <summary>The stock artwork the screen tile sheets compose from.</summary>
    private ScreenTileSources ScreenTiles() =>
        new(() => maps.Value, () => intro.Value, () => ending.Value, () => endingPalettes, () => endingMode7.Value);

    /// <summary>What sheet <paramref name="sheet"/> composes.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The sheet is the escape timer, which composes from the cartridge, or the pause HUD, which composes from the HUD's images.
    /// </exception>
    private SpriteSheetSources For(SpriteSheetId sheet) => sheet switch
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
