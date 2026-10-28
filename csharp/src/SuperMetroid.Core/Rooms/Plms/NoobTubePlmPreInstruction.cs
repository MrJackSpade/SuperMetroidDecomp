namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Pre-instructions the n00b-tube PLM installs. The values are arbitrary (#1452); verification's native view names each
/// member's routine word.
/// </summary>
public enum NoobTubePlmPreInstruction : ushort
{
    /// <summary>Wake the linked list on a newly pressed face button or horizontal direction.</summary>
    WakeOnAcceptedInput = 1,

    /// <summary>Select the linked list only after a power-bomb hit.</summary>
    WakeOnPowerBomb = 2,

    /// <summary>The inert return installed by ClearPreInstruction.</summary>
    Inactive = 3,
}
