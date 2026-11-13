namespace SuperMetroid.Core.Spatial;

/// <summary>The room effects' relations between the foreground and the effect overlay.</summary>
[EncodingOwner("effect-overlay effect plane coordinates")]
public static class FxOverlayConversions
{
    /// <summary>
    /// The effect overlay's scroll when it is anchored to the foreground: the camera position
    /// unchanged. Rain, fog, spores and the liquid surfaces all start from this anchor.
    /// </summary>
    public static Position16<OverlayPixels> AsOverlayScroll(this Position16<RoomPixels> cameraPosition) => new(cameraPosition.X, cameraPosition.Y);

    /// <summary>The whole effect overlay pixels of an 8.8 drift: each axis's signed whole part.</summary>
    public static Offset16<OverlayPixels> WholePixels(this Offset16<OverlaySubpixels> drift) =>
        new(unchecked((sbyte)(drift.X >> 8)), unchecked((sbyte)(drift.Y >> 8)));
}
