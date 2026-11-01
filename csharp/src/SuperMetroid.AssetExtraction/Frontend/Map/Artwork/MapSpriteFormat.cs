using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

/// <summary>Defines the map sprite document and atlas geometry.</summary>
public static class MapSpriteFormat
{
    /// <summary>Supported map sprite schema revision.</summary>
    public const int Version = 1;
    /// <summary>JSON composition file for map markers and world labels.</summary>
    public const string JsonFile = "map-sprites.json";
    /// <summary>Indexed PNG file for shared map object characters.</summary>
    public const string PngFile = "map-objects.png";
    /// <summary>Sixteen eight-pixel character columns in the map object atlas.</summary>
    public const int TileColumns = 16;
    /// <summary>Sixteen eight-pixel character rows in the map object atlas.</summary>
    public const int TileRows = 16;
    /// <summary>Atlas width derived from its character columns.</summary>
    public const int Width = TileColumns * 8;
    /// <summary>Atlas height derived from its character rows.</summary>
    public const int Height = TileRows * 8;
    /// <summary>Largest supported number of ordered OBJ parts in one composition.</summary>
    public const int MaximumParts = 128;
    /// <summary>Compiled byte count of the 128-by-128-pixel 4-bpp atlas.</summary>
    public const int ByteCount = Width * Height / 2;

    /// <summary>Required named compositions in the map-sprite document, one per <see cref="MapSpriteId"/>.</summary>
    public const int FrameCount = 26;

    /// <summary>The named roles in the established presentation-document order.</summary>
    internal static readonly MapSpriteId[] FrameOrder =
    [
        MapSpriteId.ArrowRight,
        MapSpriteId.ArrowLeft,
        MapSpriteId.ArrowUp,
        MapSpriteId.ArrowDown,
        MapSpriteId.MarkerBoss,
        MapSpriteId.StationEnergy,
        MapSpriteId.StationMissile,
        MapSpriteId.StationMap,
        MapSpriteId.MarkerDefeatedBoss,
        MapSpriteId.MarkerGunship,
        MapSpriteId.IndicatorBacking,
        MapSpriteId.IndicatorFrame0,
        MapSpriteId.IndicatorFrame1,
        MapSpriteId.IndicatorFrame2,
        MapSpriteId.ElevatorCrateria,
        MapSpriteId.ElevatorBrinstar,
        MapSpriteId.ElevatorNorfair,
        MapSpriteId.ElevatorWreckedShip,
        MapSpriteId.ElevatorMaridia,
        MapSpriteId.WorldTitle,
        MapSpriteId.WorldCrateria,
        MapSpriteId.WorldBrinstar,
        MapSpriteId.WorldNorfair,
        MapSpriteId.WorldWreckedShip,
        MapSpriteId.WorldMaridia,
        MapSpriteId.WorldTourian,
    ];
}
