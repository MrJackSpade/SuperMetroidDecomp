using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="BgArtworkIdentity"/> that only verification reads.</summary>
internal static class BgArtworkIdentityConstants
{
    /// <summary>
    /// The installed artwork a queued or direct transfer from <paramref name="sourceAddress"/> shows: animated tiles' frames
    /// and other transfers named by where the cartridge keeps them.
    /// </summary>
    public static BgArtworkIdentity Transfer(int sourceAddress) => new($"artwork ${sourceAddress:X6}");
}
