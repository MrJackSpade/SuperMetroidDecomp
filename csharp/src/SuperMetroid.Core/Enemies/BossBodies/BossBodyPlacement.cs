namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// What the screen shows of a boss body (#1617, #1802): every boss holds the room pixel its body's parts' area's top-left
/// corner lies on, and the display shows that area against a camera, the room pixel at the screen's top-left.
/// </summary>
public static class BossBodyPlacement
{
    /// <summary>The pixel of a body's area <paramref name="camera"/> shows at the screen's top-left where its area lies at <paramref name="origin"/>.</summary>
    public static Position16<BackgroundPixels> Shown(Position16<RoomPixels> origin, Position16<RoomPixels> camera) =>
        new(CoordinateAxis.Wrap(camera.X - origin.X), CoordinateAxis.Wrap(camera.Y - origin.Y));
}
