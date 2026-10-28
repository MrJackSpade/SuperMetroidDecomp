using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Menu atlas9D/9E are transposed triangular arrow halves,9F is an
/// outlined defeated-boss diagonal/frame corner, andAF is a three-line pulse corner.
/// Native map compositions mirror these halves/corners (82:C232..C262,C298,C38C..C3A8,
/// CF7C..CFE0). X/Y are0..7. Arrow9E fills X&lt;=Y with11, using14 at X0 or X=Y;
/// 9D transposes X/Y. Pulse uses min(X,Y):2/4 black15,3 bright14, otherwise0.
/// Boss uses black outer X0/Y0, red10 at X1/Y1 and diagonals X=Y or Y-1,
/// then black inner X2/Y2 and diagonals X=Y+1 or Y-2; other pixels are0.
/// Calculate every original pixel directly; keep only supplied edits.</summary>
internal sealed class MapMarkerTileArtwork
{
    /// <summary>The two arrow halves, defeated-boss corner, and pulse corner are the four calculated marker tiles.</summary>
    internal const int TileCount = 4;
    /// <summary>Imported pixel differences from the calculated native marker shapes.</summary>
    internal readonly Dictionary<int, byte>? edits;
    /// <summary>Recognizes only the four menu-atlas tile identities owned by this artwork.</summary>
    internal static bool Contains(int tile) => MapMarkerCellDefinitions.CalculatedCells.Any(cell => MapMarkerCellDefinitions.Tiles[cell] == tile);

    /// <summary>Compares the supplied atlas against the arrow and corner geometry, retaining only mismatched pixels.</summary>
    internal MapMarkerTileArtwork(IndexedPngImage image)
    {
        foreach (int tile in MapMarkerCellDefinitions.CalculatedCells.Select(cell => MapMarkerCellDefinitions.Tiles[cell]))
        {
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int pixel = image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
                if (pixel != Basis(MapMarkerCellDefinitions.CellsByTile[tile], new(checked((ushort)x), checked((ushort)y)))) (edits ??= []).Add(tile * 64 + y * 8 + x, (byte)pixel);
            }
        }
    }

    /// <summary>Uses an imported edit when present; otherwise calculates the requested marker pixel.</summary>
    internal byte Pixel(int tile, Position16<CharacterPixels> point)
    {
        if (!Contains(tile) || (uint)point.X >= 8 || (uint)point.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.ContainsKey(tile * 64 + point.Y * 8 + point.X) ? edits[tile * 64 + point.Y * 8 + point.X] : Basis(MapMarkerCellDefinitions.CellsByTile[tile], point);
    }

    /// <summary>Draws the transposed arrows or the nested corner lines from tile-local coordinates.</summary>
    private static byte Basis(MapMarkerCell cell, Position16<CharacterPixels> point)
    {
        if (cell == MapMarkerCell.IndicatorCorner)
        {
            // Nested right angles: outer black, bright center, inner black.
            int inset = Math.Min(point.X, point.Y);
            return inset is 2 or 4 ? (byte)15 : inset == 3 ? (byte)14 : (byte)0;
        }
        if (cell == MapMarkerCell.DefeatedBossCorner)
        {
            if (point.X == 0 || point.Y == 0) return 15;
            if (point.X == 1 || point.Y == 1 || point.X == point.Y || point.X == point.Y - 1) return 10;
            return point.X == 2 || point.Y == 2 || point.X == point.Y + 1 || point.X == point.Y - 2 ? (byte)15 : (byte)0;
        }
        if (cell == MapMarkerCell.VerticalArrowHalf) point = new(point.Y, point.X);
        if (point.X > point.Y) return 0;
        return point.X == 0 || point.X == point.Y ? (byte)14 : (byte)11;
    }
}
