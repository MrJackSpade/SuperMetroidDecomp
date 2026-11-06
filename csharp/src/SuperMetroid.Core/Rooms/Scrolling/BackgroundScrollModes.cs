namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>A room state's background scroll, one decoded <see cref="BackgroundAxisScroll"/> per axis.</summary>
/// <param name="Horizontal">How the horizontal axis follows the camera.</param>
/// <param name="Vertical">How the vertical axis follows the camera.</param>
[SpatialComponents("Spatial value type: one background parallax mode per axis, as the room header stores them.")]
public readonly record struct BackgroundScrollModes(BackgroundAxisScroll Horizontal, BackgroundAxisScroll Vertical)
{
    /// <summary>Both axes copy the foreground and stream, as cleared mode bytes select.</summary>
    public static readonly BackgroundScrollModes FollowsForeground = new(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground);

    /// <summary>Decodes a room header's horizontal and vertical mode bytes.</summary>
    /// <param name="horizontal">The header's horizontal mode byte.</param>
    /// <param name="vertical">The header's vertical mode byte.</param>
    public static BackgroundScrollModes FromHeader(int horizontal, int vertical) =>
        new(BackgroundAxisScroll.FromHeader(horizontal), BackgroundAxisScroll.FromHeader(vertical));
}
