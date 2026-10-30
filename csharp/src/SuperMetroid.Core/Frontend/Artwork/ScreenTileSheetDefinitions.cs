using SuperMetroid.Core.Assets;
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
    public static IReadOnlyDictionary<SpriteSheetId, TileInkDepth> Depths { get; } = new Dictionary<SpriteSheetId, TileInkDepth>
    {
        [SpriteSheetId.MenuForegroundTiles] = TileInkDepth.SixteenColours,
        [SpriteSheetId.MenuMapTiles] = TileInkDepth.SixteenColours,
        [SpriteSheetId.AreaMapForegroundTilesCrateria] = TileInkDepth.SixteenColours,
        [SpriteSheetId.AreaMapForegroundTilesBrinstar] = TileInkDepth.SixteenColours,
        [SpriteSheetId.AreaMapForegroundTilesNorfair] = TileInkDepth.SixteenColours,
        [SpriteSheetId.AreaMapForegroundTilesWreckedShip] = TileInkDepth.SixteenColours,
        [SpriteSheetId.AreaMapForegroundTilesMaridia] = TileInkDepth.SixteenColours,
        [SpriteSheetId.AreaMapForegroundTilesTourian] = TileInkDepth.SixteenColours,
        [SpriteSheetId.AreaMapBackgroundTilesCrateria] = TileInkDepth.FourColours,
        [SpriteSheetId.AreaMapBackgroundTilesBrinstar] = TileInkDepth.FourColours,
        [SpriteSheetId.AreaMapBackgroundTilesNorfair] = TileInkDepth.FourColours,
        [SpriteSheetId.AreaMapBackgroundTilesWreckedShip] = TileInkDepth.FourColours,
        [SpriteSheetId.AreaMapBackgroundTilesMaridia] = TileInkDepth.FourColours,
        [SpriteSheetId.AreaMapBackgroundTilesTourian] = TileInkDepth.FourColours,
        [SpriteSheetId.PauseMapTiles] = TileInkDepth.SixteenColours,
        [SpriteSheetId.PauseInterfaceTiles] = TileInkDepth.SixteenColours,
        [SpriteSheetId.IntroFontTiles] = TileInkDepth.FourColours,
    };

    /// <summary>The installed sheets of frontend turning floors' pictures (#1392, #1467).</summary>
    public static IReadOnlyList<SpriteSheetId> FloorSheets { get; } =
    [
        SpriteSheetId.TitleFloor, SpriteSheetId.CeresFloor, SpriteSheetId.EndingEscapeAFloor,
        SpriteSheetId.EndingEscapeBFloor, SpriteSheetId.EndingPlanetExplosionFloor, SpriteSheetId.EndingFlyawayFloor,
        SpriteSheetId.EndingRewardIconFloor,
    ];

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
