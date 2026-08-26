namespace SuperMetroid.Core.Game;

/// <summary>
/// Host-controlled camera for inspecting a decoded room through the 256x192 gameplay area.
/// </summary>
/// <remarks>
/// This is deliberately named <em>Debug</em> camera: it does not claim to implement Samus
/// tracking, scroll boundary PLMs, door alignment, or the bank-$80 scrolling routines yet.
/// It does enforce the same useful geometric invariant those routines ultimately produce:
/// a viewport that never samples beyond the room image.
/// </remarks>
public sealed class DebugRoomCamera
{
    public DebugRoomCamera(int roomWidth, int roomHeight, int viewportWidth, int viewportHeight)
    {
        if (roomWidth <= 0 || roomHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(roomWidth));
        if (viewportWidth <= 0 || viewportHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(viewportWidth));

        RoomWidth = roomWidth;
        RoomHeight = roomHeight;
        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
    }

    public int RoomWidth { get; }
    public int RoomHeight { get; }
    public int ViewportWidth { get; }
    public int ViewportHeight { get; }
    public int X { get; private set; }
    public int Y { get; private set; }

    /// <summary>Moves to an absolute room pixel, clamped to the last complete viewport.</summary>
    public void MoveTo(int x, int y)
    {
        X = Math.Clamp(x, 0, Math.Max(0, RoomWidth - ViewportWidth));
        Y = Math.Clamp(y, 0, Math.Max(0, RoomHeight - ViewportHeight));
    }

    /// <summary>Moves relative to the current pixel position with the same bounds.</summary>
    public void MoveBy(int deltaX, int deltaY) => MoveTo(X + deltaX, Y + deltaY);
}
