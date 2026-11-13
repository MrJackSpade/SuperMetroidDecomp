using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Compatibility;

/// <summary>
/// Confirms that <see cref="RoomReadingOrder"/> and the blocks <see cref="BlockScanCycle"/> names (#1628) are the blocks
/// the original's level-data places address: a block's place is its row times the room's width plus its column, a step
/// adds its columns and its rows times the width to the place, a scan's place is counted within 32768 blocks, and the
/// block at a place is the place's remainder and quotient by the width.
/// </summary>
internal static class RoomReadingOrderVerification
{
    /// <summary>Room widths: one block, two, an odd width, a screen, one past a screen, a wide room and the widest.</summary>
    private static readonly int[] Widths = [1, 2, 3, 16, 17, 200, 255];

    /// <summary>The blocks a scan along the rows counts before it continues from the room's first block.</summary>
    private const int ScanCycleBlocks = 32768;

    /// <summary>
    /// Compares every reading-order step with the level-data place it replaced, across row ends, from columns past the
    /// room's width and before the room's first block.
    /// </summary>
    internal static void VerifyRoomReadingOrder()
    {
        foreach (int width in Widths)
            VerifySteps(width);

        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomReadingOrder.Step(Position16<RoomBlocks>.Zero, Offset16<RoomBlocks>.Zero, 0), "a room no block wide");
        Console.WriteLine("Room reading order: every step reaches the block the level-data place it replaced addresses.");
    }

    /// <summary>
    /// Compares every block a scan names with the level-data place it replaced: rows past the scan's 256-row cycle,
    /// columns past the room's width, places past the 32768-block cycle, and extension links before the room's first
    /// block.
    /// </summary>
    internal static void VerifyBlockScanCycleBlocks()
    {
        foreach (int width in Widths)
        {
            VerifyScanBlocks(width);
            VerifyLinkedBlocks(width);
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => BlockScanCycle.LinkedBlock(Position16<RoomBlocks>.Of(0, 1), Offset16<RoomBlocks>.Of(0, -2), 16),
            "an extension link before the room's first block");
        Console.WriteLine("Block scan cycle: every scan block is the block the level-data place it replaced addresses.");
    }

    /// <summary>
    /// Every step of up to two rows and a row's blocks either way, from the first rows and from every column up to one
    /// past the width.
    /// </summary>
    private static void VerifySteps(int width)
    {
        for (int y = 0; y <= 3; y++)
        for (int x = 0; x <= width + 1; x++)
        for (int stepY = -2; stepY <= 2; stepY++)
        for (int stepX = -width - 1; stepX <= width + 1; stepX++)
        {
            int place = y * width + x + stepY * width + stepX;
            Position16<RoomBlocks>? expected = place < 0 ? null : BlockAtPlace(place, width);
            Position16<RoomBlocks>? reached = RoomReadingOrder.Step(
                Position16<RoomBlocks>.Of(x, y), Offset16<RoomBlocks>.Of(stepX, stepY), width);
            if (expected != reached)
                AssertEqual(expected, reached, $"a step of ({stepX},{stepY}) from ({x},{y}) in a room {width} blocks wide");
        }
    }

    /// <summary>
    /// The block a scan along the rows and a scan down the rows reach, from every row a lookup can name and from columns
    /// at, before and past the room's width, for steps across a row's end and past the cycle.
    /// </summary>
    private static void VerifyScanBlocks(int width)
    {
        int[] columns = [0, 1, width - 1, width, width + 1, 2 * width, 255, 4095];
        int[] steps = [0, 1, 2, width - 1, width, width + 1, 255, 256, 4095];
        for (int row = 0; row <= 4095; row++)
        foreach (int column in columns)
        foreach (int step in steps)
        {
            int first = row % 256 * width + column;
            var start = Position16<RoomBlocks>.Of(column, row);
            Position16<RoomBlocks> along = BlockAtPlace((first + step) % ScanCycleBlocks, width);
            Position16<RoomBlocks> scannedAlong = BlockScanCycle.BlockAlongRows(start, width, step);
            if (along != scannedAlong)
                AssertEqual(along, scannedAlong, $"a scan {step} blocks along the rows from {start} in a room {width} blocks wide");
            Position16<RoomBlocks> down = BlockAtPlace((first + step * width) % ScanCycleBlocks, width);
            Position16<RoomBlocks> scannedDown = BlockScanCycle.BlockDownRows(start, width, step);
            if (down != scannedDown)
                AssertEqual(down, scannedDown, $"a scan {step} rows down from {start} in a room {width} blocks wide");
        }
    }

    /// <summary>
    /// Every extension link from every block a scan can reach, for the ends and middle of the link range, and every
    /// link from the blocks before the 32768-block cycle's end.
    /// </summary>
    private static void VerifyLinkedBlocks(int width)
    {
        int[] links = [RoomBlockBehavior.MostBackwardExtension, -1, 0, 1, RoomBlockBehavior.MostForwardExtension];
        for (int place = 0; place < ScanCycleBlocks; place++)
        foreach (int link in links)
            VerifyLink(place, link, width);
        for (int place = ScanCycleBlocks - 2 * width; place < ScanCycleBlocks; place++)
        for (int link = RoomBlockBehavior.MostBackwardExtension; link <= RoomBlockBehavior.MostForwardExtension; link++)
            VerifyLink(place, link, width);
    }

    /// <summary>Compares one horizontal and one vertical link from the block at <paramref name="place"/>.</summary>
    private static void VerifyLink(int place, int link, int width)
    {
        Position16<RoomBlocks> block = BlockAtPlace(place, width);
        if (place + link >= 0)
        {
            Position16<RoomBlocks> expected = BlockAtPlace((place + link) % ScanCycleBlocks, width);
            Position16<RoomBlocks> linked = BlockScanCycle.LinkedBlock(block, Offset16<RoomBlocks>.Of(link, 0), width);
            if (expected != linked)
                AssertEqual(expected, linked, $"a horizontal link of {link} from {block} in a room {width} blocks wide");
        }
        if (place + link * width >= 0)
        {
            Position16<RoomBlocks> expected = BlockAtPlace((place + link * width) % ScanCycleBlocks, width);
            Position16<RoomBlocks> linked = BlockScanCycle.LinkedBlock(block, Offset16<RoomBlocks>.Of(0, link), width);
            if (expected != linked)
                AssertEqual(expected, linked, $"a vertical link of {link} from {block} in a room {width} blocks wide");
        }
    }

    /// <summary>The block a level-data place addresses: the place's remainder by the width is its column, its quotient its row.</summary>
    private static Position16<RoomBlocks> BlockAtPlace(int place, int width) => Position16<RoomBlocks>.Of(place % width, place / width);
}
