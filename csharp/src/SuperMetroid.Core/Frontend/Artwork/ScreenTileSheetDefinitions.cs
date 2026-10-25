using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Artwork;

/// <summary>
/// The installed sheets of frontend background characters (#1392), each one artwork's characters pre-coloured with the
/// standard palette of the screen that shows it, and the bit depth of each one's characters.
/// </summary>
public static class ScreenTileSheetDefinitions
{
    /// <summary>Each screen tile sheet's character depth.</summary>
    public static IReadOnlyDictionary<SpriteSheetId, BgBitDepth> Depths { get; } = new Dictionary<SpriteSheetId, BgBitDepth>
    {
        [SpriteSheetId.MenuForegroundTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.MenuMapTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.AreaMapForegroundTilesCrateria] = BgBitDepth.FourBpp,
        [SpriteSheetId.AreaMapForegroundTilesBrinstar] = BgBitDepth.FourBpp,
        [SpriteSheetId.AreaMapForegroundTilesNorfair] = BgBitDepth.FourBpp,
        [SpriteSheetId.AreaMapForegroundTilesWreckedShip] = BgBitDepth.FourBpp,
        [SpriteSheetId.AreaMapForegroundTilesMaridia] = BgBitDepth.FourBpp,
        [SpriteSheetId.AreaMapForegroundTilesTourian] = BgBitDepth.FourBpp,
        [SpriteSheetId.AreaMapBackgroundTilesCrateria] = BgBitDepth.TwoBpp,
        [SpriteSheetId.AreaMapBackgroundTilesBrinstar] = BgBitDepth.TwoBpp,
        [SpriteSheetId.AreaMapBackgroundTilesNorfair] = BgBitDepth.TwoBpp,
        [SpriteSheetId.AreaMapBackgroundTilesWreckedShip] = BgBitDepth.TwoBpp,
        [SpriteSheetId.AreaMapBackgroundTilesMaridia] = BgBitDepth.TwoBpp,
        [SpriteSheetId.AreaMapBackgroundTilesTourian] = BgBitDepth.TwoBpp,
        [SpriteSheetId.PauseMapTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.PauseInterfaceTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.PauseHudTiles] = BgBitDepth.TwoBpp,
        [SpriteSheetId.IntroSceneTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.IntroFontTiles] = BgBitDepth.TwoBpp,
        [SpriteSheetId.CeresFlightObjectTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.CeresIntroObjectTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.ZebesRevealTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.EndingCreditsFontTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.EndingTextFontTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.EndingSubtitleTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.EndingLogoTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.EndingWaitingTiles] = BgBitDepth.FourBpp,
        [SpriteSheetId.EndingWavySamusTiles] = BgBitDepth.TwoBpp,
        [SpriteSheetId.EndingExplosionTiles] = BgBitDepth.FourBpp,
    };

    /// <summary>The installed sheets of frontend Mode 7 characters (#1392).</summary>
    public static IReadOnlyList<SpriteSheetId> FloorSheets { get; } =
    [
        SpriteSheetId.TitleFloorTiles, SpriteSheetId.TitleBabyTiles, SpriteSheetId.CeresFlightFloorTiles, SpriteSheetId.EndingEscapeAFloorTiles,
        SpriteSheetId.EndingEscapeBFloorTiles, SpriteSheetId.EndingPlanetExplosionFloorTiles, SpriteSheetId.EndingFlyawayFloorTiles,
        SpriteSheetId.EndingRewardIconFloorTiles,
    ];

    /// <summary>Each ending Mode 7 scene's characters' sheet.</summary>
    public static IReadOnlyDictionary<EndingFloorSceneId, SpriteSheetId> EndingFloorScenes { get; } = new Dictionary<EndingFloorSceneId, SpriteSheetId>
    {
        [EndingFloorSceneId.EscapeA] = SpriteSheetId.EndingEscapeAFloorTiles,
        [EndingFloorSceneId.EscapeB] = SpriteSheetId.EndingEscapeBFloorTiles,
        [EndingFloorSceneId.PlanetExplosion] = SpriteSheetId.EndingPlanetExplosionFloorTiles,
    };

    /// <summary>The menu's world foreground characters under each Zebes area's world-map palette, which its area map shows.</summary>
    public static IReadOnlyDictionary<AreaId, SpriteSheetId> AreaMapForegrounds { get; } = new Dictionary<AreaId, SpriteSheetId>
    {
        [AreaId.Crateria] = SpriteSheetId.AreaMapForegroundTilesCrateria,
        [AreaId.Brinstar] = SpriteSheetId.AreaMapForegroundTilesBrinstar,
        [AreaId.Norfair] = SpriteSheetId.AreaMapForegroundTilesNorfair,
        [AreaId.WreckedShip] = SpriteSheetId.AreaMapForegroundTilesWreckedShip,
        [AreaId.Maridia] = SpriteSheetId.AreaMapForegroundTilesMaridia,
        [AreaId.Tourian] = SpriteSheetId.AreaMapForegroundTilesTourian,
    };

    /// <summary>The menu's world background characters under each Zebes area's world-map palette, which its area map shows.</summary>
    public static IReadOnlyDictionary<AreaId, SpriteSheetId> AreaMapBackgrounds { get; } = new Dictionary<AreaId, SpriteSheetId>
    {
        [AreaId.Crateria] = SpriteSheetId.AreaMapBackgroundTilesCrateria,
        [AreaId.Brinstar] = SpriteSheetId.AreaMapBackgroundTilesBrinstar,
        [AreaId.Norfair] = SpriteSheetId.AreaMapBackgroundTilesNorfair,
        [AreaId.WreckedShip] = SpriteSheetId.AreaMapBackgroundTilesWreckedShip,
        [AreaId.Maridia] = SpriteSheetId.AreaMapBackgroundTilesMaridia,
        [AreaId.Tourian] = SpriteSheetId.AreaMapBackgroundTilesTourian,
    };
}
