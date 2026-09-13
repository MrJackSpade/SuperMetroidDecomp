namespace SuperMetroid.Core.Rooms;

/// <summary>Native Shaktool-room persistence and scroll controller definitions.</summary>
internal static class ShaktoolRoomPlmRomData
{
    /// <summary>$84:B8EB PLMEntries_shaktoolsRoom, spawned at block (0,0) by $8F:C8D3.</summary>
    public const ushort Header = 0xb8eb;
    /// <summary>$84:B8D6 installs the callback, then sleeps.</summary>
    public const ushort InstructionList = 0xb8d6;
    /// <summary>$84:B8B0 PreInstruction_PLM_ShaktoolsRoom.</summary>
    public const ushort PreInstruction = 0xb8b0;
    /// <summary>$84:B8C3 compares $0348 with Samus X; carry clear marks event $0D.</summary>
    public const ushort ClearedPathXBoundary = 0x348;
    /// <summary>$84:B8B8/B8BF and B8DF/B8E6 write the first four scroll bytes.</summary>
    public const int ScrollCellCount = 4;
}
