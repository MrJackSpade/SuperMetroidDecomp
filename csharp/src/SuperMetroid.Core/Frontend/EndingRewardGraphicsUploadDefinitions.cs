namespace SuperMetroid.Core.Frontend;

/// <summary>Defines the sixteen WRAM-to-VRAM chunk mappings used to upload the post-credits reward icons.</summary>
internal static class EndingRewardGraphicsUploadDefinitions
{
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

    /// <summary>Rejects chunk indices outside the native upload table.</summary>
    /// <param name="index">Zero-based upload chunk index.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is negative or reaches the upload count.</exception>
    private static void ValidateIndex(int index)
    {
        if ((uint)index >= EndingRewardJumpDefinitions.UploadCount)
            throw new ArgumentOutOfRangeException(nameof(index));
    }
}
