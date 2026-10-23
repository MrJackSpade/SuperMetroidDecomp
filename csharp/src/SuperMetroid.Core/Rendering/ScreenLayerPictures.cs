using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend tile layer's tile set (#1392): the pre-coloured tile of each character its cells can show, by the
/// character's index from the layer's character base; none where no artwork the screen loads shows that character.
/// </summary>
public sealed class ScreenLayerPictures
{
    /// <summary>The character indices a cell can show.</summary>
    public const int CharacterIndices = 1024;

    /// <summary>Holds each character index's tile.</summary>
    /// <exception cref="ArgumentException">The tiles do not cover every character index.</exception>
    internal ScreenLayerPictures(ImmutableArray<ScreenTilePicture?> tiles, BgBitDepth depth)
    {
        Ensure.LengthEqual(tiles.AsSpan(), CharacterIndices);
        Tiles = tiles;
        Depth = depth;
        Sheets = [.. tiles.OfType<ScreenTilePicture>().Select(tile => tile.Sheet).Distinct()];
    }

    /// <summary>Each character index's tile, or null where no artwork shows that character.</summary>
    public ImmutableArray<ScreenTilePicture?> Tiles { get; }

    /// <summary>The bit depth of the layer's characters.</summary>
    public BgBitDepth Depth { get; }

    /// <summary>The sheets the tiles come from, each once.</summary>
    public ImmutableArray<ScreenTileSheet> Sheets { get; }

    /// <summary>
    /// What <paramref name="cell"/> shows at <paramref name="pixel"/> of its tile: the tile's pixel under the cell's palette
    /// row, mirrored as the cell mirrors it, with the cell's priority.
    /// </summary>
    /// <exception cref="InvalidDataException">The cell shows a character no artwork the screen loads shows.</exception>
    public BgSample Sample(ScreenTileCell cell, Position16<CharacterPixels> pixel)
    {
        ScreenTilePicture tile = Tiles[cell.Tile]
            ?? throw new InvalidDataException($"A cell shows character {cell.Tile}, which no artwork the screen loads shows.");
        Position16<ImagePixels> corner = tile.Sheet.Corner(tile.SheetTile, cell.ColourVariant);
        int x = cell.FlipHorizontally ? ScreenTileSheet.TilePixels - 1 - pixel.X : pixel.X;
        int y = cell.FlipVertically ? ScreenTileSheet.TilePixels - 1 - pixel.Y : pixel.Y;
        return BgSample.OfTilePicture(tile.Sheet.Picture, new Position16<ImagePixels>((ushort)(corner.X + x), (ushort)(corner.Y + y)),
            cell.HighPriority);
    }
}
