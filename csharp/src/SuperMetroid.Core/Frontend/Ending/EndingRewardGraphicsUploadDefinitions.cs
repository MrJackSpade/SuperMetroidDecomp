namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>WRAM source progression for the sixteen post-credits icon chunks.</summary>
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

    /// <summary>Rejects upload indexes outside the sixteen authored Func216 transfers.</summary>
    private static void ValidateIndex(int index)
    {
        if ((uint)index >= EndingRewardJumpDefinitions.IconChunkCount)
            throw new ArgumentOutOfRangeException(nameof(index));
    }
}
