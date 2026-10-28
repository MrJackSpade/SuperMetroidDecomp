using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>Menu atlas cellsAD/AE atB6:D5A0..D5DF are a horizontal line and its
/// left corner. Index6 covers row1; the corner also covers column0 below that row.
/// All other pixels are transparent. Calculate these lines directly, retaining only
/// independently supplied artwork differences, never a generated tile cache.</summary>
internal sealed class MenuThinBorderArtwork
{
    /// <summary>The authored border uses only the horizontal-line and left-corner cells.</summary>
    internal const int TileCount = 2;
    /// <summary>Only pixels differing from the calculable stock line pattern are retained.</summary>
    internal readonly Dictionary<int, byte>? edits;
    /// <summary>Admits atlas tile $AD or $AE, the two thin-border cells.</summary>
    internal static bool Contains(int tile) => Enum.IsDefined((MenuThinBorderCell)tile);

    /// <summary>Compares both tile cells with the stock line pattern and records independent pixel edits.</summary>
    internal MenuThinBorderArtwork(IndexedPngImage image)
    {
        foreach (int tile in Enum.GetValues<MenuThinBorderCell>().Select(cell => (int)cell))
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int pixel = image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
            if (pixel != Basis((MenuThinBorderCell)tile, new(checked((ushort)x), checked((ushort)y)))) (edits ??= []).Add(tile * 64 + y * 8 + x, (byte)pixel);
        }
    }

    /// <summary>Returns an edited pixel when present, otherwise the stock line or transparent zero.</summary>
    internal byte Pixel(int tile, Position16<CharacterPixels> point)
    {
        if (!Contains(tile) || (uint)point.X >= 8 || (uint)point.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        return edits is not null && edits.ContainsKey(tile * 64 + point.Y * 8 + point.X) ? edits[tile * 64 + point.Y * 8 + point.X] : Basis((MenuThinBorderCell)tile, point);
    }

    /// <summary>Draws palette index six across row one and down the left edge of the corner cell.</summary>
    private static byte Basis(MenuThinBorderCell cell, Position16<CharacterPixels> point) =>
        point.Y == 1 || cell == MenuThinBorderCell.Corner && point.X == 0 && point.Y > 1 ? (byte)6 : (byte)0;
}
