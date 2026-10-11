namespace SuperMetroid.Core.Spatial;

/// <summary>Conversions from room pixels to the viewport the camera selects.</summary>
public static class ScreenConversions
{
    /// <summary>
    /// The screen-relative position of <paramref name="room"/> when the camera's top-left corner is
    /// <paramref name="camera"/>: each axis is the native wrapped sixteen-bit difference read as
    /// signed, so positions left of or above the viewport are negative.
    /// </summary>
    public static Offset16<ScreenPixels> RelativeToCamera(this Position16<RoomPixels> room, Position16<RoomPixels> camera)
    {
        Offset16<RoomPixels> delta = room - camera;
        return new Offset16<ScreenPixels>(delta.X, delta.Y);
    }
}
