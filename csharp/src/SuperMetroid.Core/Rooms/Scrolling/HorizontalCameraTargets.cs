namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Horizontal camera target offsets the horizontal camera follow uses.</summary>
internal static class HorizontalCameraTargets
{
    /// <summary>
    /// Select Samus's screen-relative target. Normal tracking places
    /// Samus 32 pixels either side of viewport center; boss tracking uses its distinct
    /// facing policy. Edge modes ignore facing. The caller retains the game's reversal rules.
    /// </summary>
    internal static int Offset(CameraDistanceMode mode, bool facingRight) => mode switch
    {
        CameraDistanceMode.NormalTracking => CoordinateAxis.Wrap(256 / 2 + (facingRight ? -32 : 32)),
        CameraDistanceMode.BossTracking => facingRight ? 64 : 80,
        CameraDistanceMode.RightEdge => 256 - 32,
        _ => throw new InvalidOperationException($"Undefined {nameof(CameraDistanceMode)} {mode}."),
    };
}
