namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// How far ahead of Samus the camera aims horizontally.
/// </summary>
public enum CameraDistanceMode
{
    /// <summary>Normal tracking, restored by the camera reset and boss completion.</summary>
    NormalTracking,
    /// <summary>Boss tracking, selected by Kraid and Crocomire.</summary>
    BossTracking,
    /// <summary>Samus 32 pixels from the right edge; Crocomire selects this target mode.</summary>
    RightEdge,
}
