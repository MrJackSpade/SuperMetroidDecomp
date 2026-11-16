namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// What the screen shows of a boss body (#1617, #2031): every boss places its body at a room pixel, its position, against
/// which each installed frame lies, and the display shows the body from there against a camera, the room pixel at the
/// screen's top-left.
/// </summary>
public static class BossBodyPlacement
{
    /// <summary>
    /// The pixel of a body, reckoned from its <paramref name="placement"/>, that <paramref name="camera"/> shows at the
    /// screen's top-left.
    /// </summary>
    public static Position16<BackgroundPixels> Shown(Position16<RoomPixels> placement, Position16<RoomPixels> camera) =>
        new(CoordinateAxis.Wrap(camera.X - placement.X), CoordinateAxis.Wrap(camera.Y - placement.Y));
}
