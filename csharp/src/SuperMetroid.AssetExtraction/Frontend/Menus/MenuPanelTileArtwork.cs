using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>Menu OBJ cells57..5A atB6:CAE0..CB5F form a sixteen-row panel edge:
/// upper corner/body then lower corner/body. Calculate outer index3, inner index6
/// and lower highlight8 bands directly; only independently supplied pixel edits remain.</summary>
internal sealed class MenuPanelTileArtwork
{
    /// <summary>The two upper and two lower edge cells occupy four adjacent menu OBJ tiles.</summary>
    internal const int TileCount = 4;
    /// <summary>Only pixels that differ from the calculated panel edge are retained as artwork edits.</summary>
    internal readonly Dictionary<int, byte>? edits;
    /// <summary>Accepts only the four native panel-edge tile indexes.</summary>
    internal static bool Contains(int tile) => Enum.IsDefined((MenuPanelCell)tile);

    /// <summary>Compares installed PNG pixels with the calculated border bands and retains only differences.</summary>
    internal MenuPanelTileArtwork(IndexedPngImage image)
    {
        foreach (int tile in Enum.GetValues<MenuPanelCell>().Select(cell => (int)cell))
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
            if (pixel != Basis((MenuPanelCell)tile, new(checked((ushort)x), checked((ushort)y)))) (edits ??= []).Add(tile * 64 + y * 8 + x, pixel);
        }
    }

    /// <summary>Returns an edited pixel when present, otherwise the calculated corner, band, or highlight value.</summary>
    internal byte Pixel(int tile, Position16<CharacterPixels> point)
    {
        if (!Contains(tile) || (uint)point.X >= 8 || (uint)point.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.TryGetValue(tile * 64 + point.Y * 8 + point.X, out byte pixel) ? pixel : Basis((MenuPanelCell)tile, point);
    }

    /// <summary>Calculates the sixteen-row panel edge from its corner columns and palette-index bands.</summary>
    private static byte Basis(MenuPanelCell cell, Position16<CharacterPixels> point)
    {
        int row = point.Y + (cell is MenuPanelCell.LowerLeft or MenuPanelCell.LowerRight ? 8 : 0);
        if (row is 0 or 15) return 0;
        if (row is 1 or 2 or 14) return 3;
        bool leftCorner = cell is MenuPanelCell.UpperLeft or MenuPanelCell.LowerLeft;
        if (leftCorner && point.X == 0) return 3;
        if (leftCorner && point.X == 1 || row is 3 or 13) return 6;
        return row == 12 ? (byte)8 : (byte)0;
    }
}
