namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Animation selection for the Tripper and Kamer platforms.</summary>
internal static class PlatformAnimationDefinitions
{
    /// <summary>
    /// Program choice at $A3:9E47-9EBA, called by the zero/nonzero direction branches at $A3:9ECB and $A3:9EF1.
    /// Initialization uses the still cases. Every nonzero direction word selects the right-moving program.
    /// </summary>
    internal static PlatformProgram SelectProgram(bool isKamer, Axis axis, PlatformHorizontalMovement direction)
    {
        bool moving = axis == Axis.Vertical;
        bool left = direction == PlatformHorizontalMovement.Left;
        if (isKamer)
        {
            return moving
                ? left ? PlatformProgram.KamerMovingLeft : PlatformProgram.KamerMovingRight
                : left ? PlatformProgram.KamerStillLeft : PlatformProgram.KamerStillRight;
        }
        return moving
            ? left ? PlatformProgram.TripperMovingLeft : PlatformProgram.TripperMovingRight
            : left ? PlatformProgram.TripperStillMovingLeft : PlatformProgram.TripperStillMovingRight;
    }
}
