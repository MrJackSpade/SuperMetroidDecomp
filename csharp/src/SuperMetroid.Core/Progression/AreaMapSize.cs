namespace SuperMetroid.Core.Progression;

/// <summary>The size of one area's map: 64 map tiles across and 32 down.</summary>
public static class AreaMapSize
{
    /// <summary>Map tiles across one area's map.</summary>
    public const int WidthInTiles = 64;
    /// <summary>Map tiles down one area's map.</summary>
    public const int HeightInTiles = 32;

    /// <summary>One area's map, <see cref="WidthInTiles"/> by <see cref="HeightInTiles"/> map tiles.</summary>
    public static Extent16<MapTiles> Extent => new(WidthInTiles, HeightInTiles);
}
