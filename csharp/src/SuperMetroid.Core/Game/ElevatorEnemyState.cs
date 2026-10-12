namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal values of WRAM <c>elevator_status</c> consumed by the four-entry dispatcher at
/// <c>$A3:9540</c>. The value is global because it survives the room transition between the
/// departing and arriving elevator actors.
/// </summary>
public enum ElevatorActorStatus : ushort
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

/// <summary>
/// Typed projection of the one private word used by elevator enemy <c>$D73F</c>. Native
/// variable A retains the room-side resting Y coordinate while the global status survives
/// travel to the destination room.
/// </summary>
public sealed class ElevatorEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new elevator enemy state instance.</summary>
    internal ElevatorEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Enemy variable A, captured before an arriving actor is moved to parameter 2.</summary>
    public ushort RestingYPosition
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>
    /// Parameter 1 after the initializer doubles it. This is a byte offset into the two-word
    /// input-mask table at <c>$A3:94E2</c>, not an invented direction boolean.
    /// </summary>
    public ushort DirectionTableByteOffset => _slot.Parameter1;
}
