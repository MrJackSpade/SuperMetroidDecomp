namespace SuperMetroid.Core.Spatial;

/// <summary>The bank-$88 room-FX relations between layer one and the effect overlay effect plane.</summary>
[EncodingOwner("effect-overlay effect plane coordinates")]
public static class FxOverlayConversions
{
    /// <summary>
    /// The effect overlay scroll words of an effect plane anchored to layer one: the layer-one position
    /// words unchanged. Rain, fog, spores and the liquid surfaces all start from this anchor.
    /// </summary>
    public static Position16<OverlayPixels> AsOverlayScroll(this Position16<RoomPixels> layer1) => new(layer1.X, layer1.Y);

    /// <summary>The whole effect overlay pixels of an 8.8 drift accumulator: each axis's signed high byte.</summary>
    public static Offset16<OverlayPixels> WholePixels(this Offset16<OverlaySubpixels> drift) =>
        new(unchecked((sbyte)(drift.X >> 8)), unchecked((sbyte)(drift.Y >> 8)));
}
