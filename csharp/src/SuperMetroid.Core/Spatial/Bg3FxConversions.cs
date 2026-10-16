using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Spatial;

/// <summary>The bank-$88 room-FX relations between layer one and the BG3 effect plane.</summary>
public static class Bg3FxConversions
{
    /// <summary>
    /// The BG3 scroll words of an effect plane anchored to layer one: the layer-one position
    /// words unchanged. Rain, fog, spores and the liquid surfaces all start from this anchor.
    /// </summary>
    public static Position16<Bg3Pixels> AsBg3Scroll(this Position16<RoomPixels> layer1) => new(layer1.X, layer1.Y);

    /// <summary>The whole BG3 pixels of an 8.8 drift accumulator: each axis's signed high byte.</summary>
    public static Offset16<Bg3Pixels> WholePixels(this Offset16<Bg3Subpixels> drift) =>
        new(unchecked((sbyte)(drift.X >> 8)), unchecked((sbyte)(drift.Y >> 8)));
}
