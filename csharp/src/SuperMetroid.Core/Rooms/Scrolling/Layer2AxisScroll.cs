namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// One axis of a room state's layer-2 scroll: how it follows the camera, its parallax factor and whether the background
/// streamer refreshes that axis's tilemap as it moves.
/// </summary>
/// <param name="Kind">How the axis follows the camera.</param>
/// <param name="Factor">The parallax multiplier over 256, an even number from 2 through 254; zero for the other kinds.</param>
/// <param name="Streams">Whether the background streamer refreshes the axis's tilemap as layer two moves.</param>
public readonly record struct Layer2AxisScroll(Layer2ScrollKind Kind, int Factor, bool Streams)
{
    /// <summary>An axis that copies layer one and streams its tilemap, as a cleared mode byte selects.</summary>
    public static readonly Layer2AxisScroll CopiesLayer1 = new(Layer2ScrollKind.CopiesLayer1, 0, Streams: true);

    /// <summary>
    /// Decodes a room header's per-axis mode byte: zero copies layer one, one fixes the axis, and any other value is a
    /// parallax multiplier over 256 whose lowest bit, when set, stops the axis's tilemap streaming.
    /// </summary>
    /// <param name="mode">The header's mode byte, 0 through 255.</param>
    public static Layer2AxisScroll FromHeader(int mode)
    {
        Ensure.BetweenInclusive(0, byte.MaxValue, mode);
        bool streams = mode % 2 == 0;
        return mode switch
        {
            0 => new(Layer2ScrollKind.CopiesLayer1, 0, streams),
            1 => new(Layer2ScrollKind.Fixed, 0, streams),
            _ => new(Layer2ScrollKind.Parallax, mode - mode % 2, streams),
        };
    }
}
