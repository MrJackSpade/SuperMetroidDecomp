namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// Where a boss body lies in the room and what the screen shows of it (#1617): the room pixel its parts' area's top-left
/// corner lies on, against a camera, the room pixel at the screen's top-left.
/// </summary>
public static class BossBodyPlacement
{
    /// <summary>
    /// The room pixel a body's area's top-left corner lies on where <paramref name="camera"/> shows its pixel
    /// <paramref name="shown"/> at the screen's top-left.
    /// </summary>
    public static Position16<RoomPixels> InRoom(Position16<BackgroundPixels> shown, Position16<RoomPixels> camera) =>
        new(CoordinateAxis.Wrap(camera.X - shown.X), CoordinateAxis.Wrap(camera.Y - shown.Y));

    /// <summary>The pixel of a body's area <paramref name="camera"/> shows at the screen's top-left where its area lies at <paramref name="origin"/>.</summary>
    public static Position16<BackgroundPixels> Shown(Position16<RoomPixels> origin, Position16<RoomPixels> camera) =>
        new(CoordinateAxis.Wrap(camera.X - origin.X), CoordinateAxis.Wrap(camera.Y - origin.Y));
}
