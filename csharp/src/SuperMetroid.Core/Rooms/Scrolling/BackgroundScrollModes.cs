namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>A room state's background: how each axis of it follows the camera, and what it shows.</summary>
/// <param name="Horizontal">How the horizontal axis follows the camera.</param>
/// <param name="Vertical">How the vertical axis follows the camera.</param>
/// <param name="Source">Whether the background shows the room's level background or what its owner places there.</param>
[SpatialComponents("Spatial value type: one background parallax mode per axis, and what the background shows.")]
public readonly record struct BackgroundScrollModes(BackgroundAxisScroll Horizontal, BackgroundAxisScroll Vertical, RoomBackgroundSource Source)
{
    /// <summary>Both axes copy the foreground, and the room's level background shows.</summary>
    public static readonly BackgroundScrollModes FollowsForeground =
        new(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level);
}
