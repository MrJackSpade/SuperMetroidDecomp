using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Spatial;

/// <summary>Named conversions between room pixels and the room's block and screen grids.</summary>
[SnesEncodingOwner]
public static class RoomGridConversions
{
    /// <summary>Blocks per room screen on each axis.</summary>
    public const int BlocksPerScreen = 16;

    /// <summary>
    /// The block containing an unsigned room pixel, exactly as the native <c>LSR ×4</c> collision
    /// paths derive it from each sixteen-bit coordinate word.
    /// </summary>
    public static Position16<RoomBlocks> ToRoomBlock(this Position16<RoomPixels> position) =>
        new((ushort)(position.X >> 4), (ushort)(position.Y >> 4));

    /// <summary>
    /// The room-pixel top-left corner of a block: each axis shifted left four bits, wrapping at
    /// sixteen bits, as the native <c>ASL ×4</c> block-to-pixel paths do.
    /// </summary>
    public static Position16<RoomPixels> ToRoomPixels(this Position16<RoomBlocks> block) =>
        new(unchecked((ushort)(block.X << 4)), unchecked((ushort)(block.Y << 4)));

    /// <summary>
    /// The room-pixel origin of a screen cell: each axis shifted left eight bits, wrapping at
    /// sixteen bits.
    /// </summary>
    public static Position16<RoomPixels> ToRoomPixels(this Position16<RoomScreens> screen) =>
        new(unchecked((ushort)(screen.X << 8)), unchecked((ushort)(screen.Y << 8)));

    /// <summary>The room-pixel offset of a screen index along one axis: a room screen is 256 pixels.</summary>
    public static int ScreenToPixels(int screen) => screen << 8;

    /// <summary>A screen-grid size measured in blocks.</summary>
    public static Extent16<RoomBlocks> ToRoomBlocks(this Extent16<RoomScreens> size) =>
        new(checked((ushort)(size.X * BlocksPerScreen)), checked((ushort)(size.Y * BlocksPerScreen)));
}
