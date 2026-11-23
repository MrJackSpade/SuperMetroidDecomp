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
    /// <summary>Samus at the right edge's inset (<see cref="HorizontalCameraTargetDefinitions.RightEdgeInset"/>); Crocomire selects this target mode.</summary>
    RightEdge,
}
