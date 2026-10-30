using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Hud;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// What each installed sprite sheet composes from the stock content installed beside it (#1392), each family's inputs
/// loaded once when a sheet of it is first composed. The escape timer composes from the cartridge and is not here.
/// </summary>
internal sealed class SpriteSheetSourceCatalog
{
    /// <summary>Samus's stock artwork.</summary>
    private readonly Lazy<SamusSheetInputs> samus;

    /// <summary>The menus' presentation.</summary>
    private readonly Lazy<AreaMapPresentation> maps;
    /// <summary>The opening cinematic's artwork.</summary>
    private readonly Lazy<IntroCinematicArtwork> intro;

    /// <summary>The ending's sprite artwork.</summary>
    private readonly Lazy<EndingObjectArtwork> ending;

    /// <summary>The ending's palettes.</summary>
    private readonly Lazy<EndingPaletteSet> endingPalettes;

    /// <summary>The ending's Mode 7 artwork.</summary>
    private readonly Lazy<EndingFloorArtwork> endingMode7;

    /// <summary>Reads inputs from installed stock content.</summary>
    internal SpriteSheetSourceCatalog(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        samus = new(() => SamusSheetInputs.Load(content));
        maps = new(() => MapPresentationFiles.Load(Path.Combine(content, GameInstallationLayout.MapDirectoryName), null));
        intro = new(() => IntroCinematicArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.IntroCinematicDirectoryName), null));
        ending = new(() => EndingObjectArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.EndingObjectDirectoryName), null));
        endingPalettes = new(() => EndingPaletteArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.EndingPaletteDirectoryName), null));
        endingMode7 = new(() => EndingMode7ArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.EndingMode7DirectoryName), null));
    }

    /// <summary>The OBJ palette row and priority of parts that choose neither, as the families' draws leave them.</summary>
    private const int DefaultPalette = 0, DefaultPriority = 0;

    /// <summary>Composes sheet <paramref name="sheet"/>.</summary>
    internal ComposedSpriteSheet Compose(SpriteSheetId sheet) => sheet == SpriteSheetId.PauseHud ? HudImageSheet.ComposePause(maps.Value)
        : sheet == SpriteSheetId.MenuPictures ? MenuPictureSheet.Compose(maps.Value)
        : sheet == SpriteSheetId.RoomMapPictures ? MapPictureSheets.ComposeRoomMap(maps.Value)
        : sheet == SpriteSheetId.PausePictures ? PausePictureSheet.Compose(maps.Value)
        : ScreenTileSheetDefinitions.AreaMapForegrounds.Values.Contains(sheet)
            ? MapPictureSheets.ComposeWorldForeground(maps.Value, ScreenTileSheetDefinitions.AreaMapForegrounds.Single(entry => entry.Value == sheet).Key)
        : ScreenTileSheetDefinitions.AreaMapBackgrounds.Values.Contains(sheet)
            ? MapPictureSheets.ComposeWorldBackground(maps.Value, ScreenTileSheetDefinitions.AreaMapBackgrounds.Single(entry => entry.Value == sheet).Key)
        : ScreenTileSheetDefinitions.FloorSheets.Contains(sheet) ? FrontendFloorSheetComposer.Compose(sheet, ScreenTiles())
        : EndingPictureSheetComposer.Sheets.Contains(sheet) ? EndingPictureSheetComposer.Compose(sheet, ScreenTiles())
        : sheet == SpriteSheetId.CeresCinematicPictures ? CeresCinematicPictureSheetComposer.Compose(intro.Value)
        : sheet == SpriteSheetId.IntroScenePictures ? IntroScenePictureSheetComposer.Compose(intro.Value)
        : sheet == SpriteSheetId.IntroNarrationPictures ? IntroNarrationPictureSheetComposer.Compose(intro.Value, maps.Value.IntroFont)
        : SpriteSheetComposer.Compose(For(sheet), DefaultPalette, DefaultPriority);

    /// <summary>
    /// Whether <paramref name="sheet"/> composes from sprite parts (<see cref="For"/>), rather than from the cartridge (the
    /// escape timer) or from the pictures, pages and floors <see cref="Compose"/> paints directly.
    /// </summary>
    internal static bool ComposesFromSpriteParts(SpriteSheetId sheet) =>
        sheet is not (SpriteSheetId.EscapeTimer or SpriteSheetId.PauseHud or SpriteSheetId.MenuPictures or SpriteSheetId.RoomMapPictures
            or SpriteSheetId.PausePictures or SpriteSheetId.CeresCinematicPictures or SpriteSheetId.IntroScenePictures
            or SpriteSheetId.IntroNarrationPictures)
        && !ScreenTileSheetDefinitions.AreaMapForegrounds.Values.Contains(sheet)
        && !ScreenTileSheetDefinitions.AreaMapBackgrounds.Values.Contains(sheet)
        && !ScreenTileSheetDefinitions.FloorSheets.Contains(sheet)
        && !EndingPictureSheetComposer.Sheets.Contains(sheet);

    /// <summary>The stock artwork the screen tile sheets compose from.</summary>
    private ScreenTileSources ScreenTiles() =>
        new(() => maps.Value, () => intro.Value, () => ending.Value, () => endingPalettes.Value, () => endingMode7.Value);

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
        SpriteSheetId.CeresFlightSprites => IntroSpriteSheetComposer.CeresFlightSources(intro.Value.CeresFlight),
        SpriteSheetId.CeresDestructionSprites => IntroSpriteSheetComposer.CeresDestructionSources(intro.Value),
        SpriteSheetId.ZebesRevealSprites => IntroSpriteSheetComposer.ZebesRevealSources(intro.Value),
        SpriteSheetId.EndingCloudSprites or SpriteSheetId.EndingExplosionSprites or
            SpriteSheetId.EndingSuitedRewardSprites or SpriteSheetId.EndingSuitlessRewardSprites or
            SpriteSheetId.EndingShootingSprites or SpriteSheetId.EndingShootingLogoQuarterSprites or
            SpriteSheetId.EndingShootingLogoHalfSprites or SpriteSheetId.EndingShootingLogoThreeQuartersSprites or
            SpriteSheetId.EndingLogoSprites =>
            EndingSpriteSheetComposer.Sources(sheet, ending.Value, endingPalettes.Value, maps.Value.EndingFont),
        SpriteSheetId.Grapple or SpriteSheetId.SamusBodyTop or SpriteSheetId.SamusBodyBottom or
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
            SpriteSheetId.EndingEscapeAFloor or SpriteSheetId.EndingEscapeBFloor or
            SpriteSheetId.EndingPlanetExplosionFloor or SpriteSheetId.EndingFlyawayFloor or
            SpriteSheetId.EndingRewardIconFloor =>
            SamusSheetSources.For(samus.Value, sheet),
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
    };
}
