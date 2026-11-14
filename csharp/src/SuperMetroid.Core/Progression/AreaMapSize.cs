namespace SuperMetroid.Core.Progression;

/// <summary>The size of one area's map: 64 map tiles across and 32 down.</summary>
public static class AreaMapSize
{
    /// <summary>Map tiles across one area's map.</summary>
    public const int WidthInMapTiles = 64;
    /// <summary>Map tiles down one area's map.</summary>
    public const int HeightInMapTiles = 32;

    /// <summary>One area's map, <see cref="WidthInMapTiles"/> by <see cref="HeightInMapTiles"/> map tiles.</summary>
    public static Extent16<MapTiles> Extent => new(WidthInMapTiles, HeightInMapTiles);
}
