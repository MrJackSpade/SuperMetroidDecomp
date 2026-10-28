using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Resolves the installed characters a sprite artwork source names (#1422), as the import pipeline does, so verification
/// can compare a shown source's characters with the cartridge's; verification's own sources resolve themselves.
/// </summary>
internal static class SpriteArtworkSourceResolution
{
    /// <summary>The installed characters a source names.</summary>
    extension(SpriteArtworkSource self)
    {
        /// <summary>The installed characters the source names, as <paramref name="installed"/> holds them.</summary>
        internal ReadOnlyMemory<byte> Resolve(IInstalledArtworkContent installed) => self is IVerificationArtworkSource own
            ? own.ResolveCharacters(installed)
            : SpriteArtworkBytes.Of(self, installed);
    }
}
