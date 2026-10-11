namespace SuperMetroid.Core.Spatial;

/// <summary>Named conversions between room pixels and the room's block and screen grids.</summary>
public static class RoomGridConversions
{
    /// <summary>Blocks per room screen on each axis.</summary>
    public const int BlocksPerScreen = 16;

    /// <summary>
    /// The block containing an unsigned room pixel, exactly as the native <c>LSR ×4</c> collision
    /// paths derive it from each sixteen-bit coordinate word.
    /// </summary>
    public static Point32<RoomBlocks> ToRoomBlock(this Position16<RoomPixels> position) =>
        new(position.X >> 4, position.Y >> 4);

    /// <summary>The top-left room pixel of a screen (<c>screen &lt;&lt; 8</c>), wrapping each axis at sixteen bits.</summary>
    public static Position16<RoomPixels> ToRoomPixels(this Point32<RoomScreens> screen) =>
        new(unchecked((ushort)(screen.X << 8)), unchecked((ushort)(screen.Y << 8)));

    /// <summary>A screen-grid size measured in blocks.</summary>
    public static Size32<RoomBlocks> ToRoomBlocks(this Size32<RoomScreens> size) =>
        new(size.Width * BlocksPerScreen, size.Height * BlocksPerScreen);
}
