using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The eleven bank-$84 Mother Brain glass draw lists, $84:9717..9846.</summary>
internal enum MotherBrainGlassDraw : ushort
{
    /// <summary>Initial single glass block at $84:9717.</summary>
    Initial = 0x9717,
    /// <summary>First full pane damage frame at $84:971D.</summary>
    PaneDamage1 = 0x971d,
    /// <summary>Second full pane damage frame at $84:9731.</summary>
    PaneDamage2 = 0x9731,
    /// <summary>Three-block pane transition at $84:9745.</summary>
    PaneTransition = 0x9745,
    /// <summary>First shifted pane damage frame at $84:974F.</summary>
    ShiftedPane1 = 0x974f,
    /// <summary>Second shifted pane damage frame at $84:9769.</summary>
    ShiftedPane2 = 0x9769,
    /// <summary>Reduced shifted pane frame at $84:9781.</summary>
    ShiftedPane3 = 0x9781,
    /// <summary>First four-run shatter frame at $84:978F.</summary>
    Shatter1 = 0x978f,
    /// <summary>Second four-run shatter frame at $84:97B7.</summary>
    Shatter2 = 0x97b7,
    /// <summary>Third four-run shatter frame at $84:97E7.</summary>
    Shatter3 = 0x97e7,
    /// <summary>Four-run empty-glass frame at $84:9817.</summary>
    Cleared = 0x9817,
}

/// <summary>
/// Physical bank-$84 draw layouts selected by Mother Brain's glass PLM. The
/// multi-run records include signed offsets from the PLM origin; the native
/// instruction program still owns damage thresholds, shards, and timing.
/// </summary>
internal static class MotherBrainGlassPlmDrawDefinitions
{
    /// <summary>
    /// Eleven named glass states. Vertical panes reflect their lower halves; the
    /// shifted pane mirrors its inner column, and shatter states clear the outer
    /// columns before replacing the centre supports. Offsets are signed and absolute
    /// from the PLM origin. NTSC source: 84:9717..9846.
    /// </summary>
    internal readonly record struct Draw(MotherBrainGlassDraw Pointer)
    {
        private bool Pane => Pointer is MotherBrainGlassDraw.PaneDamage1 or MotherBrainGlassDraw.PaneDamage2;
        private bool Shifted => Pointer is MotherBrainGlassDraw.ShiftedPane1 or MotherBrainGlassDraw.ShiftedPane2 or MotherBrainGlassDraw.ShiftedPane3;
        internal int RunCount => Pointer switch
        {
            MotherBrainGlassDraw.Initial or MotherBrainGlassDraw.PaneTransition => 1,
            MotherBrainGlassDraw.PaneDamage1 or MotherBrainGlassDraw.PaneDamage2 or MotherBrainGlassDraw.ShiftedPane3 => 2,
            MotherBrainGlassDraw.ShiftedPane1 or MotherBrainGlassDraw.ShiftedPane2 => 3,
            MotherBrainGlassDraw.Shatter1 or MotherBrainGlassDraw.Shatter2 or MotherBrainGlassDraw.Shatter3 or
                MotherBrainGlassDraw.Cleared => 4,
            _ => throw new InvalidOperationException($"Undefined Mother Brain glass draw {Pointer}."),
        };
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        internal int WordCount(int run)
        {
            CheckRun(run);
            if (Pointer == MotherBrainGlassDraw.Initial || Shifted && run == 0) return 1;
            if (Pane) return run == 0 ? 4 : 2;
            if (Pointer == MotherBrainGlassDraw.PaneTransition) return 3;
            if (Shifted) return run == 2 ? 2 : Pointer == MotherBrainGlassDraw.ShiftedPane1 ? 4 : Pointer == MotherBrainGlassDraw.ShiftedPane2 ? 3 : 2;
            return Pointer == MotherBrainGlassDraw.Shatter1 && run >= 2 ? 2 : 4;
        }
        internal bool Vertical(int run)
        {
            CheckRun(run);
            return Pointer != MotherBrainGlassDraw.Initial && !(Shifted && run == 0);
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
            return (sbyte)(Pane || Shifted && (run != 0 || Pointer != MotherBrainGlassDraw.ShiftedPane1) ||
                Pointer == MotherBrainGlassDraw.Shatter1 && run >= 1 ? 1 : 0);
        }
        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new IndexOutOfRangeException();
            if (Pointer == MotherBrainGlassDraw.Initial) return 0xc6c0;
            if (Pointer == MotherBrainGlassDraw.Cleared) return 0x00ff;
            if (Pane)
            {
                bool outer = run == 0 && cell is 0 or 3;
                int tile = outer ? 0x2c7 : (run == 0 ? 0x2c9 : 0x2c8) + (Pointer == MotherBrainGlassDraw.PaneDamage2 ? 2 : 0);
                int collision = run == 0 && cell == 0 ? 12 : run == 0 && cell == 3 ? 5 : 13;
                int flip = cell >= (run == 0 ? 2 : 1) ? 0x800 : 0;
                return (ushort)(collision << 12 | flip | tile);
            }
            if (Pointer == MotherBrainGlassDraw.PaneTransition)
                return cell == 0 ? (ushort)0xc2c7 : (ushort)(0x2cc | (cell == 2 ? 0x800 : 0));
            if (Shifted)
            {
                if (run == 0) return 0xc2c7;
                if (Pointer == MotherBrainGlassDraw.ShiftedPane3) return (ushort)(0x6cc | (cell == 1 ? 0x800 : 0));
                int row = run == 1 ? cell + (Pointer == MotherBrainGlassDraw.ShiftedPane2 ? 1 : 0) : cell + 1;
                bool outer = run == 1 && row is 0 or 3;
                int tile = outer ? 0x2cd : (run == 1 ? 0x2c9 : 0x2c8) + (Pointer == MotherBrainGlassDraw.ShiftedPane2 ? 2 : 0);
                return (ushort)(0x8000 | tile | (outer ? 0 : 0x400) | (row >= 2 ? 0x800 : 0));
            }
            if (run < 2)
            {
                bool outer = cell is 0 or 3;
                if (Pointer == MotherBrainGlassDraw.Shatter3 || !outer && Pointer == MotherBrainGlassDraw.Shatter2) return 0x00ff;
                int collision = !outer ? 0 : run == 1 ? 8 : cell == 0 ? 12 : 5;
                return (ushort)(collision << 12 | (outer ? 0x2ce : 0x2cf) |
                    (run == 1 ? 0x400 : 0) | (cell >= 2 ? 0x800 : 0));
            }
            if (Pointer == MotherBrainGlassDraw.Shatter1)
                return (ushort)(0xd2d0 | (run == 2 ? 0x400 : 0) | (cell == 1 ? 0x800 : 0));
            int supportTile = (Pointer == MotherBrainGlassDraw.Shatter2 ? 0x2c2 : 0x2d2) + (cell == 0 ? 0 : cell == 3 ? 2 : 1);
            int supportCollision = Pointer == MotherBrainGlassDraw.Shatter2 ? (cell == 0 ? 5 : 13) : 0;
            return (ushort)(supportCollision << 12 | supportTile | (run == 3 ? 0x400 : 0) | (cell == 2 ? 0x800 : 0));
        }
    }

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        // Pointers outside the eleven glass states belong to other draw families.
        bool owned = Enum.IsDefined((MotherBrainGlassDraw)pointer);
        draw = owned ? new((MotherBrainGlassDraw)pointer) : default;
        return owned;
    }
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (MotherBrainGlassDraw pointer in Enum.GetValues<MotherBrainGlassDraw>())
            {
                TryGet((ushort)pointer, out var list);
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
        foreach (MotherBrainGlassDraw pointer in Enum.GetValues<MotherBrainGlassDraw>())
            if (string.Equals(id, VisualId((ushort)pointer), StringComparison.Ordinal)) return TryGet((ushort)pointer, out list);
        list = default;
        return false;
    }
    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<MotherBrainGlassDraw>(pointer, "Mother Brain glass draw") switch
    {
        MotherBrainGlassDraw.Initial => "initial",
        MotherBrainGlassDraw.PaneDamage1 => "pane-damage-1",
        MotherBrainGlassDraw.PaneDamage2 => "pane-damage-2",
        MotherBrainGlassDraw.PaneTransition => "pane-transition",
        MotherBrainGlassDraw.ShiftedPane1 => "shifted-pane-1",
        MotherBrainGlassDraw.ShiftedPane2 => "shifted-pane-2",
        MotherBrainGlassDraw.ShiftedPane3 => "shifted-pane-3",
        MotherBrainGlassDraw.Shatter1 => "shatter-1",
        MotherBrainGlassDraw.Shatter2 => "shatter-2",
        MotherBrainGlassDraw.Shatter3 => "shatter-3",
        MotherBrainGlassDraw.Cleared => "cleared",
        _ => throw new InvalidOperationException(
            $"Undefined Mother Brain glass draw ${pointer:X4}."),
    };

}
