using SuperMetroid.Core.Frontend.Ending;
using static SuperMetroid.AssetExtraction.Frontend.Ending.EndingRewardGraphicsUploadDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Locates each chunk of the ending reward graphics transfer (<see cref="EndingRewardGraphicsUploadDefinitions"/>).
/// </summary>
internal static class EndingRewardGraphicsUploads
{
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
