namespace SuperMetroid.Core.Spatial;

/// <summary>Named conversions between room pixels and the camera-relative screen space.</summary>
public static class RoomScreenConversions
{
    /// <summary>
    /// The screen position of a room position under the layer-1 camera: the native
    /// <c>position - layer-1 scroll</c> subtraction, wrapping each axis at sixteen bits.
    /// </summary>
    public static Position16<ScreenPixels> ToScreen(this Position16<RoomPixels> position, Position16<RoomPixels> camera) =>
        new(unchecked((ushort)(position.X - camera.X)), unchecked((ushort)(position.Y - camera.Y)));
}
