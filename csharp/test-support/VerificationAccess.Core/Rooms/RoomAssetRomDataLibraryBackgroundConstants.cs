namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="RoomAssetRomData.LibraryBackground"/> that only verification reads.</summary>
internal static class RoomAssetRomDataLibraryBackgroundConstants
{
    /// <summary>Bank containing room-authored library-background command lists.</summary>
    public const int CommandBank = 0x8f0000;
    /// <summary>Bank containing the shared decompression and tilemap staging buffers.</summary>
    public const int WorkRamBank = 0x7e0000;
    /// <summary>Exclusive byte offset at the end of one SNES address bank.</summary>
    public const int BankByteCount = 0x10000;

    /// <summary>Provides the library background command bank, WRAM bank, and bank byte count for room asset decoding.</summary>
    extension(RoomAssetRomData.LibraryBackground)
    {
        /// <inheritdoc cref="RoomAssetRomDataLibraryBackgroundConstants.CommandBank"/>
        internal static int CommandBank => RoomAssetRomDataLibraryBackgroundConstants.CommandBank;
        /// <inheritdoc cref="RoomAssetRomDataLibraryBackgroundConstants.WorkRamBank"/>
        internal static int WorkRamBank => RoomAssetRomDataLibraryBackgroundConstants.WorkRamBank;
        /// <inheritdoc cref="RoomAssetRomDataLibraryBackgroundConstants.BankByteCount"/>
        internal static int BankByteCount => RoomAssetRomDataLibraryBackgroundConstants.BankByteCount;
    }
}
