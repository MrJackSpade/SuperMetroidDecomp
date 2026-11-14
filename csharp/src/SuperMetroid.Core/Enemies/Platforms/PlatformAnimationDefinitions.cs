namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Animation selection for the Tripper and Kamer platforms.</summary>
internal static class PlatformAnimationDefinitions
{
    /// <summary>
    /// Animation choice for each platform pose; initialization uses the still poses. The Kamer looks the same moving or
    /// still, so only its heading chooses its animation (#1842).
    /// </summary>
    internal static readonly Dictionary<PlatformPose, PlatformSegment> Animations = new()
    {
        [new(true, Axis.Vertical, PlatformHorizontalMovement.Left)] = PlatformSegment.KamerHeadingLeft,
        [new(true, Axis.Vertical, PlatformHorizontalMovement.Right)] = PlatformSegment.KamerHeadingRight,
        [new(true, Axis.Horizontal, PlatformHorizontalMovement.Left)] = PlatformSegment.KamerHeadingLeft,
        [new(true, Axis.Horizontal, PlatformHorizontalMovement.Right)] = PlatformSegment.KamerHeadingRight,
        [new(false, Axis.Vertical, PlatformHorizontalMovement.Left)] = PlatformSegment.TripperMovingLeft,
        [new(false, Axis.Vertical, PlatformHorizontalMovement.Right)] = PlatformSegment.TripperMovingRight,
        [new(false, Axis.Horizontal, PlatformHorizontalMovement.Left)] = PlatformSegment.TripperStillMovingLeft,
        [new(false, Axis.Horizontal, PlatformHorizontalMovement.Right)] = PlatformSegment.TripperStillMovingRight,
    };
}
