using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's wall-cannon block draws: which PLM draws are the cannon's, their run geometry and cells from
/// <see cref="DraygonCannonObjectDrawDefinitions"/>.
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
        /// <summary>
        /// Where a run starts, in blocks from the cannon's origin: a right frame's lower row one block down; a left
        /// frame's split top-row cell one block left, then its lower row one block left and down.
        /// </summary>
        internal Offset16<RoomBlocks> Start(int run)
        {
            CheckRun(run);
            return new(!Right && run > 0 ? -1 : 0, run == (Right ? 1 : 2) ? 1 : 0);
        }
        /// <summary>The cannon cell's collision, mirror and shield or damage tile.</summary>
        internal RoomTerrainBlock BlockAt(int run, int cell)
        {
            if ((uint)cell >= (uint)BlockCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            return DraygonCannonObjectDrawDefinitions.Cells[this][run][cell];
        }
    }

    /// <summary>Decodes one of the twelve reachable cannon draw pointers into facing, shield state, and frame.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw)
    {
        if (request is not RoomObjectFrameDraw<DraygonCannonDraw>) { draw = default; return false; }
        DraygonCannonDraw frame = RoomObjectDraw.FrameOf<DraygonCannonDraw>(request, "DraygonCannonDraw");
        draw = DraygonCannonObjectDrawDefinitions.Draws.TryGetValue(frame, out Draw described)
            ? described
            : throw new InvalidOperationException($"Undefined {nameof(DraygonCannonDraw)} {request}.");
        return true;
    }
}
