using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Artwork a verification replay of native character transfers names (#1422, #1513): the game names no sprite looks, so
/// only verification's own replays of the cartridge's transfers name the bytes each one copies, and each resolves its own.
/// </summary>
public abstract record VerificationArtworkSource
{
    /// <summary>The characters the source names, as <paramref name="resolver"/> holds them.</summary>
    /// <param name="resolver">The installed artwork.</param>
    public abstract ReadOnlyMemory<byte> ResolveCharacters(IInstalledArtworkContent resolver);
}
