using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's bridge and invisible-wall block draws: which PLM draws are the arena's, their run geometry and cells
/// from <see cref="CrocomireArenaPlmDrawDefinitions"/>, and their editable artwork identities.
/// </summary>
internal static class CrocomireArenaDraws
{
    /// <summary>Decoded bridge or three-column wall geometry with its independently selected solid collision bit.</summary>
    internal readonly record struct Draw(CrocomireArenaDraw Frame, CrocomireArenaShape Shape, int WordsPerRun)
    {
        /// <summary>Whether the draw writes wall columns rather than a bridge run.</summary>
        internal bool Wall => Shape != CrocomireArenaShape.Bridge;

        /// <summary>Wall layouts draw three columns; bridge layouts draw one horizontal run.</summary>
        internal int RunCount => CrocomireArenaPlmDrawDefinitions.Cells[Frame].Length;
        /// <summary>Wall columns write down their column; the bridge writes along its row.</summary>
        internal PlmRunDirection Direction => Wall ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal;
        /// <summary>Offsets the next wall column from the draw origin rather than from the prior column.</summary>
        internal sbyte NextX(int run)
        {
            if ((uint)run >= RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return run + 1 < RunCount ? (sbyte)(run + 1) : (sbyte)0;
        }
        /// <summary>The bridge tile or the wall's alternating rows and final base row, solid only in the create-wall state.</summary>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= WordsPerRun)
                throw new ArgumentOutOfRangeException(nameof(block));
            return CrocomireArenaPlmDrawDefinitions.Cells[Frame][run][block].Raw;
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

    /// <summary>Projects the physical cells into temporary artwork records; gameplay evaluates cells directly.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (CrocomireArenaDraw frame in Enum.GetValues<CrocomireArenaDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Reconstructs one supported bridge or wall draw with its native run offsets.</summary>
    internal static bool TryGet(PlmDraw draw, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(draw, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[shape.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.WordsPerRun];
            for (int block = 0; block < words.Length; block++) words[block] = shape.WordAt(run, block);
            runs[run] = new(shape.Direction, words, new(shape.NextX(run), 0));
        }
        list = new(draw, runs);
        return true;
    }

    /// <summary>Maps an arena draw to the published bridge or wall artwork key.</summary>
    internal static string VisualId(PlmDraw draw) =>
        CrocomireArenaPlmDrawDefinitions.VisualIds[PlmDraw.FrameOf<CrocomireArenaDraw>(draw, "Crocomire arena")];

    /// <summary>Finds the physical bridge or wall layout for one editable visual identity.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        ShotBlockPlmDraws.FindByVisualId(All, VisualId, id, out draw);
}
