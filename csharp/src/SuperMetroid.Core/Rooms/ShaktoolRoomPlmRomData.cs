namespace SuperMetroid.Core.Rooms;

/// <summary>Native Shaktool-room persistence and scroll controller definitions.</summary>
internal static class ShaktoolRoomPlmRomData
{
    /// <summary>$84:B8EB PLMEntries_shaktoolsRoom, spawned at block (0,0) by $8F:C8D3.</summary>
    public const ushort Header = 0xb8eb;
    /// <summary>$84:B8D6 installs the callback, then sleeps.</summary>
    public const ushort InstructionList = 0xb8d6;

    /// <summary>
    /// <c>$84:B8D6-B8DB InstList_PLM_ShaktoolsRoom</c> is the complete
    /// three-word resident controller. Its scroll/event rules live in the
    /// setup-selected pre-instruction, not in editable room artwork.
    /// </summary>
    internal static bool TryReadInstructionWord(ushort address, out ushort value)
    {
        value = address switch
        {
            InstructionList => RoomPlmInstructionCodes.InstallPreInstruction,
            InstructionList + 2 => PreInstruction,
            InstructionList + 4 => RoomPlmInstructionCodes.Sleep,
            _ => 0,
        };
        return address is InstructionList or InstructionList + 2 or InstructionList + 4;
    }
    /// <summary>$84:B8B0 PreInstruction_PLM_ShaktoolsRoom.</summary>
    public const ushort PreInstruction = 0xb8b0;
    /// <summary>$84:B8C3 compares $0348 with Samus X; carry clear marks event $0D.</summary>
    public const ushort ClearedPathXBoundary = 0x348;
    /// <summary>$84:B8B8/B8BF and B8DF/B8E6 write the first four scroll bytes.</summary>
    public const int ScrollCellCount = 4;
}
