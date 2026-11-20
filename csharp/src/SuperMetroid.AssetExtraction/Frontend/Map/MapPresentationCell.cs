using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// One tile of a menu or map page as the import's extracted documents hold it (#2556): the character a page's character
/// sheet places there by its column and row on the sheet, its colour variant, priority and mirroring. Only the import reads
/// these documents, to picture each page once (#1467); this is the only place a placed tile meets the cell that stores it.
/// </summary>
[EncodingOwner("import-data menu and map page cell")]
public sealed record MapPresentationCell
{
    /// <summary>The cell that stores <paramref name="tile"/>, placed from a character sheet <paramref name="columns"/> characters wide.</summary>
    internal static MapPresentationCell Of(BackgroundTile tile, int columns) => new()
    {
        TileColumn = tile.Tile % columns,
        TileRow = tile.Tile / columns,
        Palette = tile.ColourVariant,
        Priority = tile.HighPriority,
        FlipX = tile.FlipHorizontally,
        FlipY = tile.FlipVertically,
    };

    /// <summary>
    /// The tile this cell places from a character sheet <paramref name="columns"/> characters wide and <paramref name="rows"/>
    /// high.
    /// </summary>
    /// <param name="columns">The character sheet's width in characters.</param>
    /// <param name="rows">The character sheet's height in characters.</param>
    /// <param name="cell">Names the cell in a failure.</param>
    /// <exception cref="InvalidDataException">The cell's character lies off the sheet or its colour variant is none.</exception>
    internal BackgroundTile Placement(int columns, int rows, string cell) =>
        (uint)TileColumn < (uint)columns && (uint)TileRow < (uint)rows && (uint)Palette < BackgroundTile.ColourVariants
            ? new BackgroundTile(TileRow * columns + TileColumn, Palette, FlipX, FlipY, Priority)
            : throw new InvalidDataException($"{cell} has an invalid atlas coordinate or palette.");

    /// <summary>Zero-based 8-pixel character column from 0 through 31 in <c>map-tiles.png</c>; combined with the atlas row to form the character index.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based 8-pixel character row from 0 through 7 in the 256-by-64-pixel <c>map-tiles.png</c> atlas.</summary>
    public required int TileRow { get; init; }
    /// <summary>Tile palette index from 0 through 7, encoded in bits 10–12; screen projection may adjust exploration or HUD palette attributes.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether to set the tile priority bit in the compiled word; this is a display attribute, not a reveal flag.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether to mirror the selected character horizontally, setting the packed word's horizontal-flip bit.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether to mirror the selected character vertically, setting the packed word's vertical-flip bit.</summary>
    public required bool FlipY { get; init; }
}
