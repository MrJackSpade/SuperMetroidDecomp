using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>How the post-credits icon artwork is shown in sixteen equal chunks.</summary>
internal static class EndingRewardGraphicsUploadDefinitions
{
    /// <summary>The bytes of icon artwork each of the sixteen chunk uploads shows.</summary>
    public const int ChunkBytes = 0x800;

    /// <summary>The bytes of icon artwork the sixteen chunks show together; the installed icon must cover them.</summary>
    public const int ArtworkBytes = ChunkBytes * EndingRewardJumpDefinitions.IconChunkCount;
}
