namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// Where an elevator ride stands. The status is global because it survives the room transition between the departing
/// and arriving elevators.
/// </summary>
public enum ElevatorActorStatus
{
    /// <summary>No ride: the elevator waits, while Samus stands on its pseudo-door, for the newly pressed direction that starts one.</summary>
    Inactive,
    /// <summary>Leaving the room: the departing platform moves and carries Samus at one-and-one-half pixels per update.</summary>
    Departing,
    /// <summary>Set by the door load: the arriving elevator starts at its arrival height, then begins its return.</summary>
    BeginArrivalReturn,
    /// <summary>Entering the room: the platform moves toward its saved resting height, then ends the ride and unlocks Samus's input.</summary>
    ReturningToRest,
}
