using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The X-ray reveal BG2 shows, drawn from block pictures (#1392): every block of the two-screen reveal buffer as the block
/// definition it draws and how, over the pictures the room's blocks show now. The buffer is written in whole blocks, so
/// each block's four tile words are one definition laid out one of the reveal's ways; a block copied from beyond the room
/// lies below or right of anything BG2's scroll shows, and draws nothing.
/// </summary>
public sealed class XrayRevealPlane : IBgSource
{
    /// <summary>Pixels along a block's side.</summary>
    private const int BlockPixels = RoomLevelPlane.BlockPixels;

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
    /// The blocks the reveal buffer's <paramref name="words"/> show, each found among <paramref name="index"/>'s ways of
    /// laying out the room's definitions.
    /// </summary>
    /// <exception cref="InvalidDataException">A block's words are no definition laid out any of the reveal's ways.</exception>
    public static RevealedBlock?[] BlocksOf(ReadOnlySpan<ushort> words, RevealedBlockIndex index)
    {
        Ensure.LengthEqual(words, XrayTilemapLayout.BufferWords);
        Ensure.NotNull(index);
        var found = new RevealedBlock?[XrayTilemapLayout.MetatileColumns * XrayTilemapLayout.MetatileRows];
        for (int row = 0; row < XrayTilemapLayout.MetatileRows; row++)
        {
            for (int column = 0; column < XrayTilemapLayout.MetatileColumns; column++)
            {
                int first = XrayRevealTilemap.Destination(column, row);
                found[row * XrayTilemapLayout.MetatileColumns + column] = index.Find(words[first], words[first + 1],
                    words[first + SnesPpuLayout.TilemapPageWidthInTiles], words[first + SnesPpuLayout.TilemapPageWidthInTiles + 1]);
            }
        }
        return found;
    }

    /// <summary>
    /// The picture pixel the reveal shows at a scrolled BG2 position; the reveal is BG2's two-screen tilemap, the
    /// fine-scroll column the first block column of its second screen.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The position lies past the blocks the reveal builds, or on a block copied from beyond the room, neither of which BG2's
    /// scroll shows.
    /// </exception>
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
    {
        int x = scrolled.X % XrayTilemapLayout.TilemapPixelWidth, y = scrolled.Y % XrayTilemapLayout.TilemapPixelHeight;
        int column = x / BlockPixels, row = y / BlockPixels;
        if (column >= XrayTilemapLayout.MetatileColumns || row >= XrayTilemapLayout.MetatileRows)
            throw new InvalidOperationException($"BG2 shows reveal pixel ({x}, {y}), past the blocks X-ray builds.");
        RevealedBlock block = blocks[row * XrayTilemapLayout.MetatileColumns + column]
            ?? throw new InvalidOperationException($"BG2 shows reveal block ({column}, {row}), which was copied from beyond the room.");
        SpriteSheetFrame picture = Pictures.Picture(block.Definition)
            ?? throw new InvalidOperationException($"The X-ray reveal's block definition {block.Definition} has no picture.");
        if (!Enum.IsDefined(block.Arrangement))
            throw new InvalidDataException($"Unknown revealed block arrangement {block.Arrangement}.");
        bool mirrorX = block.Arrangement is RevealedBlockArrangement.MirroredHorizontally or RevealedBlockArrangement.MirroredBoth;
        bool mirrorY = block.Arrangement is RevealedBlockArrangement.MirroredVertically or RevealedBlockArrangement.MirroredBoth;
        int px = x % BlockPixels, py = y % BlockPixels;
        if (mirrorX)
            px = BlockPixels - 1 - px;
        if (mirrorY)
            py = BlockPixels - 1 - py;
        // Swapped rows show the lower tile row on top; neither row is flipped.
        if (block.Arrangement == RevealedBlockArrangement.RowsSwapped)
            py = (py + BlockPixels / 2) % BlockPixels;
        sample = BgSample.OfPicture(picture, new Position16<ImagePixels>((ushort)px, (ushort)py));
        return true;
    }
}
