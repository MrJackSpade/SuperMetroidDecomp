namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>The steps of the elevator's ride its AI publishes in an enemy pass (<see cref="ElevatorEvent"/>).</summary>
public enum ElevatorFrameEvent
{
    /// <summary>
    /// The direction edge was accepted, departure sounds were issued, her beam and missile shots were cleared and Samus was
    /// pinned; the runtime clears her bombs and the shared cooldown, discards pending pose requests and restores the suit
    /// colours on this event.
    /// </summary>
    DepartureStarted,
    /// <summary>The arriving elevator moved from <see cref="ElevatorActorStatus.BeginArrivalReturn"/> to <see cref="ElevatorActorStatus.ReturningToRest"/> and began its first return movement; completion may follow in the same update.</summary>
    ArrivalReturnStarted,
    /// <summary>The arriving platform reached its saved resting Y, cleared elevator flags/status, queued its stopping sound, and released Samus's input lock.</summary>
    ArrivalCompleted,
}
