namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of the library-background command lists and their work-RAM staging that only verification reads.</summary>
internal static class LibraryBackgroundListConstants
{
    /// <summary>Bank containing the shared decompression and tilemap staging buffers.</summary>
    public const int WorkRamBank = 0x7e0000;
    /// <summary>Exclusive byte offset at the end of one SNES address bank.</summary>
    public const int BankByteCount = 0x10000;
    /// <summary>Maximum commands accepted before a malformed unterminated list fails.</summary>
    public const int MaximumCommandsPerList = 128;
}
