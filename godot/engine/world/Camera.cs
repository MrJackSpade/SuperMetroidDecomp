using Engine.Shared;

namespace Engine.World;

/// <summary>
/// The view onto a room. It follows a point, keeps inside the room, and keeps out of camera zones that are closed. It
/// affects nothing in play. One per session.
/// </summary>
public sealed class Camera
{
    /// <summary>A camera whose view is <paramref name="width"/> by <paramref name="height"/> pixels.</summary>
    public Camera(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        Width = width;
        Height = height;
    }

    /// <summary>The view's width, in pixels.</summary>
    public int Width { get; }

    /// <summary>The view's height, in pixels.</summary>
    public int Height { get; }

    /// <summary>The view's left edge, in room pixels.</summary>
    public int Left { get; private set; }

    /// <summary>The view's top edge, in room pixels.</summary>
    public int Top { get; private set; }

    /// <summary>The part of the room in view.</summary>
    public PixelRect View => new(Left, Top, Width, Height);

    /// <summary>
    /// Centres the view on <paramref name="point"/> as nearly as the room's edges and its closed zones allow.
    /// </summary>
    public void Follow(PixelPosition point, int roomWidth, int roomHeight, IEnumerable<CameraZone> zones)
    {
        int left = Math.Clamp(point.X.Whole - Width / 2, 0, Math.Max(0, roomWidth - Width));
        int top = Math.Clamp(point.Y.Whole - Height / 2, 0, Math.Max(0, roomHeight - Height));

        foreach (CameraZone zone in zones)
        {
            if (zone.IsOpen)
                continue;
            PixelRect closed = zone.Area;
            var view = new PixelRect(left, top, Width, Height);
            if (!view.Overlaps(closed))
                continue;

            // Leave the closed zone by the shorter way: sideways or up and down.
            int pushLeft = view.Right - closed.Left, pushRight = closed.Right - view.Left;
            int pushUp = view.Bottom - closed.Top, pushDown = closed.Bottom - view.Top;
            int across = pushLeft <= pushRight ? -pushLeft : pushRight;
            int down = pushUp <= pushDown ? -pushUp : pushDown;
            if (Math.Abs(across) <= Math.Abs(down))
                left += across;
            else
                top += down;
        }

        Left = left;
        Top = top;
    }
}
