namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Animation selection for the Tripper and Kamer platforms.</summary>
internal static class PlatformAnimationDefinitions
{
    /// <summary>Program choice at $A3:9E47-9EBA for each platform pose; initialization uses the still poses.</summary>
    internal static readonly Dictionary<PlatformPose, PlatformProgram> Programs = new()
    {
        [new(true, true, true)] = PlatformProgram.KamerMovingLeft,
        [new(true, true, false)] = PlatformProgram.KamerMovingRight,
        [new(true, false, true)] = PlatformProgram.KamerStillLeft,
        [new(true, false, false)] = PlatformProgram.KamerStillRight,
        [new(false, true, true)] = PlatformProgram.TripperMovingLeft,
        [new(false, true, false)] = PlatformProgram.TripperMovingRight,
        [new(false, false, true)] = PlatformProgram.TripperStillMovingLeft,
        [new(false, false, false)] = PlatformProgram.TripperStillMovingRight,
    };
}
