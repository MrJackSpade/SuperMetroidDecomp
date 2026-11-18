namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Horizontal camera target offsets the horizontal camera follow uses.</summary>
internal static class HorizontalCameraTargets
{
    /// <summary>
    /// Select Samus's screen-relative target (<see cref="HorizontalCameraTargetDefinitions"/>) for the way the camera
    /// takes her to face (<paramref name="facing"/>). Normal tracking places Samus behind the viewport's centre on the side
    /// she faces away from; boss tracking uses its distinct facing policy. Edge modes ignore facing. The camera keeps the
    /// game's reversal rules.
    /// </summary>
    internal static int Offset(CameraDistanceMode mode, HorizontalDirection facing) => mode switch
    {
        CameraDistanceMode.NormalTracking => ScreenGeometry.ScreenWidthPixels / 2 + (facing == HorizontalDirection.Right
            ? -HorizontalCameraTargetDefinitions.NormalTrackingLead
            : HorizontalCameraTargetDefinitions.NormalTrackingLead),
        CameraDistanceMode.BossTracking => facing == HorizontalDirection.Right
            ? HorizontalCameraTargetDefinitions.BossTrackingFacingRightX
            : HorizontalCameraTargetDefinitions.BossTrackingFacingLeftX,
        CameraDistanceMode.RightEdge => ScreenGeometry.ScreenWidthPixels - HorizontalCameraTargetDefinitions.RightEdgeInset,
        _ => throw new InvalidOperationException($"Undefined {nameof(CameraDistanceMode)} {mode}."),
    };
}
