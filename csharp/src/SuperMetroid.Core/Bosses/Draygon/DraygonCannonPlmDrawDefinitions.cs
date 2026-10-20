using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Physical draw layouts selected by the reachable right- and left-facing
/// Draygon cannon PLM programs. Unused diagonal orientations are not claimed.
/// These full level words and signed row offsets remain gameplay definitions.
/// </summary>
internal static class DraygonCannonPlmDrawDefinitions
{
    /// <summary>
    /// Twelve 2x2 cannon frames. Right lists have two horizontal rows; left lists
    /// split the first row into origin and left cells, preserving native write order.
    /// Shield frames advance two tiles; damaged frames advance one. Lower tiles
    /// are one tileset row (32 tiles) below their upper partners. Right art mirrors X.
    /// </summary>
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
        /// <summary>Calculates a cannon cell's collision nibble, mirror bit, and shield or damage tile.</summary>
        public ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            int row = Right ? run : run == 2 ? 1 : 0;
            bool core = Right ? cell == 0 : run == 0 || run == 2 && cell == 1;
            if (!Shield && !core) return 0x00ff;
            int tile = Shield ? 0x113 + Frame * 2 + (core ? 1 : 0) : 0x180 + Frame;
            int collision = !core ? 0 : Shield ? 12 + row : 10;
            return (ushort)(collision << 12 | (Right ? 0x400 : 0) | (tile + row * 32));
        }
    }

    /// <summary>Decodes one of the twelve reachable cannon draw pointers into facing, shield state, and frame.</summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        if (!Enum.IsDefined((DraygonCannonDraw)pointer)) { draw = default; return false; }
        draw = (DraygonCannonDraw)pointer switch
        {
            DraygonCannonDraw.RightShieldA => new(HorizontalDirection.Right, DraygonCannonCondition.Shielded, 0),
            DraygonCannonDraw.RightShieldB => new(HorizontalDirection.Right, DraygonCannonCondition.Shielded, 1),
            DraygonCannonDraw.RightDamagedA => new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 0),
            DraygonCannonDraw.RightDamagedB => new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 1),
            DraygonCannonDraw.RightDamagedC => new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 2),
            DraygonCannonDraw.RightDamagedD => new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 3),
            DraygonCannonDraw.LeftShieldA => new(HorizontalDirection.Left, DraygonCannonCondition.Shielded, 0),
            DraygonCannonDraw.LeftShieldB => new(HorizontalDirection.Left, DraygonCannonCondition.Shielded, 1),
            DraygonCannonDraw.LeftDamagedA => new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 0),
            DraygonCannonDraw.LeftDamagedB => new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 1),
            DraygonCannonDraw.LeftDamagedC => new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 2),
            DraygonCannonDraw.LeftDamagedD => new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 3),
            _ => throw new InvalidOperationException($"Undefined {nameof(DraygonCannonDraw)} {pointer:X4}."),
        };
        return true;
    }

    /// <summary>Enumerates only the right and left cannon lists reached by the PLM programs.</summary>
    private static IEnumerable<ushort> Pointers()
    {
        foreach (DraygonCannonDraw draw in Enum.GetValues<DraygonCannonDraw>()) yield return (ushort)draw;
    }
    /// <summary>Builds temporary draw-list views for all reachable cannon frames.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (ushort pointer in Pointers())
            {
                TryGet(pointer, out var list);
                yield return list;
            }
        }
    }
    /// <summary>Builds a temporary artwork draw-list DTO; runtime drawing calculates cells directly.</summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.TryCompile<Draw>(pointer, TryDescribe, out list);
    /// <summary>Finds a reachable cannon draw list by its editable visual ID.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(Pointers(), VisualId, TryGet, id, out list);
    /// <summary>Maps a native cannon draw pointer to its facing, condition, and pose ID.</summary>
    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<DraygonCannonDraw>(pointer, "Draygon cannon draw with a visual ID") switch
        {
            DraygonCannonDraw.RightShieldA => "right-shield-a",
            DraygonCannonDraw.RightShieldB => "right-shield-b",
            DraygonCannonDraw.RightDamagedA => "right-damaged-a",
            DraygonCannonDraw.RightDamagedB => "right-damaged-b",
            DraygonCannonDraw.RightDamagedC => "right-damaged-c",
            DraygonCannonDraw.RightDamagedD => "right-damaged-d",
            DraygonCannonDraw.LeftShieldA => "left-shield-a",
            DraygonCannonDraw.LeftShieldB => "left-shield-b",
            DraygonCannonDraw.LeftDamagedA => "left-damaged-a",
            DraygonCannonDraw.LeftDamagedB => "left-damaged-b",
            DraygonCannonDraw.LeftDamagedC => "left-damaged-c",
            DraygonCannonDraw.LeftDamagedD => "left-damaged-d",
            _ => throw new InvalidOperationException($"Undefined {nameof(DraygonCannonDraw)} {pointer:X4}."),
        };

}
