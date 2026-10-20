namespace SuperMetroid.Core.Spatial;

/// <summary>Conversions from room pixels to the viewport the camera selects, and from the viewport to OAM.</summary>
[SnesEncodingOwner]
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

    /// <summary>
    /// The screen position word pair of <paramref name="room"/> under the camera: the native
    /// <c>position - layer-1 scroll</c> subtraction kept as wrapped unsigned words, as WRAM stores it.
    /// </summary>
    public static Position16<ScreenPixels> ToScreen(this Position16<RoomPixels> room, Position16<RoomPixels> camera) =>
        new(unchecked((ushort)(room.X - camera.X)), unchecked((ushort)(room.Y - camera.Y)));

    /// <summary>
    /// The OAM coordinate of a screen position: sprite coordinates are screen pixels, so the words are
    /// unchanged and the OAM writer applies its nine-bit X and eight-bit Y range rules.
    /// </summary>
    public static Position16<OamPixels> ToOam(this Position16<ScreenPixels> screen) => new(screen.X, screen.Y);

    /// <summary>The whole screen pixels of an unsigned 8.8 screen position: each axis's high byte.</summary>
    public static Position16<ScreenPixels> WholePixels(this Position16<ScreenSubpixels> position) =>
        new((ushort)(position.X >> 8), (ushort)(position.Y >> 8));
}
