using SuperMetroid.AssetExtraction.Frontend.Map;
namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Verification access to <see cref="MapMarkerTileArtwork"/> members production does not use.</summary>
internal static class MapMarkerTileArtworkAccess
{
    /// <summary>Inspects sparse edits to map-marker tile pixels.</summary>
    extension(MapMarkerTileArtwork self)
    {
        /// <summary>Counts only explicitly edited marker bytes.</summary>
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}
