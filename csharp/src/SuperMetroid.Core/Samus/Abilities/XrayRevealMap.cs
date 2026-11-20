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
    /// <param name="foregroundPosition">
    /// Camera position, which selects the revealed blocks. The base copy reads the foreground plane's block at each revealed
    /// block (#1351, #1555), from the block holding the camera's left and top edges onwards; each revealed block that lies
    /// in the room then shows its own reveal (#1792), and a block beyond the room shows nothing.
    /// </param>
    /// <param name="area">The room's area; only Brinstar shows Brinstar-only reveals.</param>
    /// <param name="visuals">The installed reveal blocks.</param>
    /// <returns>The revealed blocks by place; null beyond the room.</returns>
    public static XrayRevealGrid Build(RoomLevelData level, Position16<RoomPixels> foregroundPosition,
        AreaId area, InstalledXrayReveals visuals)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(visuals);
        RoomBlockCells foreground = level.ForegroundPlane.Cells;
        Position16<RoomBlocks> cameraBlock = foregroundPosition.ToRoomBlock();
        var result = new XrayRevealGrid();
        foreach (Position16<RoomBlocks> place in XrayRevealGrid.Places)
            result.Show(place, Copied(place));
        for (int row = 0; row < XrayRevealGrid.Size.Y; row++)
        {
            // A wide or square reveal in the room's column just before the revealed area shows its right half in the
            // first column. Its halves are copied before this row's own reveals overwrite them.
            if (RoomBlock(level, cameraBlock.X - 1, cameraBlock.Y + row) is { } before &&
                Lookup(before)?.Blocks is { } predecessor)
            {
                if (predecessor.BottomRight is { } bottomRight)
                    CopyBlock(Position16<RoomBlocks>.Of(0, row + 1), bottomRight);
                if (predecessor.TopRight is { } topRight)
                    CopyBlock(Position16<RoomBlocks>.Of(0, row), topRight);
            }
            for (int column = 0; column < XrayRevealGrid.Size.X; column++)
            {
                var place = Position16<RoomBlocks>.Of(column, row);
                if (RoomBlock(level, cameraBlock.X + column, cameraBlock.Y + row) is not { } block ||
                    Lookup(block) is not { } reveal)
                    continue;
                if (reveal.Blocks is not { } shown)
                {
                    // An extension shows the block its chain reaches.
                    if (XrayRevealExtensions.Resolve(level, block, visuals) is { } replacement)
                        CopyBlock(place, replacement);
                    continue;
                }
                if (!XrayRevealShapes.RevealsIn(reveal.Shape, area))
                    continue;
                CopyBlock(place, shown.TopLeft);
                if (shown.TopRight is { } topRight)
                    CopyBlock(place + Right, topRight);
                if (shown.BottomLeft is { } bottomLeft)
                    CopyBlock(place + Down, bottomLeft);
                if (shown.BottomRight is { } bottomRight)
                    CopyBlock(place + Right + Down, bottomRight);
            }
        }
        return result;

        XrayRevealDefinition? Lookup(Position16<RoomBlocks> at)
        {
            RoomCollisionBlock block = level.GetObjectCollisionBlock(at);
            return visuals.Apply(block.CollisionType, block.Behavior);
        }
        // The foreground block the revealed block lies over, mirrored as the level mirrors it.
        RevealedBlock? Copied(Position16<RoomBlocks> place)
        {
            if (RoomBlock(level, cameraBlock.X + place.X, cameraBlock.Y + place.Y) is not { } block)
                return null;
            RoomBlockCell cell = foreground[block];
            return new RevealedBlock(cell.Picture, Mirrored(cell.Flips));
        }
        // A reveal block is placed as it is defined. Only blocks inside the revealed area are shown.
        void CopyBlock(Position16<RoomBlocks> place, RoomCellPicture picture) =>
            result.Show(place, new RevealedBlock(picture, RevealedBlockArrangement.AsDefined));
    }

    /// <summary>The next place to the right, where a wide or square reveal's right half lies.</summary>
    private static Offset16<RoomBlocks> Right => Offset16<RoomBlocks>.Of(1, 0);

    /// <summary>The next place down, where a tall or square reveal's lower half lies.</summary>
    private static Offset16<RoomBlocks> Down => Offset16<RoomBlocks>.Of(0, 1);

    /// <summary>
    /// The room's block at <paramref name="column"/> and <paramref name="row"/>, or null when that place lies beyond the
    /// room: the reveal shows only the room's own blocks, so nothing it looks up runs on into another row or past an edge.
    /// </summary>
    /// <param name="level">The room.</param>
    /// <param name="column">The block's column, which may lie before the room's first or past its last.</param>
    /// <param name="row">The block's row, which may lie above the room's top or below its bottom.</param>
    internal static Position16<RoomBlocks>? RoomBlock(RoomLevelData level, int column, int row) =>
        column >= 0 && row >= 0 && column < level.Size.X && row < level.Size.Y ? Position16<RoomBlocks>.Of(column, row) : null;

    /// <summary>
    /// The foreground pixel the revealed area's first block lies on: the top-left corner of the room block the camera lies
    /// in, so every revealed block lies over the room's own.
    /// </summary>
    /// <param name="foregroundPosition">Camera position.</param>
    public static Position16<RoomPixels> OriginOf(Position16<RoomPixels> foregroundPosition) =>
        foregroundPosition.ToRoomBlock().ToRoomPixels();

    /// <summary>How a revealed block lays out its picture: mirrored as the room mirrors it.</summary>
    internal static RevealedBlockArrangement Mirrored(BlockMirroring flips) => flips switch
    {
        BlockMirroring.None => RevealedBlockArrangement.AsDefined,
        BlockMirroring.Horizontal => RevealedBlockArrangement.MirroredHorizontally,
        BlockMirroring.Vertical => RevealedBlockArrangement.MirroredVertically,
        BlockMirroring.Horizontal | BlockMirroring.Vertical => RevealedBlockArrangement.MirroredBoth,
        _ => throw new InvalidDataException($"A revealed block has undefined flips {flips}."),
    };
}
