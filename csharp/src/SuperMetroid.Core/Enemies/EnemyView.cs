namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemies' view of the room: the square the screen shows, <see cref="Extent"/> pixels wide and tall from the
/// camera's top-left corner. Enemy projectiles leave the room once outside it, some enemies sound only inside it, and an
/// enemy counts as on screen by it. A point's place is measured from the camera across the wrapping room space, so a
/// point far enough above or left of the camera reads as outside.
/// </summary>
internal static class EnemyView
{
    /// <summary>The view's width and height in pixels: the screen's.</summary>
    internal const int Extent = 256;

    /// <summary>Whether <paramref name="point"/> lies in the view, its right and bottom edges included.</summary>
    internal static bool Contains(Position16<RoomPixels> point, Position16<RoomPixels> camera) =>
        ContainsWithMargin(point, camera, 0);

    /// <summary>
    /// Whether <paramref name="point"/> lies in the view widened by <paramref name="margin"/> pixels on every side, its
    /// right and bottom edges included.
    /// </summary>
    internal static bool ContainsWithMargin(Position16<RoomPixels> point, Position16<RoomPixels> camera, int margin) =>
        !CoordinateAxis.Precedes(point.X + margin, camera.X) &&
        !CoordinateAxis.Precedes(camera.X + Extent + margin, point.X) &&
        !CoordinateAxis.Precedes(point.Y + margin, camera.Y) &&
        !CoordinateAxis.Precedes(camera.Y + Extent + margin, point.Y);

    /// <summary>Whether <paramref name="point"/> lies in the view, its right and bottom edges excluded.</summary>
    internal static bool ContainsBeforeFarEdges(Position16<RoomPixels> point, Position16<RoomPixels> camera) =>
        CoordinateAxis.Wrap(point.X - camera.X) < Extent && CoordinateAxis.Wrap(point.Y - camera.Y) < Extent;

    /// <summary>Deletes <paramref name="projectile"/> once it lies outside the view, its right and bottom edges included in it.</summary>
    internal static void DeleteOutside(EnemyProjectile projectile, Position16<RoomPixels> camera)
    {
        if (!Contains(projectile.Position.Whole, camera))
            projectile.Delete();
    }
}
