namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// Literal values of WRAM <c>elevator_status</c> consumed by the four-entry dispatcher at
/// <c>$A3:9540</c>. The value is global because it survives the room transition between the
/// departing and arriving elevator actors.
/// </summary>
public enum ElevatorActorStatus
{
    /// <summary>Status zero dispatches native <c>ElevatorAI_0_LeavingRoom</c> at <c>$A3:9548</c> when pseudo-door contact is present; waits for the required newly pressed direction.</summary>
    Inactive = 0,
    /// <summary>Status one dispatches native <c>ElevatorAI_1_LeavingRoom</c> at <c>$A3:9579</c>; moves the departing platform and pins Samus to it at one-and-one-half pixels per actor update.</summary>
    Departing = 1,
    /// <summary>Status two enters <c>$A3:95B9</c> after a door load; the arriving actor starts at population parameter 2's Y coordinate, then advances to the return state.</summary>
    BeginArrivalReturn = 2,
    /// <summary>Status three dispatches native <c>ElevatorAI_3_EnteringRoom</c> at <c>$A3:95BC</c>; moves toward the destination actor's saved resting Y, then clears travel globals and unlocks Samus input.</summary>
    ReturningToRest = 3,
}
