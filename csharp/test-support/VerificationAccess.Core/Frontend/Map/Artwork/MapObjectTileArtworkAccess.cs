using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Map;
namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Verification access to <see cref="MapObjectTileArtwork"/> members production does not use.</summary>
internal static class MapObjectTileArtworkAccess
{
    /// <summary>Distinguishes sparse highlight and reserve edits from full other-character bytes.</summary>
    extension(MapObjectTileArtwork self)
    {
        /// <summary>Counts edited highlight pixels in the sparse override dictionary.</summary>
        internal int StoredHighlightPixelCount => self.highlightEdits?.Count ?? 0;

        /// <summary>Counts edited reserve pixels in the sparse override dictionary.</summary>
        internal int StoredReservePixelCount => self.reserveEdits?.Count ?? 0;

        /// <summary>Measures the separate non-highlight, non-reserve character payload.</summary>
        internal int StoredOtherByteCount => self.otherCharacters.Length;
        /// <summary>Encodes retained and calculated menu characters into the immediate VRAM transfer payload.</summary>
        internal void LoadTo(VramRegion destination) => destination.Load(self.Encode());
    }
}
