
namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Development-tool instance members of <see cref="SamusBodyCharacterArtwork"/>.</summary>
internal static class SamusBodyCharacterArtworkToolingExtensions
{
    /// <summary>Development-tool instance members of <see cref="SamusBodyCharacterArtwork"/>.</summary>
    extension(SamusBodyCharacterArtwork self)
    {
        /// <summary>Gets an upper-body definition group by pointer-table index.</summary>
        public IReadOnlyList<SamusBodyTileTransfer> TopSet(int set) => self.top[set];
    }
}
