using Engine.World;

namespace Game.Compatibility;

/// <summary>
/// The cartridge's view for its enemies: a square of <see cref="Extent"/> pixels each way from the camera's corner, so
/// reaching further below the screen than the screen shows. Some enemies count as on screen by it and some sound only
/// inside it.
/// </summary>
public static class EnemyView
{
    /// <summary>The view's width and height, in pixels.</summary>
    public const int Extent = 256;

    /// <summary>
    /// Whether the point lies in the view widened by <paramref name="margin"/> pixels on every side, its right and
    /// bottom edges included.
    /// </summary>
    public static bool Contains(Room room, int x, int y, int margin = 0)
    {
        Camera camera = room.Session.Camera;
        return x >= camera.Left - margin && x <= camera.Left + Extent + margin &&
            y >= camera.Top - margin && y <= camera.Top + Extent + margin;
    }
}
