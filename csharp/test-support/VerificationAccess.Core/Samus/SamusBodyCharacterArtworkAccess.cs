
namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Verification access to <see cref="SamusBodyCharacterArtwork"/> members production does not use.</summary>
internal static class SamusBodyCharacterArtworkAccess
{
    /// <summary>Exposes <see cref="SamusBodyCharacterArtwork"/> state that only verification reads.</summary>
    extension(SamusBodyCharacterArtwork self)
    {
        /// <summary>Gets a lower-body definition group by pointer-table index.</summary>
        public IReadOnlyList<SamusBodyTileTransfer> BottomSet(int set) => self.bottom[set];
    }
}
