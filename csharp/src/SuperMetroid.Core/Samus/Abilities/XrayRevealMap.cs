using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Builds what the X-ray reveals over the screen, one whole 16x16 block at a time (#1431, #1514): each revealed block is
/// the block it shows and how it lays the block's picture out. A reveal that writes more than one block writes those that
/// lie inside the revealed area.
/// </summary>
public static class XrayRevealMap
{
    /// <summary>
    /// Runs the reveal's base-copy and terrain-reveal passes. Item and special-room
    /// overlays are applied afterwards by their respective owners, as on the game.
    /// </summary>
    /// <param name="level">The room whose blocks the reveal resolves and whose foreground plane the base copy reads.</param>
    /// <param name="foregroundPosition">Camera position, which selects the revealed blocks.</param>
    /// <param name="foregroundOffset">
    /// The foreground's scroll less camera position. The base copy reads the foreground plane's block at each revealed
    /// block's scrolled position (#1351), from the block holding the screen's left and top edges onwards; a block beyond
    /// the room shows nothing.
    /// </param>
    /// <param name="area">The room's area; only Brinstar shows Brinstar-only reveals.</param>
    /// <param name="visuals">Installed reveal art, or null for the stock table.</param>
    /// <returns>The revealed blocks, row by row, <see cref="XrayRevealArea.Columns"/> to a row; null beyond the room.</returns>
    public static RevealedBlock?[] Build(RoomLevelData level, Position16<RoomPixels> foregroundPosition,
        Offset16<RoomPixels> foregroundOffset, AreaId area, InstalledXrayReveals? visuals = null)
    {
        Ensure.NotNull(level);
        RoomBlockCells foreground = level.ForegroundPlane.Cells;
        Position16<RoomPixels> foregroundScroll = foregroundPosition + foregroundOffset;
        var result = new RevealedBlock?[XrayRevealArea.BlockCount];
        for (int row = 0; row < XrayRevealArea.Rows; row++)
        for (int col = 0; col < XrayRevealArea.Columns; col++)
            Place(col, row, Copied(col, row));
        int firstBlock = BlockScanCycle.RowStart(foregroundPosition.Y >> 4, level.Size.X) + (foregroundPosition.X >> 4);
        for (int row = 0; row < XrayRevealArea.Rows; row++)
        {
            int first = firstBlock + row * level.Size.X;
            // The prior column may own a 2-wide reveal crossing the left edge. Its
            // right half is copied before this row's own commands overwrite it.
            if (first != 0 && Lookup(first - 1) is { } predecessor)
            {
                if (predecessor.Command == XrayRevealCommand.CopySquare)
                    CopyMetatile(0, row + 1, predecessor.BottomRight);
                if (predecessor.Command is XrayRevealCommand.CopyWide or XrayRevealCommand.CopySquare)
                    CopyMetatile(0, row, predecessor.TopRight);
            }
            for (int col = 0; col < XrayRevealArea.Columns; col++)
            {
                int index = first + col;
                if (Lookup(index) is not { } reveal) continue;
                if (reveal.Command is XrayRevealCommand.HorizontalExtension or XrayRevealCommand.VerticalExtension)
                {
                    if (XrayRevealExtensions.Resolve(level, index, visuals) is { } replacement)
                        CopyMetatile(col, row, replacement);
                    continue;
                }
                if (reveal.Command == XrayRevealCommand.CopyBrinstar && area != AreaId.Brinstar)
                    continue;
                CopyMetatile(col, row, reveal.TopLeft);
                if (reveal.Command is XrayRevealCommand.CopyWide or XrayRevealCommand.CopySquare)
                    CopyMetatile(col + 1, row, reveal.TopRight);
                if (reveal.Command is XrayRevealCommand.CopyTall or XrayRevealCommand.CopySquare)
                    CopyMetatile(col, row + 1, reveal.BottomLeft);
                if (reveal.Command == XrayRevealCommand.CopySquare)
                    CopyMetatile(col + 1, row + 1, reveal.BottomRight);
            }
        }
        return result;

        XrayRevealDefinition? Lookup(int index)
        {
            RoomCollisionBlock block = level.GetObjectCollisionBlockByIndex(index);
            return visuals is not null
                ? visuals.Apply(block.CollisionType, block.Behavior)
                : XrayRevealTable.Find(block.CollisionType, block.Behavior);
        }
        // The foreground block shown at the revealed block's scrolled position, mirrored as the level mirrors it.
        RevealedBlock? Copied(int col, int row)
        {
            int x = CoordinateAxis.Wrap(((foregroundScroll.X >> 4) + col) * RoomLevelPlane.BlockPixels - foregroundOffset.X);
            int y = CoordinateAxis.Wrap(((foregroundScroll.Y >> 4) + row) * RoomLevelPlane.BlockPixels - foregroundOffset.Y);
            if (x >= foreground.Size.X * RoomLevelPlane.BlockPixels || y >= foreground.Size.Y * RoomLevelPlane.BlockPixels)
                return null;
            RoomBlockCell cell = foreground.Cells[y / RoomLevelPlane.BlockPixels * foreground.Size.X + x / RoomLevelPlane.BlockPixels];
            return new RevealedBlock(cell.Definition, Mirrored(cell.Flips));
        }
        // A reveal block is placed as it is defined; its number is validated as a room block.
        void CopyMetatile(int col, int row, int metatile) =>
            Place(col, row, new RevealedBlock(RoomBlockAppearance.Of(metatile).Block, RevealedBlockArrangement.AsDefined));
        // Only blocks inside the revealed area are shown.
        void Place(int col, int row, RevealedBlock? block)
        {
            if (col < XrayRevealArea.Columns && row < XrayRevealArea.Rows)
                result[row * XrayRevealArea.Columns + col] = block;
        }
    }

    /// <summary>
    /// The foreground pixel the revealed area's first block lies on: the top-left corner of the block the foreground's
    /// scroll lies in, so every revealed block lies over the room's own.
    /// </summary>
    /// <param name="foregroundPosition">Camera position.</param>
    /// <param name="foregroundOffset">The foreground's scroll less camera position.</param>
    public static Position16<RoomPixels> OriginOf(Position16<RoomPixels> foregroundPosition, Offset16<RoomPixels> foregroundOffset)
    {
        Position16<RoomPixels> foregroundScroll = foregroundPosition + foregroundOffset;
        return new(CoordinateAxis.Wrap(foregroundScroll.X >> 4 << 4), CoordinateAxis.Wrap(foregroundScroll.Y >> 4 << 4));
    }

    /// <summary>How a copied block lays out its picture: mirrored as the room mirrors it.</summary>
    private static RevealedBlockArrangement Mirrored(BlockMirroring flips) => flips switch
    {
        BlockMirroring.None => RevealedBlockArrangement.AsDefined,
        BlockMirroring.Horizontal => RevealedBlockArrangement.MirroredHorizontally,
        BlockMirroring.Vertical => RevealedBlockArrangement.MirroredVertically,
        BlockMirroring.Horizontal | BlockMirroring.Vertical => RevealedBlockArrangement.MirroredBoth,
        _ => throw new InvalidDataException($"A foreground block has undefined flips {flips}."),
    };
}
