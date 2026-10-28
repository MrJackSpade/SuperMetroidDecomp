namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Bank-$84 pre-instruction callbacks used by the resident gate coroutine.</summary>
internal enum DownwardGatePreInstruction
{
    /// <summary>No pre-instruction installed in the PLM slot yet.</summary>
    None = 0,

    /// <summary>$84:BB52: wake the gate once its trigger is set.</summary>
    WakeIfTriggered = 0xbb52,

    /// <summary>$84:BB6B: wake the gate once triggered or when Samus is inside its column.</summary>
    WakeIfTriggeredOrSamusBelow = 0xbb6b,

    /// <summary>The <c>RTS</c> at $84:BB6A, installed once the gate has woken.</summary>
    Inert = 0xbb6a,
}
