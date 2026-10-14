namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The save-pod coroutine's handshake between resident PLM animation and frontend messages.</summary>
public enum SaveStationPhase : byte
{
    /// <summary>The initial pod draw sleeps until an eligible floor collision; a room-entry lockout can still prevent another request.</summary>
    Idle,
    /// <summary>A floor trigger has published the YES/NO request and waits for the frontend's confirmation result.</summary>
    AwaitingConfirmation,
    /// <summary>Accepted confirmation has centered and locked Samus; alternating four-tick pod frames run for the authored loop count.</summary>
    Animating,
    /// <summary>Animation has published save-completed message $18 and waits for its close callback to unlock input and install the room-entry lockout.</summary>
    AwaitingCompletionMessageClose,
}
