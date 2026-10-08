namespace SuperMetroid.Core.Game;

/// <summary>
/// <c>CameraDistanceIndex</c> ($0941): the byte offset $90:95A0 uses to select Samus's
/// horizontal camera target.
/// </summary>
public enum CameraDistanceMode : ushort
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

/// <summary>Horizontal camera target offsets consumed by $90:95A0.</summary>
internal static class HorizontalCameraTargetDefinitions
{
    /// <summary>
    /// $90:963F/9647 select Samus's screen-relative target. Normal tracking places
    /// Samus32 pixels either side of viewport center; boss tracking uses its distinct
    /// facing policy. Edge modes ignore facing. The caller retains the native reversal rules.
    /// </summary>
    internal static ushort Offset(ushort nativeMode, bool facingRight) => (CameraDistanceMode)nativeMode switch
    {
        CameraDistanceMode.NormalTracking => (ushort)(256 / 2 + (facingRight ? -32 : 32)),
        CameraDistanceMode.BossTracking => facingRight ? (ushort)64 : (ushort)80,
        CameraDistanceMode.LeftEdge => 32,
        CameraDistanceMode.RightEdge => 256 - 32,
        _ => throw new IndexOutOfRangeException(),
    };
}
