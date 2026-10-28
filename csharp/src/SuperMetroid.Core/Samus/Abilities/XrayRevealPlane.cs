using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The X-ray reveal BG2 shows, drawn from block pictures (#1392): every block of the two-screen reveal buffer as the block
/// definition it draws and how, over the pictures the room's blocks show now. The buffer is written in whole blocks, so
/// each block's four tiles are one definition laid out one of the reveal's ways; a block copied from beyond the room
/// lies below or right of anything BG2's scroll shows, and draws nothing.
/// </summary>
public sealed class XrayRevealPlane
{

    /// <summary>The blocks, row by row, <see cref="XrayTilemapLayout.MetatileColumns"/> to a row; null beyond the room.</summary>
    private readonly RevealedBlock?[] blocks;

    /// <summary>Keeps the blocks and the pictures they draw from.</summary>
    /// <exception cref="ArgumentException">The blocks do not fill the reveal's rows and columns.</exception>
    /// <exception cref="InvalidDataException">A block draws a definition the pictures do not hold.</exception>
    public XrayRevealPlane(ReadOnlySpan<RevealedBlock?> blocks, RoomBlockPictureTable pictures)
    {
        Ensure.LengthEqual(blocks, XrayTilemapLayout.MetatileColumns * XrayTilemapLayout.MetatileRows);
        Pictures = Ensure.NotNull(pictures);
        foreach (RevealedBlock? shown in blocks)
        {
            if (shown is { } block && pictures.Picture(block.Definition) is null)
                throw new InvalidDataException($"The X-ray reveal draws block definition {block.Definition}, which was not resolved.");
        }
        this.blocks = blocks.ToArray();
    }

    /// <summary>The pictures the blocks draw from.</summary>
    public RoomBlockPictureTable Pictures { get; }

    /// <summary>The blocks, row by row; null where a block was copied from beyond the room.</summary>
    public ReadOnlySpan<RevealedBlock?> Blocks => blocks;

    /// <summary>
    /// The blocks the reveal buffer's <paramref name="tiles"/> show, each found among <paramref name="index"/>'s ways of
    /// laying out the room's definitions.
    /// </summary>
    /// <exception cref="InvalidDataException">A block's tiles are no definition laid out any of the reveal's ways.</exception>
    public static RevealedBlock?[] BlocksOf(ReadOnlySpan<BackgroundTile> tiles, RevealedBlockIndex index)
    {
        Ensure.LengthEqual(tiles, XrayTilemapLayout.BufferWords);
        Ensure.NotNull(index);
        var found = new RevealedBlock?[XrayTilemapLayout.MetatileColumns * XrayTilemapLayout.MetatileRows];
        for (int row = 0; row < XrayTilemapLayout.MetatileRows; row++)
        {
            for (int column = 0; column < XrayTilemapLayout.MetatileColumns; column++)
            {
                int first = XrayRevealTilemap.FirstCellOf(column, row);
                found[row * XrayTilemapLayout.MetatileColumns + column] = index.Find(new ExpandedBlockTiles(tiles[first], tiles[first + 1],
                    tiles[first + ScreenGeometry.TilemapScreenWidthInTiles], tiles[first + ScreenGeometry.TilemapScreenWidthInTiles + 1]));
            }
        }
        return found;
    }
}
