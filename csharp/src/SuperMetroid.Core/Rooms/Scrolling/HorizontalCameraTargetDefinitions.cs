namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Horizontal camera target offsets consumed by $90:95A0.</summary>
internal static class HorizontalCameraTargetDefinitions
{
    /// <summary>
    /// $90:963F/9647 select Samus's screen-relative target. Normal tracking places
    /// Samus32 pixels either side of viewport center; boss tracking uses its distinct
    /// facing policy. Edge modes ignore facing. The caller retains the native reversal rules.
    /// </summary>
    internal static ushort Offset(CameraDistanceMode mode, bool facingRight) => mode switch
    {
        CameraDistanceMode.NormalTracking => (ushort)(256 / 2 + (facingRight ? -32 : 32)),
        CameraDistanceMode.BossTracking => facingRight ? (ushort)64 : (ushort)80,
        CameraDistanceMode.LeftEdge => 32,
        CameraDistanceMode.RightEdge => 256 - 32,
        _ => throw new InvalidOperationException($"Undefined {nameof(CameraDistanceMode)} {mode}."),
    };
}
