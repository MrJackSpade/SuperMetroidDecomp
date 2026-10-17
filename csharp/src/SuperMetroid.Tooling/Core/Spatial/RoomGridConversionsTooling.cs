using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Spatial;

/// <summary>Development-tool members of <see cref="RoomGridConversions"/>; never linked by player hosts.</summary>
[SnesEncodingOwner]
internal static class RoomGridConversionsTooling
{
    /// <summary>
    /// The screen cell containing an unsigned room pixel: a room screen is 256 pixels on each axis, so the screen
    /// index is each coordinate's upper eight bits.
    /// </summary>
    public static Position16<RoomScreens> ToRoomScreen(this Position16<RoomPixels> position) =>
        new((ushort)(position.X >> 8), (ushort)(position.Y >> 8));
}
