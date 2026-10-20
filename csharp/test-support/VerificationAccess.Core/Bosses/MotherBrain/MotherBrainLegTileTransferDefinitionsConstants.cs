namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values of <see cref="MotherBrainLegTileTransferDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainLegTileTransferDefinitionsConstants
{
    /// <summary>The full $A9:8F8F address used only by cartridge parity checks.</summary>
    public const int NativeListAddress = 0xa98f8f;

    /// <summary>Provides the native list address for Mother Brain leg tile transfer verification.</summary>
    extension(MotherBrainLegTileTransferDefinitions)
    {
        /// <inheritdoc cref="MotherBrainLegTileTransferDefinitionsConstants.NativeListAddress"/>
        internal static int NativeListAddress => MotherBrainLegTileTransferDefinitionsConstants.NativeListAddress;
    }
    /// <summary>$A9:8F8F, first seven-byte record in the twelve-page list.</summary>
    public const ushort NativeListPointer = 0x8f8f;
    /// <summary>Seven bytes per native size/source/destination record.</summary>
    public const ushort RecordByteCount = 7;
    /// <summary>Maps a serialized native record pointer to its page index.</summary>
    public static int IndexOf(ushort pointer)
    {
        int offset = pointer - NativeListPointer;
        if (offset < 0 || offset % RecordByteCount != 0 ||
            offset / RecordByteCount >= MotherBrainLegTileTransferDefinitions.PageCount)
            throw new InvalidDataException(
                $"Mother Brain leg transfer pointer ${pointer:X4} is outside the twelve-record list.");
        return offset / RecordByteCount;
    }
}
