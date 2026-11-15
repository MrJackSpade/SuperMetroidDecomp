namespace SuperMetroid.Core.Spatial;

/// <summary>Conversions from room pixels to the viewport the camera selects.</summary>
[EncodingOwner("screen and sprite coordinates")]
public static class ScreenConversions
{
    /// <summary>
    /// The screen-relative position of <paramref name="room"/> when the camera's top-left corner is
    /// <paramref name="camera"/>: each axis is the difference wrapped around the room's 65536-pixel coordinates and read as
    /// signed, so positions left of or above the viewport are negative.
    /// </summary>
    public static Offset16<ScreenPixels> RelativeToCamera(this Position16<RoomPixels> room, Position16<RoomPixels> camera)
    {
        Offset16<RoomPixels> delta = room - camera;
        return new Offset16<ScreenPixels>(delta.X, delta.Y);
    }

    /// <summary>
    /// The screen position of <paramref name="room"/> under the camera: the game's
    /// position less camera subtraction kept as wrapped unsigned coordinates.
    /// </summary>
    public static Position16<ScreenPixels> ToScreen(this Position16<RoomPixels> room, Position16<RoomPixels> camera) =>
        new(unchecked((ushort)(room.X - camera.X)), unchecked((ushort)(room.Y - camera.Y)));
}
