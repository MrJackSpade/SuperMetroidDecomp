using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's wall-cannon block draws: which PLM draws are the cannon's, their run geometry and cells from
/// <see cref="DraygonCannonPlmDrawDefinitions"/>, and their editable artwork identities.
/// </summary>
internal static class DraygonCannonDraws
{
    /// <summary>One of the twelve 2x2 cannon frames: its facing, shield state and frame.</summary>
    internal readonly record struct Draw(HorizontalDirection Side, DraygonCannonCondition Condition, int Frame) : IPlmRunDrawShape
    {
        /// <summary>Whether the cannon faces right.</summary>
        internal bool Right => Side == HorizontalDirection.Right;

        /// <summary>Whether the frame shows the shield.</summary>
        internal bool Shield => Condition == DraygonCannonCondition.Shielded;

        /// <summary>Right frames draw two horizontal runs; left frames split the first row.</summary>
        public int RunCount => Right ? 2 : 3;
        /// <summary>Rejects a draw-run index outside this frame's native write order.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
        /// <summary>Counts cells in a horizontal cannon row or split left-side run.</summary>
        public int WordCount(int run)
        {
            CheckRun(run);
            return Right || run == 2 ? 2 : 1;
        }
        /// <summary>Moves left between split top-row cells and otherwise keeps the X cursor.</summary>
        public sbyte NextX(int run)
        {
            CheckRun(run);
            return !Right && run < 2 ? (sbyte)-1 : (sbyte)0;
        }
        /// <summary>Advances to the lower row after the right row or left split pair.</summary>
        public sbyte NextY(int run)
        {
            CheckRun(run);
            return run == (Right ? 0 : 1) ? (sbyte)1 : (sbyte)0;
        }
        /// <summary>The cannon cell's collision, mirror and shield or damage tile.</summary>
        public ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            return DraygonCannonPlmDrawDefinitions.Cells[this][run][cell].Raw;
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

    /// <summary>Enumerates only the right and left cannon lists reached by the PLM programs.</summary>
    private static IEnumerable<PlmDraw> Draws()
    {
        foreach (DraygonCannonDraw draw in Enum.GetValues<DraygonCannonDraw>()) yield return PlmDraw.Of(draw);
    }
    /// <summary>Builds temporary draw-list views for all reachable cannon frames.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (PlmDraw draw in Draws())
            {
                TryGet(draw, out var list);
                yield return list;
            }
        }
    }
    /// <summary>Builds a temporary artwork draw-list DTO; runtime drawing reads cells directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.TryCompile<Draw>(request, TryDescribe, out list);
    /// <summary>Finds a reachable cannon draw list by its editable visual ID.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(Draws(), VisualId, TryGet, id, out list);
    /// <summary>Maps a native cannon draw pointer to its facing, condition, and pose ID.</summary>
    internal static string VisualId(PlmDraw request) =>
        DraygonCannonPlmDrawDefinitions.VisualIds.TryGetValue(
            PlmDraw.FrameOf<DraygonCannonDraw>(request, "Draygon cannon draw with a visual ID"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined {nameof(DraygonCannonDraw)} {request}.");
}
