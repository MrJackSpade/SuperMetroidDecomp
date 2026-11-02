using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's wall-cannon block draws: which PLM draws are the cannon's, their run geometry and cells from
/// <see cref="DraygonCannonPlmDrawDefinitions"/>.
/// </summary>
internal static class DraygonCannonDraws
{
    /// <summary>One of the twelve 2x2 cannon frames: its facing, shield state and frame.</summary>
    internal readonly record struct Draw(HorizontalDirection Side, DraygonCannonCondition Condition, int Frame)
    {
        /// <summary>Whether the cannon faces right.</summary>
        internal bool Right => Side == HorizontalDirection.Right;

        /// <summary>Whether the frame shows the shield.</summary>
        internal bool Shield => Condition == DraygonCannonCondition.Shielded;

        /// <summary>Right frames draw two horizontal runs; left frames split the first row.</summary>
        internal int RunCount => Right ? 2 : 3;
        /// <summary>Rejects a draw-run index outside this frame's write order.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
        /// <summary>Counts cells in a horizontal cannon row or split left-side run.</summary>
        internal int BlockCount(int run)
        {
            CheckRun(run);
            return Right || run == 2 ? 2 : 1;
        }
        /// <summary>Moves left between split top-row cells and otherwise keeps the X cursor.</summary>
        internal int NextX(int run)
        {
            CheckRun(run);
            return !Right && run < 2 ? -1 : 0;
        }
        /// <summary>Advances to the lower row after the right row or left split pair.</summary>
        internal int NextY(int run)
        {
            CheckRun(run);
            return run == (Right ? 0 : 1) ? 1 : 0;
        }
        /// <summary>The cannon cell's collision, mirror and shield or damage tile.</summary>
        internal RoomTerrainBlock BlockAt(int run, int cell)
        {
            if ((uint)cell >= (uint)BlockCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            return DraygonCannonPlmDrawDefinitions.Cells[this][run][cell];
        }
    }

    /// <summary>Decodes one of the twelve reachable cannon draw pointers into facing, shield state, and frame.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        if (request is not PlmFrameDraw<DraygonCannonDraw>) { draw = default; return false; }
        DraygonCannonDraw frame = PlmDraw.FrameOf<DraygonCannonDraw>(request, "DraygonCannonDraw");
        draw = DraygonCannonPlmDrawDefinitions.Draws.TryGetValue(frame, out Draw described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(DraygonCannonDraw)} {request}.");
        return true;
    }
}
