using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Decodes a room state's two layer-2 scroll bytes once, at room-state import (#1630). The game holds the result as a
/// <see cref="BackgroundScrollModes"/> and never sees either byte.
/// </summary>
/// <remarks>
/// Each byte decodes one axis: zero copies the foreground, one fixes the axis, and any other value is a parallax
/// multiplier over 256 whose lowest bit, when set, stops the background streamer refreshing that axis's tilemap. The
/// background shows the room's level data when either axis streams (the two agree in every retail room state);
/// otherwise it shows what its owner places there.
/// </remarks>
internal static class BackgroundScrollCodec
{
    /// <summary>Decodes a room state's horizontal and vertical scroll bytes.</summary>
    /// <param name="horizontal">The room state's horizontal layer-2 scroll byte.</param>
    /// <param name="vertical">The room state's vertical layer-2 scroll byte.</param>
    /// <returns>The room state's background scroll and what its background shows.</returns>
    public static BackgroundScrollModes Decode(byte horizontal, byte vertical) =>
        new(Axis(horizontal), Axis(vertical), Streams(horizontal) || Streams(vertical) ? RoomBackgroundSource.Level : RoomBackgroundSource.Placed);

    /// <summary>Decodes one axis's scroll byte.</summary>
    private static BackgroundAxisScroll Axis(byte mode) => mode switch
    {
        0 => BackgroundAxisScroll.FollowsForeground,
        1 => BackgroundAxisScroll.Fixed,
        _ => BackgroundAxisScroll.Parallax(mode - mode % 2),
    };

    /// <summary>Whether the background streamer refreshes the axis's tilemap: the byte's lowest bit is clear.</summary>
    private static bool Streams(byte mode) => mode % 2 == 0;
}
