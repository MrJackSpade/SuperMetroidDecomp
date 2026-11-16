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
        [new(true, Axis.Vertical, HorizontalDirection.Left)] = PlatformSegment.SuspensorPlatformHeadingLeft,
        [new(true, Axis.Vertical, HorizontalDirection.Right)] = PlatformSegment.SuspensorPlatformHeadingRight,
        [new(true, Axis.Horizontal, HorizontalDirection.Left)] = PlatformSegment.SuspensorPlatformHeadingLeft,
        [new(true, Axis.Horizontal, HorizontalDirection.Right)] = PlatformSegment.SuspensorPlatformHeadingRight,
        [new(false, Axis.Vertical, HorizontalDirection.Left)] = PlatformSegment.TripperMovingLeft,
        [new(false, Axis.Vertical, HorizontalDirection.Right)] = PlatformSegment.TripperMovingRight,
        [new(false, Axis.Horizontal, HorizontalDirection.Left)] = PlatformSegment.TripperStillMovingLeft,
        [new(false, Axis.Horizontal, HorizontalDirection.Right)] = PlatformSegment.TripperStillMovingRight,
    };
}
