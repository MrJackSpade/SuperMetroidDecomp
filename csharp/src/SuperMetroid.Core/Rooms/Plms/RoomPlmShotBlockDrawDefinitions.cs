using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Immutable bank-$84 level-word mutations drawn by the ordinary shot-block PLMs.
/// Each word contains both collision type and a visual block reference; keeping the
/// entire native word here prevents artwork replacement from changing terrain rules.
/// </summary>
internal static class RoomPlmShotBlockDrawDefinitions
{

    /// <summary>One run: its direction, the complete level words it writes, then the signed offset to the next run.</summary>
    /// <param name="Direction">The direction the run writes its blocks in.</param>
    /// <param name="LevelWords">The level words the run writes, one per block.</param>
    /// <param name="Next">The signed offset to the next run.</param>
    internal readonly record struct Run(
        PlmRunDirection Direction,
        ReadOnlyMemory<ushort> LevelWords,
        Offset16<RoomBlocks> Next)
    {
        /// <summary>The number of blocks the run writes: one per level word.</summary>
        internal int Count => LevelWords.Length;
    }

    /// <summary>One PLM draw with its ordered, immutable level-word runs.</summary>
    internal readonly record struct DrawList(PlmDraw Draw, ReadOnlyMemory<Run> Runs)
    {
    }

    /// <summary>Sixteen breakup frames and three parent/child restoration draws.</summary>
    internal const int DrawCount = 19;
    /// <summary>Physical block arrangement affected by a shot-block draw list.</summary>
    internal enum Shape
    {
        /// <summary>One breakable block.</summary>
        Single,
        /// <summary>Two adjacent blocks in a row.</summary>
        Horizontal,
        /// <summary>Two stacked blocks in a column.</summary>
        Vertical,
        /// <summary>Four blocks arranged in two rows.</summary>
        Square,
    }

    /// <summary>Bounded native draw: four breakup stages per shape at $84:A345-$A3DC,
    /// or parent/child restoration at $84:A47B-$A49A. Breakup uses consecutive
    /// visual blocks $53-$55 then air; restoration uses the shape's base tile,
    /// consecutive columns and a 32-index row stride, with native collision links.</summary>
    [IndependentFlag("whether a shot-block draw is the restoring variant, a fact of the decoded draw list")]
    internal readonly record struct Draw(Shape Layout, int Frame, bool Restore)
    {
        /// <summary>Gets two runs for a square or one for the other block arrangements.</summary>
        internal int RunCount => Layout == Shape.Square ? 2 : 1;
        /// <summary>Gets the number of level words in each run for this shape.</summary>
        internal int WordsPerRun => Layout == Shape.Single ? 1 : 2;
        /// <summary>Whether the native draw writes its words down a column.</summary>
        internal bool Vertical => Layout == Shape.Vertical;

        /// <summary>Calculates breakup air or a restored parent/child collision word at a block position.</summary>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= WordsPerRun)
                throw new ArgumentOutOfRangeException(nameof(block));
            if (!Restore) return Frame == 3 ? (ushort)0x00ff : (ushort)(0x0053 + Frame);
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            int tile = Layout switch
            {
                Shape.Horizontal => 0x96,
                Shape.Vertical => 0x98,
                Shape.Single or Shape.Square => 0x99,
                _ => throw new InvalidOperationException($"Undefined shot-block shape {Layout}."),
            };
            // Parent is shootable; children link left on the first row and up below.
            int collision = row == 0 && column == 0 ? 0xc000 : row == 0 ? 0x5000 : 0xd000;
            return (ushort)(collision | (tile + 32 * row + column));
        }
    }

    /// <summary>Resolves a published visual resource identity to its compiled draw list.</summary>
    internal delegate bool VisualLookup(string id, out DrawList list);

    /// <summary>A lookup of a compiled draw list by its draw.</summary>
    internal delegate bool DrawListLookup(PlmDraw draw, out DrawList list);

    /// <summary>Finds the draw list among <paramref name="candidates"/> whose published visual resource identity is <paramref name="id"/>.</summary>
    internal static bool FindByVisualId(IEnumerable<DrawList> candidates, Func<PlmDraw, string> visualId, string? id, out DrawList found) =>
        PlmDraw.TryFindByVisualId(candidates, candidate => visualId(candidate.Draw), id, out found);

    /// <summary>Compiles the draw list of the first of <paramref name="draws"/> whose visual resource identity is <paramref name="id"/>.</summary>
    internal static bool FindByVisualId(IEnumerable<PlmDraw> draws, Func<PlmDraw, string> visualId, DrawListLookup lookup, string? id, out DrawList found)
    {
        foreach (PlmDraw draw in draws)
        {
            if (string.Equals(id, visualId(draw), StringComparison.Ordinal))
                return lookup(draw, out found);
        }
        found = default;
        return false;
    }

    /// <summary>Describes the compiled shape of a draw.</summary>
    internal delegate bool ShapeLookup<TShape>(PlmDraw draw, [MaybeNullWhen(false)] out TShape shape);

    /// <summary>Builds the draw list of the run-shaped draw <paramref name="describe"/> finds for <paramref name="draw"/>.</summary>
    internal static bool TryCompile<TShape>(PlmDraw draw, ShapeLookup<TShape> describe, out DrawList list) where TShape : IPlmRunDrawShape
    {
        list = default;
        if (!describe(draw, out TShape? shape)) return false;
        list = Compile(draw, shape);
        return true;
    }

    /// <summary>Builds the draw list of the door cap <paramref name="describe"/> finds for <paramref name="draw"/>.</summary>
    internal static bool TryCompileCap<TShape>(PlmDraw draw, ShapeLookup<TShape> describe, out DrawList list) where TShape : IPlmCapDrawShape
    {
        list = default;
        if (!describe(draw, out TShape? shape)) return false;
        list = CompileCap(draw, shape);
        return true;
    }

    /// <summary>Builds the run-by-run draw list a compiled draw shape describes.</summary>
    private static DrawList Compile<TShape>(PlmDraw draw, TShape shape) where TShape : IPlmRunDrawShape
    {
        var runs = new Run[shape.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = shape.WordAt(run, cell);
            runs[run] = new(PlmRunDirection.Horizontal, words, new(shape.NextX(run), shape.NextY(run)));
        }
        return new(draw, runs);
    }

    /// <summary>Builds the one four-word run of a door cap draw.</summary>
    private static DrawList CompileCap<TShape>(PlmDraw draw, TShape shape) where TShape : IPlmCapDrawShape
    {
        var words = new ushort[4];
        for (int cell = 0; cell < 4; cell++) words[cell] = shape.WordAt(cell);
        return new(draw, new Run[] { new(shape.Direction, words, new(0, 0)) });
    }

    /// <summary>Breakup frame <paramref name="frame"/>, 0 through 3, of a shot block of <paramref name="shape"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside 0 through 3.</exception>
    internal static PlmFrameDraw<Draw> Breakup(Shape shape, int frame) =>
        PlmDraw.Of(new Draw(shape, (uint)frame < BreakupFrameCount ? frame : throw new ArgumentOutOfRangeException(nameof(frame)), Restore: false));

    /// <summary>The draw that restores a broken shot block of <paramref name="shape"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> is a single block, which has no restoration draw.</exception>
    internal static PlmFrameDraw<Draw> Restoration(Shape shape) =>
        PlmDraw.Of(new Draw(shape != Shape.Single ? shape : throw new ArgumentOutOfRangeException(nameof(shape)), 0, Restore: true));

    /// <summary>Breakup frames per shape.</summary>
    private const int BreakupFrameCount = 4;

    /// <summary>The published artwork identity of a shot-block draw: its shape and breakup frame, or its restoration.</summary>
    internal static string VisualId(Draw draw)
    {
        string shape = draw.Layout switch
        {
            Shape.Single => "single",
            Shape.Horizontal => "horizontal",
            Shape.Vertical => "vertical",
            Shape.Square => "square",
            _ => throw new InvalidOperationException($"Undefined shot-block shape {draw.Layout}."),
        };
        return draw.Restore ? $"{shape}-restore" : $"{shape}-breakup-{draw.Frame}";
    }

    /// <summary>The published artwork identity of a shot-block <paramref name="draw"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a shot-block draw.</exception>
    internal static string VisualId(PlmDraw draw) => VisualId(PlmDraw.FrameOf<Draw>(draw, "shot-block"));

    /// <summary>Finds the shot-block draw whose artwork identity is <paramref name="id"/>.</summary>
    internal static bool TryGetByVisualId(string? id, out Draw draw) =>
        PlmDraw.TryFindByVisualId(Calculated, VisualId, id, out draw);

    /// <summary>Yields breakup frames interleaved by shape, followed by the three restoration draws.</summary>
    internal static IEnumerable<Draw> Calculated
    {
        get
        {
            for (int frame = 0; frame < BreakupFrameCount; frame++)
            {
                yield return new Draw(Shape.Single, frame, Restore: false);
                yield return new Draw(Shape.Horizontal, frame, Restore: false);
                yield return new Draw(Shape.Vertical, frame, Restore: false);
                yield return new Draw(Shape.Square, frame, Restore: false);
            }
            yield return new Draw(Shape.Horizontal, 0, Restore: true);
            yield return new Draw(Shape.Vertical, 0, Restore: true);
            yield return new Draw(Shape.Square, 0, Restore: true);
        }
    }

    /// <summary>Materializes draw-list DTOs for asset tooling; runtime uses calculated draws directly.</summary>
    internal static IEnumerable<DrawList> All => Calculated.Select(Export);

    /// <summary>Expands a calculated shot-block draw into ordered level-word runs and offsets.</summary>
    private static DrawList Export(Draw draw)
    {
        var runs = new Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordsPerRun];
            for (int block = 0; block < words.Length; block++) words[block] = draw.WordAt(run, block);
            runs[run] = new(draw.Vertical ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal, words,
                new(0, (sbyte)(run + 1 < runs.Length ? 1 : 0)));
        }
        return new(PlmDraw.Of(draw), runs);
    }
}
