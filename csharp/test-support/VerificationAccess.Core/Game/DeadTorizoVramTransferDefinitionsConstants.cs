using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="DeadTorizoVramTransferDefinitions"/> that only verification reads.</summary>
internal static class DeadTorizoVramTransferDefinitionsConstants
{
    /// <summary><c>DeadTorizo_VRAMWriteTable_Even</c> at $A9:D549.</summary>
    public const ushort EvenTable = 0xd549;
    /// <summary><c>DeadTorizo_VRAMWriteTable_Odd</c> at $A9:D583.</summary>
    public const ushort OddTable = 0xd583;
    /// <summary>Four 16-bit descriptor words per native record.</summary>
    public const int RecordByteCount = 8;

    /// <summary>Provides the two dead Torizo VRAM tables and record width used to check transfer decoding.</summary>
    extension(DeadTorizoVramTransferDefinitions)
    {
        /// <inheritdoc cref="DeadTorizoVramTransferDefinitionsConstants.EvenTable"/>
        internal static ushort EvenTable => DeadTorizoVramTransferDefinitionsConstants.EvenTable;
        /// <inheritdoc cref="DeadTorizoVramTransferDefinitionsConstants.OddTable"/>
        internal static ushort OddTable => DeadTorizoVramTransferDefinitionsConstants.OddTable;
        /// <inheritdoc cref="DeadTorizoVramTransferDefinitionsConstants.RecordByteCount"/>
        internal static int RecordByteCount => DeadTorizoVramTransferDefinitionsConstants.RecordByteCount;
    }
}
