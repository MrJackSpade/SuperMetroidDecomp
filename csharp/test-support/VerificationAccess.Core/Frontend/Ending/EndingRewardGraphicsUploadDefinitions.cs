namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>How the cartridge transfers the post-credits icon artwork in sixteen equal chunks, which the VRAM oracle composes.</summary>
internal static class EndingRewardGraphicsUploadDefinitions
{
    /// <summary>The chunk transfers the cartridge makes of the icon artwork.</summary>
    public const int ChunkCount = 16;

    /// <summary>The bytes of icon artwork each of the sixteen chunk uploads shows.</summary>
    public const int ChunkBytes = 0x800;

    /// <summary>The bytes of icon artwork the sixteen chunks show together; the installed icon must cover them.</summary>
    public const int ArtworkBytes = ChunkBytes * ChunkCount;
}
