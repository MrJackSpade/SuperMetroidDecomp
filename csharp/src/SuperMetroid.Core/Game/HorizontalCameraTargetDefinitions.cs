namespace SuperMetroid.Core.Game;

/// <summary>Horizontal camera target modes consumed by $90:95A0.</summary>
internal static class HorizontalCameraTargetDefinitions
{
    private enum Mode : ushort
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

    /// <summary>
    /// $90:963F/9647 select Samus's screen-relative target. Normal tracking places
    /// Samus32 pixels either side of viewport center; boss tracking uses its distinct
    /// facing policy. Edge modes ignore facing. The caller retains the native reversal rules.
    /// </summary>
    internal static ushort Offset(ushort nativeMode, bool facingRight) => (Mode)nativeMode switch
    {
        Mode.NormalTracking => (ushort)(256 / 2 + (facingRight ? -32 : 32)),
        Mode.BossTracking => facingRight ? (ushort)64 : (ushort)80,
        Mode.LeftEdge => 32,
        Mode.RightEdge => 256 - 32,
        _ => throw new IndexOutOfRangeException(),
    };
}
