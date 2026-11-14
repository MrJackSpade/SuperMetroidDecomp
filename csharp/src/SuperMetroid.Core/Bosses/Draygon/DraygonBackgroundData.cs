namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Where Draygon's body image is placed. The image is drawn whole where it lies in the room (#1873): the original showed
/// his body layer only on a band of lines around him to hide the layer's repeat, which the port's single body does not
/// have.
/// </summary>
internal static class DraygonBackgroundData
{
    /// <summary>
    /// How far right of his body position the top-left corner of his body's parts' area lies, in pixels: 62 to the left
    /// (#1680). The game's body layer repeated every 512 pixels across, so it placed the area 450 to the right, the same
    /// place on that repeat; his body now lies once in the room (#1617), so the area lies where the body shows.
    /// </summary>
    public const int HorizontalOrigin = -62;

    /// <summary>
    /// How far below his body position the top-left corner of his body's parts' area lies, in pixels: 64 above (#1680). The
    /// original's body layer repeated every 256 pixels down, so it placed the area 192 below, the same place on that repeat.
    /// </summary>
    public const int VerticalOrigin = -64;
}
