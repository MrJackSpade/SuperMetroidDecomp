namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// <c>CameraDistanceIndex</c> ($0941): the byte offset $90:95A0 uses to select Samus's
/// horizontal camera target.
/// </summary>
public enum CameraDistanceMode
{
    /// <summary>Normal tracking, restored by $88:8347 and boss completion routines.</summary>
    NormalTracking = 0,
    /// <summary>Boss tracking, selected by Kraid $A7:A9E4 and Crocomire $A4:8ABA.</summary>
    BossTracking = 2,
    /// <summary>Samus32 pixels from the left edge. No assignment found in pinned native callers.</summary>
    LeftEdge = 4,
    /// <summary>Samus32 pixels from the right edge; Crocomire $A4:97F3 selects this target mode.</summary>
    RightEdge = 6,
}
