using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Assets;

/// <summary>Installed artwork a verification replay of native transfers copies: an escape timer page or a gunship takeoff page.</summary>
/// <param name="Artwork">The installed artwork.</param>
internal sealed record InstalledArtworkUploadSource(InstalledArtworkId Artwork) : VerificationArtworkSource
{
    /// <inheritdoc/>
    public override ReadOnlyMemory<byte> ResolveCharacters(IInstalledArtworkContent resolver) => Ensure.NotNull(resolver).Resolve(Artwork);

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"asset {Artwork}";
}
