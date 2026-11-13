using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Artwork;

/// <summary>
/// The installed sheets of the menu's world-map pages (#1392, #1467).
/// </summary>
public static class ScreenTileSheetDefinitions
{
    /// <summary>The file-select world map's foreground picture in each Zebes area's world-map colours, shown while that area is selected.</summary>
    public static IReadOnlyDictionary<AreaId, SpriteSheetId> AreaMapForegrounds { get; } = new Dictionary<AreaId, SpriteSheetId>
    {
        [AreaId.Crateria] = SpriteSheetId.AreaMapForegroundCrateria,
        [AreaId.Brinstar] = SpriteSheetId.AreaMapForegroundBrinstar,
        [AreaId.Norfair] = SpriteSheetId.AreaMapForegroundNorfair,
        [AreaId.WreckedShip] = SpriteSheetId.AreaMapForegroundWreckedShip,
        [AreaId.Maridia] = SpriteSheetId.AreaMapForegroundMaridia,
        [AreaId.Tourian] = SpriteSheetId.AreaMapForegroundTourian,
    };

    /// <summary>The file-select world map's background picture in each Zebes area's world-map colours, shown while that area is selected.</summary>
    public static IReadOnlyDictionary<AreaId, SpriteSheetId> AreaMapBackgrounds { get; } = new Dictionary<AreaId, SpriteSheetId>
    {
        [AreaId.Crateria] = SpriteSheetId.AreaMapBackgroundCrateria,
        [AreaId.Brinstar] = SpriteSheetId.AreaMapBackgroundBrinstar,
        [AreaId.Norfair] = SpriteSheetId.AreaMapBackgroundNorfair,
        [AreaId.WreckedShip] = SpriteSheetId.AreaMapBackgroundWreckedShip,
        [AreaId.Maridia] = SpriteSheetId.AreaMapBackgroundMaridia,
        [AreaId.Tourian] = SpriteSheetId.AreaMapBackgroundTourian,
    };
}
