using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectShotBlockDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The breakup and restoration draws of the ordinary shot blocks and the terrain each writes.</summary>
internal static class ShotBlockObjectDraws
{
    /// <summary>Breakup frame <paramref name="frame"/>, 0 through 3, of a shot block of <paramref name="shape"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside 0 through 3.</exception>
    internal static RoomObjectFrameDraw<Draw> Breakup(Shape shape, int frame) =>
        RoomObjectDraw.Of(new Draw(shape, (uint)frame < BreakupFrameCount ? frame : throw new ArgumentOutOfRangeException(nameof(frame)), Restore: false));

    /// <summary>The draw that restores a broken shot block of <paramref name="shape"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> is a single block, which has no restoration draw.</exception>
    internal static RoomObjectFrameDraw<Draw> Restoration(Shape shape) =>
        RoomObjectDraw.Of(new Draw(shape != Shape.Single ? shape : throw new ArgumentOutOfRangeException(nameof(shape)), 0, Restore: true));

    /// <summary>A shot-block draw: four breakup stages per shape, or parent/child restoration. Breakup uses consecutive
    /// visual blocks 83 to 85 then air; restoration uses the shape's base tile,
    /// consecutive columns and a 32-index row stride, with collision links.</summary>
    [IndependentFlag("whether a shot-block draw is the restoring variant, a fact of the decoded draw list")]
    internal readonly record struct Draw(Shape Layout, int Frame, bool Restore)
    {
        /// <summary>Gets two runs for a square or one for the other block arrangements.</summary>
        internal int RunCount => Layout == Shape.Square ? 2 : 1;
        /// <summary>Gets the number of blocks in each run for this shape.</summary>
        internal int BlocksPerRun => Layout == Shape.Single ? 1 : 2;
        /// <summary>Whether the draw writes its blocks down a column.</summary>
        internal bool Vertical => Layout == Shape.Vertical;

        /// <summary>Calculates breakup air or a restored parent/child block at a block position.</summary>
        internal RoomTerrainBlock BlockAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= BlocksPerRun)
                throw new ArgumentOutOfRangeException(nameof(block));
            if (!Restore)
                return Frame == 3 ? RoomTerrainBlocks.BlankAir
                    : RoomTerrainBlock.Of(BreakupFirstBlock + Frame, BlockMirroring.None, RoomCollisionType.Air);
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            int tile = RestoreBlocks.TryGetValue(Layout, out int restored)
                ? restored
                : throw new InvalidOperationException($"Undefined shot-block shape {Layout}.");
            // Parent is shootable; children link left on the first row and up below.
            RoomCollisionType collision = row == 0 && column == 0 ? RoomCollisionType.ShootableBlock
                : row == 0 ? RoomCollisionType.HorizontalExtension : RoomCollisionType.VerticalExtension;
            return RoomTerrainBlock.Of(tile + RestoreRowStride * row + column, BlockMirroring.None, collision);
        }
    }
}
