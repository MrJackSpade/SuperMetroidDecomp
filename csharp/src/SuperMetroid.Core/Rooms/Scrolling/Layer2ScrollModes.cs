namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>A room state's layer-2 scroll, one decoded <see cref="Layer2AxisScroll"/> per axis.</summary>
/// <param name="Horizontal">How the horizontal axis follows the camera.</param>
/// <param name="Vertical">How the vertical axis follows the camera.</param>
[SpatialComponents("Spatial value type: one layer-2 parallax mode per axis, as the room header stores them.")]
public readonly record struct Layer2ScrollModes(Layer2AxisScroll Horizontal, Layer2AxisScroll Vertical)
{
    /// <summary>Both axes copy layer one and stream, as cleared mode bytes select.</summary>
    public static readonly Layer2ScrollModes CopiesLayer1 = new(Layer2AxisScroll.CopiesLayer1, Layer2AxisScroll.CopiesLayer1);

    /// <summary>Decodes a room header's horizontal and vertical mode bytes.</summary>
    /// <param name="horizontal">The header's horizontal mode byte.</param>
    /// <param name="vertical">The header's vertical mode byte.</param>
    public static Layer2ScrollModes FromHeader(int horizontal, int vertical) =>
        new(Layer2AxisScroll.FromHeader(horizontal), Layer2AxisScroll.FromHeader(vertical));
}
