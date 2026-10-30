using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Hashes placed tiles into an installed content identity (#1467): only the import pipeline holds tilemaps.</summary>
internal static class SelectedPresentationHashTiles
{
    /// <summary>The tile framing a content identity hashes.</summary>
    extension(SelectedPresentationHash self)
    {
        /// <summary>Adds each placed tile's character, colour variant, mirrorings and priority tier, in order.</summary>
        public void AppendTiles(string label, ReadOnlySpan<BackgroundTile> tiles)
        {
            self.Append(label, tiles.Length);
            foreach (BackgroundTile tile in tiles)
            {
                self.Append("tile", tile.Tile);
                self.Append("colour", tile.ColourVariant);
                self.Append("horizontal", tile.FlipHorizontally ? 1 : 0);
                self.Append("vertical", tile.FlipVertically ? 1 : 0);
                self.Append("priority", tile.HighPriority ? 1 : 0);
            }
        }
    }
}
