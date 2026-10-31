using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Builds the cartridge's copied-BG1/revealed-BG2 screens in native write order, one whole 16x16 block at a time (#1431):
/// each cell is the block it shows and how it lays the block's picture out.
/// </summary>
public static class XrayRevealMap
{
    /// <summary>
    /// Ports $91:CB8E-CFD9's base-copy and terrain-reveal passes. Item and special-room
    /// overlays are applied afterwards by their respective owners, as on the cartridge.
    /// </summary>
    /// <param name="level">The room whose blocks the reveal resolves and whose BG1 plane the base copy reads.</param>
    /// <param name="layer1Position">Layer one's position, which selects the revealed blocks.</param>
    /// <param name="bg1Offset">
    /// BG1's scroll less layer one's position. The base copy reads the BG1 plane's block at each cell's scrolled position
    /// (#1351), from the block holding the screen's left and top edges onwards, where the cartridge read the BG1 tilemap
    /// back out of VRAM; a cell beyond the room shows nothing.
    /// </param>
    /// <param name="area">The room's area; only Brinstar shows Brinstar-only reveals.</param>
    /// <param name="visuals">Installed reveal art, or null for the stock table.</param>
    /// <returns>The shown cells, row by row, <see cref="XrayRevealLayout.MetatileColumns"/> to a row; null beyond the room.</returns>
    public static RevealedBlock?[] Build(RoomLevelData level, Position16<RoomPixels> layer1Position,
        Offset16<RoomPixels> bg1Offset, AreaId area, InstalledXrayReveals? visuals = null)
    {
        Ensure.NotNull(level);
        RoomBlockCells bg1 = level.ForegroundPlane.Cells;
        Position16<RoomPixels> bg1Scroll = layer1Position + bg1Offset;
        var result = new RevealedBlock?[XrayRevealLayout.Cells];
        for (int row = 0; row < XrayRevealLayout.MetatileRows; row++)
        for (int col = 0; col < XrayRevealLayout.MetatileColumns; col++)
            Place(col, row, Copied(col, row));
        int firstBlock = WordWrap.LowByte(layer1Position.Y >> 4) * level.Size.X + (layer1Position.X >> 4);
        for (int row = 0; row < XrayRevealLayout.MetatileRows; row++)
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
            for (int col = 0; col < XrayRevealLayout.MetatileColumns; col++)
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
                // The last column of screen one suppresses right-hand writes. Screen
                // two is visited separately with native remaining-count zero, not one.
                bool right = col != XrayRevealLayout.MetatileColumns - 2;
                if (right && reveal.Command is XrayRevealCommand.CopyWide or XrayRevealCommand.CopySquare)
                    CopyMetatile(col + 1, row, reveal.TopRight);
                if (reveal.Command is XrayRevealCommand.CopyTall or XrayRevealCommand.CopySquare)
                    CopyMetatile(col, row + 1, reveal.BottomLeft);
                if (right && reveal.Command == XrayRevealCommand.CopySquare)
                    CopyMetatile(col + 1, row + 1, reveal.BottomRight);
            }
        }
        return result;

        XrayRevealDefinition? Lookup(int index)
        {
            RoomCollisionBlock block = level.GetPlmCollisionBlockByIndex(index);
            return visuals is not null
                ? visuals.Apply(block.CollisionType, block.Behavior)
                : XrayRevealTable.Find(block.CollisionType, block.Behavior);
        }
        // The BG1 block shown at the cell's scrolled position, mirrored as the level mirrors it.
        RevealedBlock? Copied(int col, int row)
        {
            int x = CoordinateAxis.Wrap(((bg1Scroll.X >> 4) + col) * RoomLevelPlane.BlockPixels - bg1Offset.X);
            int y = CoordinateAxis.Wrap(((bg1Scroll.Y >> 4) + row) * RoomLevelPlane.BlockPixels - bg1Offset.Y);
            if (x >= bg1.Size.X * RoomLevelPlane.BlockPixels || y >= bg1.Size.Y * RoomLevelPlane.BlockPixels)
                return null;
            RoomBlockCell cell = bg1.Cells[y / RoomLevelPlane.BlockPixels * bg1.Size.X + x / RoomLevelPlane.BlockPixels];
            return new RevealedBlock(cell.Definition, Mirrored(cell.Flips));
        }
        // A reveal block is placed as it is defined; its number is validated as a room block.
        void CopyMetatile(int col, int row, int metatile) =>
            Place(col, row, new RevealedBlock(RoomBlockAppearance.Of(metatile).Block, RevealedBlockArrangement.AsDefined));
        void Place(int col, int row, RevealedBlock? block)
        {
            // Bottom-edge multi-block commands can write past the two transferred screens, into
            // native scratch RAM; only the shown cells belong to this display projection.
            int cell = XrayRevealCells.CellOf(col, row);
            if (cell >= 0) result[cell] = block;
        }
    }

    /// <summary>How a copied block lays out its picture: mirrored as the room mirrors it.</summary>
    private static RevealedBlockArrangement Mirrored(BlockMirroring flips) => flips switch
    {
        BlockMirroring.None => RevealedBlockArrangement.AsDefined,
        BlockMirroring.Horizontal => RevealedBlockArrangement.MirroredHorizontally,
        BlockMirroring.Vertical => RevealedBlockArrangement.MirroredVertically,
        BlockMirroring.Horizontal | BlockMirroring.Vertical => RevealedBlockArrangement.MirroredBoth,
        _ => throw new InvalidDataException($"A BG1 block has undefined flips {flips}."),
    };
}
