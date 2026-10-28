using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's glass block draws: which PLM draws are the glass's, their run geometry and cells from
/// <see cref="MotherBrainGlassPlmDrawDefinitions"/>, and their editable artwork identities.
/// </summary>
internal static class MotherBrainGlassDraws
{
    /// <summary>
    /// One of eleven named glass states. Offsets are signed and absolute from the PLM origin; panes draw down their
    /// columns, and the shifted pane writes its origin block separately before its inner columns.
    /// </summary>
    internal readonly record struct Draw(MotherBrainGlassDraw Frame)
    {
        /// <summary>Whether this frame is one of the two symmetric full-pane damage layouts.</summary>
        private bool Pane => Frame is MotherBrainGlassDraw.PaneDamage1 or MotherBrainGlassDraw.PaneDamage2;
        /// <summary>Whether the pane has shifted and uses separate inner-column runs.</summary>
        private bool Shifted => Frame is MotherBrainGlassDraw.ShiftedPane1 or MotherBrainGlassDraw.ShiftedPane2 or MotherBrainGlassDraw.ShiftedPane3;
        /// <summary>Native draw-record count for the selected glass damage phase.</summary>
        internal int RunCount => MotherBrainGlassPlmDrawDefinitions.Cells[Frame].Length;
        /// <summary>Rejects a run outside this frame's native draw-record count.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
        /// <summary>Returns the number of level words in one pane or shatter draw run.</summary>
        internal int WordCount(int run)
        {
            CheckRun(run);
            return MotherBrainGlassPlmDrawDefinitions.Cells[Frame][run].Length;
        }
        /// <summary>Selects vertical draw traversal except for the initial and shifted origin blocks.</summary>
        internal bool Vertical(int run)
        {
            CheckRun(run);
            return Frame != MotherBrainGlassDraw.Initial && !(Shifted && run == 0);
        }
        /// <summary>Horizontal origin adjustment before the following run.</summary>
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            if (run == RunCount - 1) return 0;
            return Pane ? (sbyte)-1 : (sbyte)(run - 3);
        }
        /// <summary>Vertical origin adjustment before the following run.</summary>
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            if (run == RunCount - 1) return 0;
            return (sbyte)(Pane || Shifted && (run != 0 || Frame != MotherBrainGlassDraw.ShiftedPane1) ||
                Frame == MotherBrainGlassDraw.Shatter1 && run >= 1 ? 1 : 0);
        }
        /// <summary>The selected block's visual tile, flip bits, and native collision nibble.</summary>
        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            return MotherBrainGlassPlmDrawDefinitions.Cells[Frame][run][cell].Raw;
        }
    }

    /// <summary>Recognizes one of the eleven glass draw pointers without claiming adjacent PLM families.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        // Pointers outside the eleven glass states belong to other draw families.
        bool owned = request is PlmFrameDraw<MotherBrainGlassDraw>;
        draw = owned ? new(PlmDraw.FrameOf<MotherBrainGlassDraw>(request, "MotherBrainGlassDraw")) : default;
        return owned;
    }
    /// <summary>Materializes the eleven glass layouts in native pointer order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (MotherBrainGlassDraw frame in Enum.GetValues<MotherBrainGlassDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }
    /// <summary>Materializes a temporary draw-list DTO; gameplay reads individual cells directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(request, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new(draw.Vertical(run) ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal, words, new(draw.NextX(run), draw.NextY(run)));
        }
        list = new(request, runs);
        return true;
    }
    /// <summary>Finds a glass layout by its stable artwork identifier.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (MotherBrainGlassDraw frame in Enum.GetValues<MotherBrainGlassDraw>())
            if (string.Equals(id, VisualId(PlmDraw.Of(frame)), StringComparison.Ordinal)) return TryGet(PlmDraw.Of(frame), out list);
        list = default;
        return false;
    }
    /// <summary>Maps a native glass draw pointer to its editable presentation frame name.</summary>
    internal static string VisualId(PlmDraw request) =>
        MotherBrainGlassPlmDrawDefinitions.VisualIds.TryGetValue(
            PlmDraw.FrameOf<MotherBrainGlassDraw>(request, "Mother Brain glass draw"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined Mother Brain glass draw {request}.");
}
