namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Animation selection for the Tripper and Kamer platforms.</summary>
internal static class PlatformAnimationDefinitions
{
    /// <summary>Animation choice for each platform pose; initialization uses the still poses.</summary>
    internal static readonly Dictionary<PlatformPose, PlatformSegment> Animations = new()
    {
        [new(true, Axis.Vertical, PlatformHorizontalMovement.Left)] = PlatformSegment.KamerMovingLeft,
        [new(true, Axis.Vertical, PlatformHorizontalMovement.Right)] = PlatformSegment.KamerMovingRight,
        [new(true, Axis.Horizontal, PlatformHorizontalMovement.Left)] = PlatformSegment.KamerStillLeft,
        [new(true, Axis.Horizontal, PlatformHorizontalMovement.Right)] = PlatformSegment.KamerStillRight,
        [new(false, Axis.Vertical, PlatformHorizontalMovement.Left)] = PlatformSegment.TripperMovingLeft,
        [new(false, Axis.Vertical, PlatformHorizontalMovement.Right)] = PlatformSegment.TripperMovingRight,
        [new(false, Axis.Horizontal, PlatformHorizontalMovement.Left)] = PlatformSegment.TripperStillMovingLeft,
        [new(false, Axis.Horizontal, PlatformHorizontalMovement.Right)] = PlatformSegment.TripperStillMovingRight,
    };
}
