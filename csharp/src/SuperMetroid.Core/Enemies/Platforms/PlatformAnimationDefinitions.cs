namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Animation selection for the Tripper and Kamer platforms.</summary>
internal static class PlatformAnimationDefinitions
{
    /// <summary>Program choice at $A3:9E47-9EBA for each platform pose; initialization uses the still poses.</summary>
    internal static readonly Dictionary<PlatformPose, PlatformProgram> Programs = new()
    {
        [new(true, Axis.Vertical, PlatformHorizontalMovement.Left)] = PlatformProgram.KamerMovingLeft,
        [new(true, Axis.Vertical, PlatformHorizontalMovement.Right)] = PlatformProgram.KamerMovingRight,
        [new(true, Axis.Horizontal, PlatformHorizontalMovement.Left)] = PlatformProgram.KamerStillLeft,
        [new(true, Axis.Horizontal, PlatformHorizontalMovement.Right)] = PlatformProgram.KamerStillRight,
        [new(false, Axis.Vertical, PlatformHorizontalMovement.Left)] = PlatformProgram.TripperMovingLeft,
        [new(false, Axis.Vertical, PlatformHorizontalMovement.Right)] = PlatformProgram.TripperMovingRight,
        [new(false, Axis.Horizontal, PlatformHorizontalMovement.Left)] = PlatformProgram.TripperStillMovingLeft,
        [new(false, Axis.Horizontal, PlatformHorizontalMovement.Right)] = PlatformProgram.TripperStillMovingRight,
    };
}
