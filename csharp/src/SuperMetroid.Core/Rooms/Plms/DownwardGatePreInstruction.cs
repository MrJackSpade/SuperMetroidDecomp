namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Pre-instruction callbacks used by the resident gate coroutine. The values are arbitrary (#1452); verification's
/// native view names each member's routine word.
/// </summary>
internal enum DownwardGatePreInstruction
{
    /// <summary>No pre-instruction installed in the PLM slot yet.</summary>
    None = 0,

    /// <summary>Wake the gate once its trigger is set.</summary>
    WakeIfTriggered = 1,

    /// <summary>Wake the gate once triggered or when Samus is inside its column.</summary>
    WakeIfTriggeredOrSamusBelow = 2,

    /// <summary>The empty routine installed once the gate has woken.</summary>
    Inert = 3,
}
