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

    /// <summary>
    /// A shot-block draw: four breakup stages per shape, or parent/child restoration. Breakup frames are air;
    /// restoration links its children to a shootable parent. What each frame shows is its installed picture (#1557).
    /// </summary>
    [IndependentFlag("whether a shot-block draw is the restoring variant, a fact of the decoded draw list")]
    internal readonly record struct Draw(Shape Layout, int Frame, bool Restore)
    {
        /// <summary>Gets two runs for a square or one for the other block arrangements.</summary>
        internal int RunCount => Layout == Shape.Square ? 2 : 1;
        /// <summary>Gets the number of blocks in each run for this shape.</summary>
        internal int BlocksPerRun => Layout == Shape.Single ? 1 : 2;
        /// <summary>Whether the draw writes its blocks down a column.</summary>
        internal bool Vertical => Layout == Shape.Vertical;

        /// <summary>How the cell at a block position collides: breakup air, or a restored parent or linked child.</summary>
        internal RoomCollisionType CollisionAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= BlocksPerRun)
                throw new ArgumentOutOfRangeException(nameof(block));
            if (!Restore)
                return RoomCollisionType.Air;
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            // Parent is shootable; children link left on the first row and up below.
            return row == 0 && column == 0 ? RoomCollisionType.ShootableBlock
                : row == 0 ? RoomCollisionType.HorizontalExtension : RoomCollisionType.VerticalExtension;
        }
    }
}
