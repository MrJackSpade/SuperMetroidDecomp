using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>Reviewed large menu glyph halves and transparent tile0F. Calculate index13 shadows one pixel down/right
/// from authored index14 silhouettes, including the shared upper/lower boundary established
/// by82:CBFB. Join touching upper/left ink corners and continue an incoming diagonal
/// shadow into a left/right/lower-ink notch with both upper diagonals open. These rules
/// calculate seven former M/Q/X shading additions without glyph-specific corrections.
/// The chosen letter silhouettes and three particular T/W/N shading choices remain art:
/// T extends its shadow left of its stem; W interrupts a shadow between identical ink
/// rows; N trims its bottom shadow where M's identical lower contour does not. These
/// unequal treatments of matching ink are drawing content, not a uniform lighting rule.
/// Replacing their coordinates with cases would merely recite the artwork.</summary>
internal sealed class MenuLargeFontArtwork
{
    /// <summary>Reviewed native tile span includes the transparent $0F cell and selected large-letter halves.</summary>
    internal const int TileCount = 43;
    /// <summary>Index-14 ink silhouettes, one eight-bit row per selected nonblank tile.</summary>
    internal readonly byte[] faces = new byte[(TileCount - 1) * 8];
    /// <summary>Authored pixels where the native shading departs from the shared shadow rule.</summary>
    internal readonly Dictionary<int, byte>? edits;

    /// <summary>Separates supplied letter ink from its calculable shadow and retains only nonuniform art edits.</summary>
    internal MenuLargeFontArtwork(IndexedPngImage image)
    {
        foreach (int tile in Tiles())
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
            if (tile != MenuLargeFontCellDefinitions.TransparentCell && Source(tile, x, y) == 14) faces[Index(tile) * 8 + y] |= (byte)(1 << x);
        foreach (int tile in Tiles())
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = Source(tile, x, y);
            if (pixel != Basis(tile, new(checked((ushort)x), checked((ushort)y)))) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
        }
        byte Source(int tile, int x, int y) => image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
    }

    /// <summary>Reviewed large-letter cells and the transparent cell (<see cref="MenuLargeFontCellDefinitions.Cells"/>).</summary>
    internal static bool Contains(int tile) => MenuAtlasCellRange.AnyContains(MenuLargeFontCellDefinitions.Cells, tile);
    /// <summary>Enumerates the reviewed large-letter cell set in native tile order.</summary>
    private static IEnumerable<int> Tiles()
    {
        for (int tile = 0; tile <= MenuLargeFontCellDefinitions.LastCell; tile++) if (Contains(tile)) yield return tile;
    }
    /// <summary>Compacts a native tile number into the stored ink-row array, omitting the transparent cell.</summary>
    private static int Index(int tile)
    {
        int index = 0;
        for (int candidate = 0; candidate < tile; candidate++) if (candidate != MenuLargeFontCellDefinitions.TransparentCell && Contains(candidate)) index++;
        return index;
    }
    /// <summary>Finds the upper letter half when a lower half's shadow needs ink across the tile boundary.</summary>
    private static int UpperHalf(int tile) => MenuLargeFontCellDefinitions.UpperHalves.TryGetValue(tile, out int upper) ? upper : -1;
    /// <summary>Retrieves the authored pixel when it differs from the calculated ink/shadow basis.</summary>
    internal byte Pixel(int tile, Position16<CharacterPixels> point)
    {
        if (!Contains(tile) || (uint)point.X >= 8 || (uint)point.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + point.Y * 8 + point.X, out byte pixel)
            ? pixel : Basis(tile, point);
    }

    /// <summary>Calculates index-13 shadow from index-14 ink, including touching corners and upper-half continuation.</summary>
    private byte Basis(int tile, Position16<CharacterPixels> point)
    {
        if (tile == MenuLargeFontCellDefinitions.TransparentCell) return 0;
        int row = Index(tile) * 8 + point.Y;
        if ((faces[row] & (1 << point.X)) != 0) return 14;
        if (point.X == 0) return 0;
        if (Ink(point.X - 1, point.Y - 1)) return 13;
        if (Ink(point.X - 1, point.Y) && (Ink(point.X, point.Y - 1) ||
            Ink(point.X + 1, point.Y) && Ink(point.X, point.Y + 1) && !Ink(point.X + 1, point.Y - 1) && Ink(point.X - 2, point.Y - 2))) return 13;
        return 0;

        bool Ink(int px, int py)
        {
            if ((uint)px >= 8 || py >= 8) return false;
            int sourceTile = tile;
            if (py < 0)
            {
                sourceTile = UpperHalf(tile);
                if (sourceTile < 0) return false;
                py += 8;
            }
            return (faces[Index(sourceTile) * 8 + py] & (1 << px)) != 0;
        }
    }
}
