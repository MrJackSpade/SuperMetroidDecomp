using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's bridge and invisible-wall block draws: which room-object draws are the arena's, their run geometry and how their
/// cells collide, from <see cref="CrocomireArenaObjectDrawDefinitions"/>.
/// </summary>
internal static class CrocomireArenaDraws
{
    /// <summary>Bridge or three-column wall geometry with its independently selected solidity.</summary>
    internal readonly record struct Draw(CrocomireArenaDraw Frame, CrocomireArenaShape Shape, int BlocksPerRun)
    {
        /// <summary>Whether the draw writes wall columns rather than a bridge run.</summary>
        internal bool Wall => Shape != CrocomireArenaShape.Bridge;

        /// <summary>Wall layouts draw three columns; bridge layouts draw one horizontal run.</summary>
        internal int RunCount => CrocomireArenaObjectDrawDefinitions.Cells[Frame].Length;
        /// <summary>How a bridge cell or wall cell collides; the wall is solid only in the create-wall state.</summary>
        internal RoomCollisionType CollisionAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= BlocksPerRun)
                throw new ArgumentOutOfRangeException(nameof(block));
            return CrocomireArenaObjectDrawDefinitions.Cells[Frame][run][block];
        }
    }

    /// <summary>
    /// Describes a draw handed over by the shared room-object drawing; draws of other families are not the arena's.
    /// </summary>
    internal static bool TryDescribe(RoomObjectDraw draw, out Draw shape)
    {
        if (draw is not RoomObjectFrameDraw<CrocomireArenaDraw> { Frame: var frame })
        {
            shape = default;
            return false;
        }
        shape = new(frame, CrocomireArenaObjectDrawDefinitions.Shapes[frame], CrocomireArenaObjectDrawDefinitions.Cells[frame][0].Length);
        return true;
    }
}
