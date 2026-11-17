namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Where a save station stands in a save: waiting for Samus, awaiting the player's answer, playing its animation, and
/// showing the saved message.
/// </summary>
public enum SaveStationPhase
{
    /// <summary>The initial pod draw rests until an eligible floor collision; a room-entry lockout can still prevent another request.</summary>
    Idle,
    /// <summary>A floor trigger has published the YES/NO request and waits for the frontend's confirmation result.</summary>
    AwaitingConfirmation,
    /// <summary>Accepted confirmation has centered and locked Samus; alternating four-tick pod frames run for the authored loop count.</summary>
    Animating,
    /// <summary>Animation has published the save-completed message and waits for its closing cue to unlock input and install the room-entry lockout.</summary>
    AwaitingCompletionMessageClose,
}
