namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Immutable bank-$84 level-word mutations drawn by the ordinary shot-block PLMs.
/// Each word contains both collision type and a visual block reference; keeping the
/// entire native word here prevents artwork replacement from changing terrain rules.
/// </summary>
internal static class RoomPlmShotBlockDrawDefinitions
{
    /// <summary>First one-block breakup draw list, <c>$84:A345</c>.</summary>
    internal const ushort SingleFrame0 = 0xa345;
    /// <summary>First horizontal-pair breakup draw list, <c>$84:A35D</c>.</summary>
    internal const ushort HorizontalFrame0 = 0xa35d;
    /// <summary>First vertical-pair breakup draw list, <c>$84:A37D</c>.</summary>
    internal const ushort VerticalFrame0 = 0xa37d;
    /// <summary>First square breakup draw list, <c>$84:A39D</c>.</summary>
    internal const ushort SquareFrame0 = 0xa39d;

    /// <summary>The direction/count word's vertical flag: a vertical run writes down a column.</summary>
    private const ushort VerticalRunBit = 0x8000;
    /// <summary>The direction/count word's block count.</summary>
    private const ushort RunCountBits = 0x7fff;

    /// <summary>Whether a run's direction/count word writes down a column.</summary>
    internal static bool IsVerticalRun(ushort directionAndCount) => (directionAndCount & VerticalRunBit) != 0;

    /// <summary>The blocks a run's direction/count word writes.</summary>
    internal static int RunCount(ushort directionAndCount) => directionAndCount & RunCountBits;

    /// <summary>One native record: direction/count, complete level words, then the signed offset to the next record.</summary>
    internal readonly record struct Run(
        ushort DirectionAndCount,
        ReadOnlyMemory<ushort> LevelWords,
        Offset16<RoomBlocks> Next);

    /// <summary>One native shot-block draw pointer with its ordered, immutable level-word runs.</summary>
    internal readonly record struct DrawList(ushort Pointer, ReadOnlyMemory<Run> Runs)
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
    internal readonly record struct Draw(ushort Pointer, Shape Layout, int Frame, bool Restore)
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

    /// <summary>A lookup of a compiled draw list by its bank-$84 pointer.</summary>
    internal delegate bool DrawListLookup(ushort pointer, out DrawList list);

    /// <summary>Finds the draw list among <paramref name="candidates"/> whose published visual resource identity is <paramref name="id"/>.</summary>
    internal static bool FindByVisualId(IEnumerable<DrawList> candidates, Func<ushort, string> visualId, string? id, out DrawList found)
    {
        foreach (DrawList candidate in candidates)
        {
            if (string.Equals(visualId(candidate.Pointer), id, StringComparison.Ordinal))
            {
                found = candidate;
                return true;
            }
        }
        found = default;
        return false;
    }

    /// <summary>Compiles the draw list of the first of <paramref name="pointers"/> whose visual resource identity is <paramref name="id"/>.</summary>
    internal static bool FindByVisualId(IEnumerable<ushort> pointers, Func<ushort, string> visualId, DrawListLookup lookup, string? id, out DrawList found)
    {
        foreach (ushort pointer in pointers)
        {
            if (string.Equals(id, visualId(pointer), StringComparison.Ordinal))
                return lookup(pointer, out found);
        }
        found = default;
        return false;
    }

    /// <summary>Describes a compiled draw shape by its bank-$84 pointer.</summary>
    internal delegate bool ShapeLookup<TShape>(ushort pointer, out TShape shape);

    /// <summary>Builds the draw list of the run-shaped draw <paramref name="describe"/> finds for <paramref name="pointer"/>.</summary>
    internal static bool TryCompile<TShape>(ushort pointer, ShapeLookup<TShape> describe, out DrawList list) where TShape : IPlmRunDrawShape
    {
        list = default;
        if (!describe(pointer, out TShape shape)) return false;
        list = Compile(pointer, shape);
        return true;
    }

    /// <summary>Builds the draw list of the door cap <paramref name="describe"/> finds for <paramref name="pointer"/>.</summary>
    internal static bool TryCompileCap<TShape>(ushort pointer, ShapeLookup<TShape> describe, out DrawList list) where TShape : IPlmCapDrawShape
    {
        list = default;
        if (!describe(pointer, out TShape shape)) return false;
        list = CompileCap(pointer, shape);
        return true;
    }

    /// <summary>Builds the run-by-run draw list a compiled draw shape describes.</summary>
    private static DrawList Compile<TShape>(ushort pointer, TShape shape) where TShape : IPlmRunDrawShape
    {
        var runs = new Run[shape.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = shape.WordAt(run, cell);
            runs[run] = new((ushort)words.Length, words, new(shape.NextX(run), shape.NextY(run)));
        }
        return new(pointer, runs);
    }

    /// <summary>Builds the one four-word run of a door cap draw.</summary>
    private static DrawList CompileCap<TShape>(ushort pointer, TShape shape) where TShape : IPlmCapDrawShape
    {
        var words = new ushort[4];
        for (int cell = 0; cell < 4; cell++) words[cell] = shape.WordAt(cell);
        return new(pointer, new Run[] { new(shape.DirectionAndCountWord(), words, new(0, 0)) });
    }

    /// <summary>Recognizes a breakup frame or restoration pointer and returns its calculated draw.</summary>
    internal static bool TryGet(ushort pointer, out Draw draw)
    {
        if (TryFrames(pointer, SingleFrame0, 6, Shape.Single, out draw) ||
            TryFrames(pointer, HorizontalFrame0, 8, Shape.Horizontal, out draw) ||
            TryFrames(pointer, VerticalFrame0, 8, Shape.Vertical, out draw) ||
            TryFrames(pointer, SquareFrame0, 16, Shape.Square, out draw)) return true;
        var restore = (ShotBlockRestoreDraw)pointer;
        if (!Enum.IsDefined(restore))
        {
            draw = default;
            return false;
        }
        Shape layout = restore switch
        {
            ShotBlockRestoreDraw.Horizontal => Shape.Horizontal,
            ShotBlockRestoreDraw.Vertical => Shape.Vertical,
            ShotBlockRestoreDraw.Square => Shape.Square,
            _ => throw new InvalidOperationException($"Undefined ShotBlockRestoreDraw {restore}."),
        };
        draw = new(pointer, layout, 0, true);
        return true;
    }

    /// <summary>Checks whether a pointer is one of four fixed-stride breakup frames for a shape.</summary>
    private static bool TryFrames(ushort pointer, ushort first, int stride, Shape shape, out Draw draw)
    {
        int relative = pointer - first;
        if ((uint)relative < 4 * stride && relative % stride == 0)
        {
            draw = new(pointer, shape, relative / stride, false);
            return true;
        }
        draw = default;
        return false;
    }

    /// <summary>Yields breakup frames interleaved by shape, followed by the three restoration draws.</summary>
    internal static IEnumerable<Draw> Calculated
    {
        get
        {
            for (int frame = 0; frame < 4; frame++)
            {
                yield return Describe((ushort)(SingleFrame0 + frame * 6));
                yield return Describe((ushort)(HorizontalFrame0 + frame * 8));
                yield return Describe((ushort)(VerticalFrame0 + frame * 8));
                yield return Describe((ushort)(SquareFrame0 + frame * 16));
            }
            yield return Describe((ushort)ShotBlockRestoreDraw.Horizontal);
            yield return Describe((ushort)ShotBlockRestoreDraw.Vertical);
            yield return Describe((ushort)ShotBlockRestoreDraw.Square);
        }
    }

    /// <summary>Materializes draw-list DTOs for asset tooling; runtime uses calculated draws directly.</summary>
    internal static IEnumerable<DrawList> All => Calculated.Select(Export);

    /// <summary>Gets a calculated draw for an owned pointer and rejects unsupported native addresses.</summary>
    private static Draw Describe(ushort pointer)
    {
        if (!TryGet(pointer, out Draw draw)) throw new InvalidDataException($"Unknown shot-block draw ${pointer:X4}.");
        return draw;
    }

    /// <summary>Expands a calculated shot-block draw into ordered level-word runs and offsets.</summary>
    private static DrawList Export(Draw draw)
    {
        var runs = new Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordsPerRun];
            for (int block = 0; block < words.Length; block++) words[block] = draw.WordAt(run, block);
            runs[run] = new((ushort)((draw.Vertical ? 0x8000 : 0) | words.Length), words,
                new(0, (sbyte)(run + 1 < runs.Length ? 1 : 0)));
        }
        return new(draw.Pointer, runs);
    }
}
