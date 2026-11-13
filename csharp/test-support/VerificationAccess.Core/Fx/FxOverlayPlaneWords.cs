using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Reads the BG3 plane as the tile words the cartridge would write for it (#1466): the cleared cell everywhere, with the
/// effect's installed page from its first row, so verification can compare the plane with the words the cartridge uploads.
/// The plane itself names only which page shows; the words are the import's knowledge.
/// </summary>
internal static class FxOverlayPlaneWords
{
    /// <summary>The tile words of the plane.</summary>
    extension(FxOverlayPlane plane)
    {
        /// <summary>Each cell's tile, row-major, its effect's page read from <paramref name="pages"/>.</summary>
        internal BackgroundTile[] TilesIn(RoomFxLayer3Pages pages)
        {
            var tiles = new BackgroundTile[FxOverlayPlaneConstants.Columns * FxOverlayPlaneConstants.Rows];
            tiles.AsSpan().Fill(Layer3ClearedCellDefinitions.ClearedCell);
            if (plane.Effect is { } type)
                pages.Resolve(type).Span.CopyTo(tiles.AsSpan(FxOverlayPlaneConstants.EffectPageFirstRow * FxOverlayPlaneConstants.Columns));
            return tiles;
        }

        /// <summary>Each cell's packed tilemap word, row-major, its effect's page read from <paramref name="pages"/>.</summary>
        internal ushort[] WordsIn(RoomFxLayer3Pages pages) => BackgroundTileWordBytes.Words(plane.TilesIn(pages));
    }
}
