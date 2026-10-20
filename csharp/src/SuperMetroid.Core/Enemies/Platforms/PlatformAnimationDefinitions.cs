namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Animation selection for the Tripper and Kamer platforms.</summary>
internal static class PlatformAnimationDefinitions
{
    /// <summary>
    /// Program choice at $A3:9E47-9EBA, called by the zero/nonzero direction branches at $A3:9ECB and $A3:9EF1.
    /// Initialization uses the still cases. Every nonzero direction word selects the right-moving program.
    /// </summary>
    internal static PlatformProgram SelectProgram(bool isKamer, Axis axis, PlatformHorizontalMovement direction) =>
        (isKamer, axis == Axis.Vertical, direction == PlatformHorizontalMovement.Left) switch
        {
            (true, true, true) => PlatformProgram.KamerMovingLeft,
            (true, true, false) => PlatformProgram.KamerMovingRight,
            (true, false, true) => PlatformProgram.KamerStillLeft,
            (true, false, false) => PlatformProgram.KamerStillRight,
            (false, true, true) => PlatformProgram.TripperMovingLeft,
            (false, true, false) => PlatformProgram.TripperMovingRight,
            (false, false, true) => PlatformProgram.TripperStillMovingLeft,
            (false, false, false) => PlatformProgram.TripperStillMovingRight,
        };
}
