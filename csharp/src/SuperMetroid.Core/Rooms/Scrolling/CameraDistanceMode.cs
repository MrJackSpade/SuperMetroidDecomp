namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// How far ahead of Samus the camera aims horizontally.
/// </summary>
public enum CameraDistanceMode
{
    /// <summary>Normal tracking, restored by the camera reset and boss completion routines.</summary>
    NormalTracking = 0,
    /// <summary>Boss tracking, selected by Kraid and Crocomire.</summary>
    BossTracking = 2,
    /// <summary>Samus 32 pixels from the left edge. Nothing in the game selects it.</summary>
    LeftEdge = 4,
    /// <summary>Samus 32 pixels from the right edge; Crocomire selects this target mode.</summary>
    RightEdge = 6,
}
