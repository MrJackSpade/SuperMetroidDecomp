namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>How the post-credits icon artwork is shown in sixteen equal chunks.</summary>
internal static class EndingRewardGraphicsUploadDefinitions
{
    /// <summary>The bytes of icon artwork each of the sixteen chunk uploads shows.</summary>
    public const int ChunkBytes = 0x800;

    /// <summary>The bytes of icon artwork the sixteen chunks show together; the installed icon must cover them.</summary>
    public const int ArtworkBytes = ChunkBytes * EndingRewardJumpDefinitions.IconChunkCount;

    /// <summary>
    /// The bytes into the icon artwork chunk <paramref name="index"/> starts at: the chunks are consecutive
    /// (#1416: the cartridge's work-RAM source words, $4000 to $B800, are verification's to check).
    /// </summary>
    public static int ChunkOffset(int index)
    {
        ValidateIndex(index);
        return index * ChunkBytes;
    }

    /// <summary>Rejects upload indexes outside the sixteen chunk transfers.</summary>
    private static void ValidateIndex(int index)
    {
        if ((uint)index >= EndingRewardJumpDefinitions.IconChunkCount)
            throw new ArgumentOutOfRangeException(nameof(index));
    }
}
