namespace SuperMetroid.Core.Frontend;

internal static class EndingRewardGraphicsUploadDefinitions
{
    /// <summary>$8B:F6B8: Func216 source-address table, relative to WRAM bank $7F.</summary>
    public const int SourceTable = 0x8bf6b8;
    /// <summary>$8B:F6D8: Func216 destination VRAM word-address table.</summary>
    public const int DestinationTable = 0x8bf6d8;
    /// <summary>$7F:4000: decompressed post-credits icon staging buffer.</summary>
    public const int SourceBase = 0x4000;
    /// <summary>Func216 transfers $800 bytes on each of sixteen calls.</summary>
    public const int ChunkBytes = 0x800;
    /// <summary>Full $7F:4000–BFFF staging range consumed by Func216.</summary>
    public const int SourceBytes = 0x8000;

    /// <summary>
    /// $8B:F6B8..F6D7 stores $4000,$4800,...,$B800. The transfer source is a
    /// WRAM word address; the uploaded artwork array begins at $7F:4000.
    /// </summary>
    public static int SourceWord(int index)
    {
        ValidateIndex(index);
        return SourceBase + index * ChunkBytes;
    }

    /// <summary>
    /// $8B:F6D8..F6F7 stores $0000,$0400,...,$3C00. VRAM increments after
    /// its high port, so each $800-byte chunk spans $400 word addresses.
    /// </summary>
    public static int DestinationWord(int index)
    {
        ValidateIndex(index);
        return index * (ChunkBytes / sizeof(ushort));
    }

    private static void ValidateIndex(int index)
    {
        if ((uint)index >= EndingRewardJumpDefinitions.UploadCount)
            throw new ArgumentOutOfRangeException(nameof(index));
    }
}
