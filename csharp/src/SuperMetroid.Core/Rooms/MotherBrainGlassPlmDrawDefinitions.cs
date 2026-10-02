namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical bank-$84 draw layouts selected by Mother Brain's glass PLM. The
/// multi-run records include signed offsets from the PLM origin; the native
/// instruction program still owns damage thresholds, shards, and timing.
/// </summary>
internal static class MotherBrainGlassPlmDrawDefinitions
{
    /// <summary>Initial single glass block at $84:9717.</summary>
    internal const ushort Initial = 0x9717;
    /// <summary>First full pane damage frame at $84:971D.</summary>
    internal const ushort PaneDamage1 = 0x971d;
    /// <summary>Second full pane damage frame at $84:9731.</summary>
    internal const ushort PaneDamage2 = 0x9731;
    /// <summary>Three-block pane transition at $84:9745.</summary>
    internal const ushort PaneTransition = 0x9745;
    /// <summary>First shifted pane damage frame at $84:974F.</summary>
    internal const ushort ShiftedPane1 = 0x974f;
    /// <summary>Second shifted pane damage frame at $84:9769.</summary>
    internal const ushort ShiftedPane2 = 0x9769;
    /// <summary>Reduced shifted pane frame at $84:9781.</summary>
    internal const ushort ShiftedPane3 = 0x9781;
    /// <summary>First four-run shatter frame at $84:978F.</summary>
    internal const ushort Shatter1 = 0x978f;
    /// <summary>Second four-run shatter frame at $84:97B7.</summary>
    internal const ushort Shatter2 = 0x97b7;
    /// <summary>Third four-run shatter frame at $84:97E7.</summary>
    internal const ushort Shatter3 = 0x97e7;
    /// <summary>Four-run empty-glass frame at $84:9817.</summary>
    internal const ushort Cleared = 0x9817;

    /// <summary>
    /// Eleven named glass states. Vertical panes reflect their lower halves; the
    /// shifted pane mirrors its inner column, and shatter states clear the outer
    /// columns before replacing the centre supports. Offsets are signed and absolute
    /// from the PLM origin. NTSC source: 84:9717..9846.
    /// </summary>
    internal readonly record struct Draw(ushort Pointer)
    {
        private bool Pane => Pointer is PaneDamage1 or PaneDamage2;
        private bool Shifted => Pointer is ShiftedPane1 or ShiftedPane2 or ShiftedPane3;
        internal int RunCount => Pointer switch
        {
            Initial or PaneTransition => 1,
            PaneDamage1 or PaneDamage2 or ShiftedPane3 => 2,
            ShiftedPane1 or ShiftedPane2 => 3,
            _ => 4,
        };
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        internal int WordCount(int run)
        {
            CheckRun(run);
            if (Pointer == Initial || Shifted && run == 0) return 1;
            if (Pane) return run == 0 ? 4 : 2;
            if (Pointer == PaneTransition) return 3;
            if (Shifted) return run == 2 ? 2 : Pointer == ShiftedPane1 ? 4 : Pointer == ShiftedPane2 ? 3 : 2;
            return Pointer == Shatter1 && run >= 2 ? 2 : 4;
        }
        internal bool Vertical(int run)
        {
            CheckRun(run);
            return Pointer != Initial && !(Shifted && run == 0);
        }
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            if (run == RunCount - 1) return 0;
            return Pane ? (sbyte)-1 : (sbyte)(run - 3);
        }
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            if (run == RunCount - 1) return 0;
            return (sbyte)(Pane || Shifted && (run != 0 || Pointer != ShiftedPane1) ||
                Pointer == Shatter1 && run >= 1 ? 1 : 0);
        }
        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new IndexOutOfRangeException();
            if (Pointer == Initial) return 0xc6c0;
            if (Pointer == Cleared) return 0x00ff;
            if (Pane)
            {
                bool outer = run == 0 && cell is 0 or 3;
                int tile = outer ? 0x2c7 : (run == 0 ? 0x2c9 : 0x2c8) + (Pointer == PaneDamage2 ? 2 : 0);
                int collision = run == 0 && cell == 0 ? 12 : run == 0 && cell == 3 ? 5 : 13;
                int flip = cell >= (run == 0 ? 2 : 1) ? 0x800 : 0;
                return (ushort)(collision << 12 | flip | tile);
            }
            if (Pointer == PaneTransition)
                return cell == 0 ? (ushort)0xc2c7 : (ushort)(0x2cc | (cell == 2 ? 0x800 : 0));
            if (Shifted)
            {
                if (run == 0) return 0xc2c7;
                if (Pointer == ShiftedPane3) return (ushort)(0x6cc | (cell == 1 ? 0x800 : 0));
                int row = run == 1 ? cell + (Pointer == ShiftedPane2 ? 1 : 0) : cell + 1;
                bool outer = run == 1 && row is 0 or 3;
                int tile = outer ? 0x2cd : (run == 1 ? 0x2c9 : 0x2c8) + (Pointer == ShiftedPane2 ? 2 : 0);
                return (ushort)(0x8000 | tile | (outer ? 0 : 0x400) | (row >= 2 ? 0x800 : 0));
            }
            if (run < 2)
            {
                bool outer = cell is 0 or 3;
                if (Pointer == Shatter3 || !outer && Pointer == Shatter2) return 0x00ff;
                int collision = !outer ? 0 : run == 1 ? 8 : cell == 0 ? 12 : 5;
                return (ushort)(collision << 12 | (outer ? 0x2ce : 0x2cf) |
                    (run == 1 ? 0x400 : 0) | (cell >= 2 ? 0x800 : 0));
            }
            if (Pointer == Shatter1)
                return (ushort)(0xd2d0 | (run == 2 ? 0x400 : 0) | (cell == 1 ? 0x800 : 0));
            int supportTile = (Pointer == Shatter2 ? 0x2c2 : 0x2d2) + (cell == 0 ? 0 : cell == 3 ? 2 : 1);
            int supportCollision = Pointer == Shatter2 ? (cell == 0 ? 5 : 13) : 0;
            return (ushort)(supportCollision << 12 | supportTile | (run == 3 ? 0x400 : 0) | (cell == 2 ? 0x800 : 0));
        }
    }

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        bool owned = pointer is Initial or PaneDamage1 or PaneDamage2 or PaneTransition or
            ShiftedPane1 or ShiftedPane2 or ShiftedPane3 or Shatter1 or Shatter2 or Shatter3 or Cleared;
        draw = owned ? new(pointer) : default;
        return owned;
    }
    private static IEnumerable<ushort> Pointers()
    {
        yield return Initial;
        yield return PaneDamage1;
        yield return PaneDamage2;
        yield return PaneTransition;
        yield return ShiftedPane1;
        yield return ShiftedPane2;
        yield return ShiftedPane3;
        yield return Shatter1;
        yield return Shatter2;
        yield return Shatter3;
        yield return Cleared;
    }
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
    // Temporary artwork DTOs; gameplay calculates individual cells directly.
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new((ushort)(words.Length | (draw.Vertical(run) ? 0x8000 : 0)), words, draw.NextX(run), draw.NextY(run));
        }
        list = new(pointer, runs);
        return true;
    }
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (ushort pointer in Pointers())
            if (string.Equals(id, VisualId(pointer), StringComparison.Ordinal)) return TryGet(pointer, out list);
        list = default;
        return false;
    }
    internal static string VisualId(ushort pointer) => pointer switch
    {
        Initial => "initial",
        PaneDamage1 => "pane-damage-1",
        PaneDamage2 => "pane-damage-2",
        PaneTransition => "pane-transition",
        ShiftedPane1 => "shifted-pane-1",
        ShiftedPane2 => "shifted-pane-2",
        ShiftedPane3 => "shifted-pane-3",
        Shatter1 => "shatter-1",
        Shatter2 => "shatter-2",
        Shatter3 => "shatter-3",
        Cleared => "cleared",
        _ => throw new InvalidDataException(
            $"Mother Brain glass draw ${pointer:X4} has no visual ID."),
    };

}
