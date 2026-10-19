namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Named bank-$84 PLM instruction-list offsets used by translated room actors.</summary>
/// <remarks>
/// Values are native low-word pointers in fixed bank $84. List identities remain separate
/// from executable opcode identities in <c>RoomPlmInstruction</c> and from
/// draw-list pointers, even though all three domains share the same 16-bit storage type.
/// </remarks>
public static class RoomPlmInstructionLists
{

    /// <summary>Wrecked Ship west-entry treadmill list at $84:AD38.</summary>
    public const ushort WreckedShipEntranceTreadmillFromWest = 0xad38;

    /// <summary>Wrecked Ship east-entry treadmill list at $84:AD4D.</summary>
    public const ushort WreckedShipEntranceTreadmillFromEast = 0xad4d;

    /// <summary>Idle save-pod draw entry at $84:AFE8, before its trigger sleeps.</summary>
    public const ushort SaveStationIdleDraw = 0xafe8;

    /// <summary>First save-pod animation frame entry at $84:AFFA.</summary>
    public const ushort SaveStationAnimationFirstFrame = 0xaffa;

    /// <summary>Second save-pod animation frame entry at $84:AFFE.</summary>
    public const ushort SaveStationAnimationSecondFrame = 0xaffe;
}
