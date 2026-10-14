namespace SuperMetroid.Core.Game;

/// <summary>Cross-system effects published by elevator AI during the current enemy frame.</summary>
public enum ElevatorFrameEvent
{
    /// <summary>No elevator lifecycle change has been published during the current enemy update; this is the reset value at its beginning.</summary>
    None,
    /// <summary>The direction edge was accepted, departure sounds and projectile reset were issued, and Samus was pinned; runtime discards pending pose requests and restores the suit palette on this event.</summary>
    DepartureStarted,
    /// <summary>The arriving actor changed global status from two to three and began its first return movement; completion in the same update may replace this event.</summary>
    ArrivalReturnStarted,
    /// <summary>The arriving platform reached its saved resting Y, cleared elevator flags/status, queued its stopping sound, and released Samus's input lock.</summary>
    ArrivalCompleted,
}
