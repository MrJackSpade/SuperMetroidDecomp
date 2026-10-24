using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The VRAM word each ending upload lands at (#1406): the cartridge's layout, which only extraction's replay of the
/// ending screen needs.
/// </summary>
internal static class EndingVramDestinations
{
    /// <summary>The subtitle's VRAM word, from the first record at <c>$8B:E45A</c>.</summary>
    private const ushort SubtitleWord = 0x4800;

    /// <summary>The first logo chunk's VRAM word, from the second record; later chunks follow it.</summary>
    private const ushort FirstLogoTileWord = 0x6000;

    /// <summary>The logo map's VRAM word, from the sixth record.</summary>
    private const ushort LogoMapWord = 0x5400;

    /// <summary>The VRAM word post-shot part <paramref name="transfer"/> lands at.</summary>
    /// <exception cref="InvalidOperationException">The part names no defined source.</exception>
    internal static ushort PostShotWordOf(EndingPostShotUploadDefinition transfer) => transfer.Source switch
    {
        EndingPostShotUploadSource.Subtitle => SubtitleWord,
        EndingPostShotUploadSource.LogoTiles => checked((ushort)(FirstLogoTileWord +
            (transfer.SourceAddress - EndingPostShotDefinitions.LogoTileSource) / sizeof(ushort))),
        EndingPostShotUploadSource.LogoMap => LogoMapWord,
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingPostShotUploadSource)} {(int)transfer.Source}."),
    };

    /// <summary>
    /// The VRAM word reward icon chunk <paramref name="chunk"/> lands at: <c>$8B:F6D8..F6F7</c> stores
    /// $0000,$0400,...,$3C00, each $800-byte chunk spanning $400 words.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="chunk"/> is not one of the sixteen chunks.</exception>
    internal static ushort RewardChunkWordOf(int chunk)
    {
        if ((uint)chunk >= EndingRewardJumpDefinitions.IconChunkCount)
            throw new ArgumentOutOfRangeException(nameof(chunk));
        return checked((ushort)(chunk * (EndingRewardGraphicsUploadDefinitions.ChunkBytes / sizeof(ushort))));
    }
}
