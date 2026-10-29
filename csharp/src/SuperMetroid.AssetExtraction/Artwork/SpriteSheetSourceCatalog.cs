using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics;
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
        : ScreenTileSheetDefinitions.Depths.ContainsKey(sheet) ? ScreenTileSheetComposer.Compose(sheet, ScreenTiles())
        : ScreenTileSheetDefinitions.FloorSheets.Contains(sheet) ? ScreenTileSheetComposer.ComposeMode7(sheet, ScreenTiles())
        : SpriteSheetComposer.Compose(For(sheet), DefaultPalette, DefaultPriority);

    /// <summary>The stock artwork the screen tile sheets compose from.</summary>
    private ScreenTileSources ScreenTiles() =>
        new(() => maps.Value, () => intro.Value, () => ending.Value, () => endingPalettes.Value, () => endingMode7.Value);

    /// <summary>What sheet <paramref name="sheet"/> composes.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet is the escape timer, which composes from the cartridge.</exception>
    internal SpriteSheetSources For(SpriteSheetId sheet) => sheet switch
    {
        SpriteSheetId.EscapeTimer => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The escape timer composes from the cartridge."),
        SpriteSheetId.GameOver => GameOverSheetComposer.Sources(maps.Value),
        SpriteSheetId.FileSelect => FileSelectSheetComposer.Sources(maps.Value),
        SpriteSheetId.GameOptions => GameOptionsSheetComposer.Sources(maps.Value),
        SpriteSheetId.MapSprites => MapSpriteSheetComposer.Sources(maps.Value),
        SpriteSheetId.PauseSprites => PauseSheetComposer.Sources(maps.Value),
        SpriteSheetId.Title => TitleSheetComposer.Sources(maps.Value),
        SpriteSheetId.IntroSprites => IntroSpriteSheetComposer.Sources(intro.Value),
        SpriteSheetId.CeresFlightSprites => IntroSpriteSheetComposer.CeresFlightSources(intro.Value.CeresFlight),
        SpriteSheetId.CeresDestructionSprites => IntroSpriteSheetComposer.CeresDestructionSources(intro.Value,
            [CeresDestructionUpload.CeresScene]),
        SpriteSheetId.ZebesRevealSprites => IntroSpriteSheetComposer.CeresDestructionSources(intro.Value,
            [CeresDestructionUpload.CeresScene, CeresDestructionUpload.ZebesReveal]),
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
            SpriteSheetId.ProjectilesSpazerPlasmaIce or SpriteSheetId.MenuForegroundTiles or
            SpriteSheetId.MenuMapTiles or SpriteSheetId.AreaMapForegroundTilesCrateria or
            SpriteSheetId.AreaMapForegroundTilesBrinstar or SpriteSheetId.AreaMapForegroundTilesNorfair or
            SpriteSheetId.AreaMapForegroundTilesWreckedShip or SpriteSheetId.AreaMapForegroundTilesMaridia or
            SpriteSheetId.AreaMapForegroundTilesTourian or SpriteSheetId.AreaMapBackgroundTilesCrateria or
            SpriteSheetId.AreaMapBackgroundTilesBrinstar or SpriteSheetId.AreaMapBackgroundTilesNorfair or
            SpriteSheetId.AreaMapBackgroundTilesWreckedShip or SpriteSheetId.AreaMapBackgroundTilesMaridia or
            SpriteSheetId.AreaMapBackgroundTilesTourian or SpriteSheetId.PauseMapTiles or
            SpriteSheetId.PauseInterfaceTiles or SpriteSheetId.PauseHud or SpriteSheetId.IntroSceneTiles or
            SpriteSheetId.IntroFontTiles or SpriteSheetId.CeresFlightObjectTiles or
            SpriteSheetId.CeresIntroObjectTiles or SpriteSheetId.ZebesRevealTiles or
            SpriteSheetId.EndingCreditsFontTiles or SpriteSheetId.EndingTextFontTiles or
            SpriteSheetId.EndingSubtitleTiles or SpriteSheetId.EndingLogoTiles or SpriteSheetId.EndingWaitingTiles or
            SpriteSheetId.EndingWavySamusTiles or SpriteSheetId.EndingExplosionTiles or
            SpriteSheetId.TitleFloorTiles or SpriteSheetId.TitleBabyTiles or SpriteSheetId.CeresFlightFloorTiles or
            SpriteSheetId.EndingEscapeAFloorTiles or SpriteSheetId.EndingEscapeBFloorTiles or
            SpriteSheetId.EndingPlanetExplosionFloorTiles or SpriteSheetId.EndingFlyawayFloorTiles or
            SpriteSheetId.EndingRewardIconFloorTiles =>
            SamusSheetSources.For(samus.Value, sheet),
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
    };
}
