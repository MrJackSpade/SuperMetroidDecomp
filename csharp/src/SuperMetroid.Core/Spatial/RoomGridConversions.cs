namespace SuperMetroid.Core.Spatial;

/// <summary>Conversions between the room's 256-pixel screen grid and room pixels.</summary>
public static class RoomGridConversions
{
    /// <summary>
    /// The room-pixel origin of a screen cell: each axis shifted left eight bits, wrapping at
    /// sixteen bits.
    /// </summary>
    public static Position16<RoomPixels> ToRoomPixels(this Position16<RoomScreens> screen) =>
        new(unchecked((ushort)(screen.X << 8)), unchecked((ushort)(screen.Y << 8)));
}
