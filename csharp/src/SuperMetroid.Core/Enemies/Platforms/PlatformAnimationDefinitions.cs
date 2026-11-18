namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Animation selection for the Tripper and suspensor platforms.</summary>
internal static class PlatformAnimationDefinitions
{
    /// <summary>
    /// Animation choice for each platform pose; initialization uses the still poses. The suspensor platform looks the same moving or
    /// still, so only its heading chooses its animation (#1842).
    /// </summary>
    internal static readonly Dictionary<PlatformPose, PlatformSegment> Animations = new()
    {
        [new(PlatformKind.SuspensorPlatform, Axis.Vertical, HorizontalDirection.Left)] = PlatformSegment.SuspensorPlatformHeadingLeft,
        [new(PlatformKind.SuspensorPlatform, Axis.Vertical, HorizontalDirection.Right)] = PlatformSegment.SuspensorPlatformHeadingRight,
        [new(PlatformKind.SuspensorPlatform, Axis.Horizontal, HorizontalDirection.Left)] = PlatformSegment.SuspensorPlatformHeadingLeft,
        [new(PlatformKind.SuspensorPlatform, Axis.Horizontal, HorizontalDirection.Right)] = PlatformSegment.SuspensorPlatformHeadingRight,
        [new(PlatformKind.Tripper, Axis.Vertical, HorizontalDirection.Left)] = PlatformSegment.TripperMovingLeft,
        [new(PlatformKind.Tripper, Axis.Vertical, HorizontalDirection.Right)] = PlatformSegment.TripperMovingRight,
        [new(PlatformKind.Tripper, Axis.Horizontal, HorizontalDirection.Left)] = PlatformSegment.TripperStillMovingLeft,
        [new(PlatformKind.Tripper, Axis.Horizontal, HorizontalDirection.Right)] = PlatformSegment.TripperStillMovingRight,
    };
}
