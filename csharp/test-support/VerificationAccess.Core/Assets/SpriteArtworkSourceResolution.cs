using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Assets;

/// <summary>Resolves the installed characters a verification replay's source names (#1422).</summary>
internal static class SpriteArtworkSourceResolution
{
    /// <summary>The installed characters a source names.</summary>
    extension(VerificationArtworkSource self)
    {
        /// <summary>The installed characters the source names, as <paramref name="installed"/> holds them.</summary>
        internal ReadOnlyMemory<byte> Resolve(IInstalledArtworkContent installed) => self.ResolveCharacters(installed);
    }
}
