using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's bridge and invisible-wall block draws: which PLM draws are the arena's, their run geometry and cells
/// from <see cref="CrocomireArenaPlmDrawDefinitions"/>.
/// </summary>
internal static class CrocomireArenaDraws
{
    /// <summary>Bridge or three-column wall geometry with its independently selected solidity.</summary>
    internal readonly record struct Draw(CrocomireArenaDraw Frame, CrocomireArenaShape Shape, int BlocksPerRun)
    {
        /// <summary>Whether the draw writes wall columns rather than a bridge run.</summary>
        internal bool Wall => Shape != CrocomireArenaShape.Bridge;

        /// <summary>Wall layouts draw three columns; bridge layouts draw one horizontal run.</summary>
        internal int RunCount => CrocomireArenaPlmDrawDefinitions.Cells[Frame].Length;
        /// <summary>The bridge tile or the wall's alternating rows and final base row, solid only in the create-wall state.</summary>
        internal RoomTerrainBlock BlockAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= BlocksPerRun)
                throw new ArgumentOutOfRangeException(nameof(block));
            return CrocomireArenaPlmDrawDefinitions.Cells[Frame][run][block];
        }
    }

    /// <summary>
    /// Describes a draw handed over by the shared PLM drawing; draws of other families are not the arena's.
    /// </summary>
    internal static bool TryDescribe(PlmDraw draw, out Draw shape)
    {
        if (draw is not PlmFrameDraw<CrocomireArenaDraw> { Frame: var frame })
        {
            shape = default;
            return false;
        }
        shape = new(frame, CrocomireArenaPlmDrawDefinitions.Shapes[frame], CrocomireArenaPlmDrawDefinitions.Cells[frame][0].Length);
        return true;
    }
}
