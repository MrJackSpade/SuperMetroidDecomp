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
}
