using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>Cartridge values and helpers of <see cref="EndingVramDestinations"/> that only verification reads.</summary>
internal static class EndingVramDestinationsConstants
{
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
