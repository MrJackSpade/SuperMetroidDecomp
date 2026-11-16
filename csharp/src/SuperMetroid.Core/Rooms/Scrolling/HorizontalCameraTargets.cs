namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Horizontal camera target offsets the horizontal camera follow uses.</summary>
internal static class HorizontalCameraTargets
{
    /// <summary>
    /// Select Samus's screen-relative target (<see cref="HorizontalCameraTargetDefinitions"/>). Normal tracking places
    /// Samus behind the viewport's centre on the side she faces away from; boss tracking uses its distinct facing policy.
    /// Edge modes ignore facing. The camera keeps the game's reversal rules.
    /// </summary>
    internal static int Offset(CameraDistanceMode mode, bool facingRight) => mode switch
    {
        CameraDistanceMode.NormalTracking => ScreenGeometry.ScreenWidthPixels / 2 + (facingRight
            ? -HorizontalCameraTargetDefinitions.NormalTrackingLead
            : HorizontalCameraTargetDefinitions.NormalTrackingLead),
        CameraDistanceMode.BossTracking => facingRight
            ? HorizontalCameraTargetDefinitions.BossTrackingFacingRightX
            : HorizontalCameraTargetDefinitions.BossTrackingFacingLeftX,
        CameraDistanceMode.RightEdge => ScreenGeometry.ScreenWidthPixels - HorizontalCameraTargetDefinitions.RightEdgeInset,
        _ => throw new InvalidOperationException($"Undefined {nameof(CameraDistanceMode)} {mode}."),
    };
}
