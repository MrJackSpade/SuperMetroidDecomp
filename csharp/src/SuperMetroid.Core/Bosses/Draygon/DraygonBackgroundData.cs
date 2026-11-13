namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Where Draygon's body image is placed on his body layer, and the bands of lines that layer shows on.</summary>
internal static class DraygonBackgroundData
{
    /// <summary>The body layer shows on no line while the body's screen X is below -64 or at or past 320.</summary>
    public const int MinimumScreenX = -64, MaximumScreenX = 320;
    /// <summary>The body layer shows on no line while the body's screen Y is below -16 or at or past 304.</summary>
    public const int MinimumScreenY = -16, MaximumScreenY = 304;
    /// <summary>A body above screen Y 40 shows the top band, one above 192 the whole gameplay area, and lower ones the bottom band.</summary>
    public const int MiddleScreenY = 40, BottomScreenY = 192;
    /// <summary>The top band runs from below the HUD to line 96.</summary>
    public const int TopBandEnd = 96;
    /// <summary>The bottom band runs from line 128 to the bottom of the screen.</summary>
    public const int BottomBandStart = 128;
    /// <summary>
    /// How far right of his body position the top-left corner of his body's parts' area lies, in pixels: 62 to the left
    /// (#1680). The original's body layer repeated every 512 pixels across, so it placed the area 450 to the right, the same
    /// place on that repeat; his body now lies once in the room (#1617), so the area lies where the body shows.
    /// </summary>
    public const int HorizontalOrigin = -62;

    /// <summary>
    /// How far below his body position the top-left corner of his body's parts' area lies, in pixels: 64 above (#1680). The
    /// original's body layer repeated every 256 pixels down, so it placed the area 192 below, the same place on that repeat.
    /// </summary>
    public const int VerticalOrigin = -64;
}
