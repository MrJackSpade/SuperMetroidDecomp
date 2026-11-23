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
internal sealed partial class SpriteSheetSourceCatalog
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
        _parts = PartSheets();
    }

    /// <summary>Each painted sheet's composer (<see cref="PaintedSheets"/>).</summary>
    private readonly Dictionary<SpriteSheetId, Func<ComposedSpriteSheet>> _painted;

    /// <summary>The sprite parts each sheet composed from them composes (<see cref="PartSheets"/>).</summary>
    private readonly Dictionary<SpriteSheetId, Func<SpriteSheetSources>> _parts;

    /// <summary>The OBJ palette row of parts that do not choose their own, as the families' draws leave it.</summary>
    private const int DefaultPalette = 0;

    /// <summary>The depth of parts that do not choose their own, as the families' draws leave it.</summary>
    private const ActorDepth DefaultDepth = ActorDepth.BehindRoom;

    /// <summary>
    /// Composes sheet <paramref name="sheet"/> with the composer bound to it: a painted sheet's own composer, or the sprite
    /// parts <see cref="For"/> names.
    /// </summary>
    internal ComposedSpriteSheet Compose(SpriteSheetId sheet) => _painted.TryGetValue(sheet, out Func<ComposedSpriteSheet>? paint)
        ? paint()
        : SpriteSheetComposer.Compose(For(sheet), DefaultPalette, DefaultDepth);

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

    /// <summary>What sheet <paramref name="sheet"/> composes from sprite parts.</summary>
    /// <exception cref="KeyNotFoundException">
    /// The sheet does not compose from sprite parts: the escape timer composes from the cartridge, and the painted sheets
    /// from their pages, images, floors and pictures.
    /// </exception>
    internal SpriteSheetSources For(SpriteSheetId sheet) => _parts[sheet]();

    /// <summary>
    /// Each sheet composed from sprite parts, bound to the parts it composes: the menus' and the opening's sprites, the
    /// ending's sprites, and Samus's sheets (<see cref="SamusSheetSources.Sheets"/>).
    /// </summary>
    /// <exception cref="ArgumentException">Two bindings name one sheet.</exception>
    private Dictionary<SpriteSheetId, Func<SpriteSheetSources>> PartSheets()
    {
        var parts = new Dictionary<SpriteSheetId, Func<SpriteSheetSources>>
        {
            [SpriteSheetId.GameOver] = () => GameOverSheetComposer.Sources(maps.Value),
            [SpriteSheetId.FileSelect] = () => FileSelectSheetComposer.Sources(maps.Value),
            [SpriteSheetId.GameOptions] = () => GameOptionsSheetComposer.Sources(maps.Value),
            [SpriteSheetId.MapSprites] = () => MapSpriteSheetComposer.Sources(maps.Value),
            [SpriteSheetId.PauseSprites] = () => PauseSheetComposer.Sources(maps.Value),
            [SpriteSheetId.Title] = () => TitleSheetComposer.Sources(maps.Value),
            [SpriteSheetId.IntroSprites] = () => IntroSpriteSheetComposer.Sources(intro.Value),
            [SpriteSheetId.CeresFlightSprites] = () => IntroSpriteSheetComposer.CeresFlightSources(intro.Value.FlightCharacters,
                intro.Value.FlightPalette, maps.Value.RoomPaletteFx),
            [SpriteSheetId.CeresDestructionSprites] = () => IntroSpriteSheetComposer.CeresDestructionSources(intro.Value),
            [SpriteSheetId.ZebesRevealSprites] = () => IntroSpriteSheetComposer.ZebesRevealSources(intro.Value, maps.Value.RoomPaletteFx),
        };
        foreach (SpriteSheetId sprites in EndingSpriteSheets)
            parts.Add(sprites, () => EndingSpriteSheetComposer.Sources(sprites, ending.Value, endingPalettes, maps.Value.EndingFont, maps.Value.RoomPaletteFx));
        foreach (SpriteSheetId samusSheet in SamusSheetSources.Sheets.Keys)
            parts.Add(samusSheet, () => SamusSheetSources.For(samus.Value, samusSheet));
        return parts;
    }

    /// <summary>The ending's sprite sheets, each composed from the ending's objects in its own colours.</summary>
    private static readonly SpriteSheetId[] EndingSpriteSheets =
    [
        SpriteSheetId.EndingCloudSprites, SpriteSheetId.EndingExplosionSprites, SpriteSheetId.EndingSuitedRewardSprites,
        SpriteSheetId.EndingSuitlessRewardSprites, SpriteSheetId.EndingShootingSprites,
    ];
}
