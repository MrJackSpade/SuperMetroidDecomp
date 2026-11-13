using System.Collections.Immutable;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Rooms;

/// <summary>
/// Checks the room grid's own bounds in place of byte and word bounds (#1627): a room's width is any positive number of
/// blocks, a room plane and a picture grid are at least one cell along each axis, and the block conversions that replaced
/// hand-written masks (the doorway nudges and Samus's block spans) give the masks' results for every room coordinate.
/// </summary>
internal static class RoomGridBoundsVerification
{
    /// <summary>Exercises a room wider than 255 blocks, the planes' and grids' least size, and the block conversions.</summary>
    internal static void VerifyRoomGridBounds()
    {
        const int width = 300, height = 2;
        RoomTerrainBlock[] foreground = [.. Enumerable.Range(0, width * height)
            .Select(index => RoomTerrainBlock.Of(index % RoomBlockAppearance.LastBlock, BlockMirroring.None, RoomCollisionType.Air))];
        var wide = new RoomLevelData(new Extent16<RoomBlocks>(width, height),
            new RoomLevelPlanes(foreground, new RoomBlockBehavior[width * height], new RoomBlockAppearance[width * height]),
            RoomLevelData.NothingBeyondGrid());
        AssertEqual(foreground[width - 1], wide.GetCollisionBlock(new(width - 1, 0)).Terrain, "a room keeps its 300th column");
        AssertEqual(foreground[width], wide.GetCollisionBlock(new(0, 1)).Terrain, "a 300-block row strides to the next row's first block");
        AssertThrows<ArgumentOutOfRangeException>(() => new RoomLevelData(new Extent16<RoomBlocks>(0, 1),
            new RoomLevelPlanes([], [], []), RoomLevelData.NothingBeyondGrid()), "a room is at least one block wide");

        AssertEqual(1, new RoomBlockCells(new Extent16<RoomBlocks>(1, 1), [new RoomBlockCell(0, BlockMirroring.None)]).Cells.Length,
            "a room plane of one block");
        AssertThrows<ArgumentOutOfRangeException>(() => new RoomBlockCells(new Extent16<RoomBlocks>(0, 1), []),
            "a room plane is at least one block wide");
        AssertThrows<ArgumentOutOfRangeException>(() => new RoomBlockCells(new Extent16<RoomBlocks>(1, 0), []),
            "a room plane is at least one block tall");
        AssertEqual(1, new PictureGrid(new Extent16<MapTiles>(1, 1), [null]).Cells.Length, "a picture grid of one square");
        AssertThrows<ArgumentOutOfRangeException>(() => new PictureGrid(new Extent16<MapTiles>(0, 1), ImmutableArray<GridPicture?>.Empty),
            "a picture grid is at least one square wide");
        AssertThrows<ArgumentOutOfRangeException>(() => new PictureGrid(new Extent16<MapTiles>(1, 0), ImmutableArray<GridPicture?>.Empty),
            "a picture grid is at least one square tall");

        for (int pixel = 0; pixel <= ushort.MaxValue; pixel++)
        {
            AssertEqual(pixel & ~15, RoomGridConversions.BlockStart(pixel), $"pixel {pixel}: its block's first pixel");
            AssertEqual(pixel | 15, RoomGridConversions.BlockEnd(pixel), $"pixel {pixel}: its block's last pixel");
            AssertEqual((pixel >> 4) & 15, RoomGridConversions.BlockRowWithinScreen(pixel), $"pixel {pixel}: its block within its screen");
        }

        // The save station's trigger column and its centring of Samus (#1632) replaced a shift by four and a rounding
        // shift pair; both give the shifts' results for every room coordinate.
        int blockMismatches = 0, boundaryMismatches = 0;
        for (int pixel = 0; pixel <= ushort.MaxValue; pixel++)
        {
            blockMismatches += RoomGridConversions.PixelToBlock(pixel) == pixel >> 4 ? 0 : 1;
            boundaryMismatches += RoomGridConversions.NearestBlockBoundary(pixel) == ((pixel + 8) >> 4) << 4 ? 0 : 1;
        }
        AssertEqual(0, blockMismatches, "every room coordinate's block is the coordinate shifted right four");
        AssertEqual(0, boundaryMismatches, "every room coordinate's nearest block boundary is the half-block rounding shift pair's");
        AssertThrows<ArgumentOutOfRangeException>(() => RoomGridConversions.PixelToBlock(-1), "a block below the room's coordinates is refused");
        AssertThrows<ArgumentOutOfRangeException>(() => RoomGridConversions.NearestBlockBoundary(ushort.MaxValue + 1),
            "a boundary past the room's coordinates is refused");

        // Samus's changed-pose probe (#1632) replaced a two's-complement mask and half-block bit; the block step's centre
        // gives the mask's result for every signed whole-pixel displacement.
        int centreMismatches = 0;
        for (int displacement = short.MinValue; displacement <= short.MaxValue; displacement++)
            centreMismatches += RoomGridConversions.BlockStepCentre(displacement) == ((displacement & ~15) | 8) ? 0 : 1;
        AssertEqual(0, centreMismatches, "every signed displacement's block step centre is the masked displacement with its half-block bit");
        AssertEqual(-8, RoomGridConversions.BlockStepCentre(-8), "a probe eight pixels back reaches its block step's centre at -8");
        AssertThrows<ArgumentOutOfRangeException>(() => RoomGridConversions.BlockStepCentre(short.MaxValue + 1),
            "a displacement past the signed whole-pixel range is refused");
        Console.WriteLine("Room grid bounds: a 300-block room, one-cell planes and grids, and every coordinate's block conversions.");
    }
}
