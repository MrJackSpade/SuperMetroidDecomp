namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Bank-$84 pre-instructions the n00b-tube PLM installs.</summary>
public enum NoobTubePlmPreInstruction
{
    /// <summary><c>$84:D4BF</c>: wake the linked list on a newly pressed face button or horizontal direction.</summary>
    WakeOnAcceptedInput = 0xd4bf,

    /// <summary><c>$84:BD26</c>: select the linked list only after a power-bomb hit.</summary>
    WakeOnPowerBomb = 0xbd26,

    /// <summary><c>$84:86D0</c>: inert return installed by ClearPreInstruction.</summary>
    Inactive = 0x86d0,
}
