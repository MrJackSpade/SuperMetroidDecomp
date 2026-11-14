namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// Values of the elevator status consumed by the four-entry elevator
/// phases. The value is global because it survives the room transition between the
/// departing and arriving elevator actors.
/// </summary>
public enum ElevatorActorStatus
{
    /// <summary>Status zero, leaving the room, applies when pseudo-door contact is present; waits for the required newly pressed direction.</summary>
    Inactive = 0,
    /// <summary>Status one, leaving the room, moves the departing platform and pins Samus to it at one-and-one-half pixels per actor update.</summary>
    Departing = 1,
    /// <summary>Status two is entered after a door load; the arriving actor starts at its arrival height, then advances to the return state.</summary>
    BeginArrivalReturn = 2,
    /// <summary>Status three, entering the room, moves toward the destination actor's saved resting Y, then clears travel globals and unlocks Samus input.</summary>
    ReturningToRest = 3,
}
