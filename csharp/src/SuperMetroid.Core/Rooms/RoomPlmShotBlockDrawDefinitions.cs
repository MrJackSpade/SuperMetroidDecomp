namespace SuperMetroid.Core.Rooms;

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
    /// <summary>Final two-block horizontal collision-parent restoration, <c>$84:A47B</c>.</summary>
    internal const ushort RestoreHorizontal = 0xa47b;
    /// <summary>Final two-block vertical collision-parent restoration, <c>$84:A483</c>.</summary>
    internal const ushort RestoreVertical = 0xa483;
    /// <summary>Final four-block square collision-parent restoration, <c>$84:A48B</c>.</summary>
    internal const ushort RestoreSquare = 0xa48b;

    /// <summary>One native record: direction/count, complete level words, then the signed offset to the next record.</summary>
    /// <param name="DirectionAndCount">Native direction flag combined with the number of level words in this run.</param>
    /// <param name="LevelWords">Complete collision-and-visual words written by the run.</param>
    /// <param name="NextX">Signed horizontal block offset to the next run.</param>
    /// <param name="NextY">Signed vertical block offset to the next run.</param>
    internal readonly record struct Run(
        ushort DirectionAndCount,
        ReadOnlyMemory<ushort> LevelWords,
        sbyte NextX,
        sbyte NextY);

    /// <summary>Export form of one native draw routine, including its ordered level-word mutation runs.</summary>
    /// <param name="Pointer">Bank-$84 address of the draw routine.</param>
    /// <param name="Runs">Ordered mutations and signed offsets emitted by that routine.</param>
    internal readonly record struct DrawList(ushort Pointer, ReadOnlyMemory<Run> Runs);

    /// <summary>Number of breakup and collision-restoration draw routines represented by this catalog.</summary>
    internal const int DrawCount = 19;
    /// <summary>Block footprint used by a shot-block draw routine.</summary>
    internal enum Shape
    {
        /// <summary>One tile cell belonging to a single-block shot block.</summary>
        Single,
        /// <summary>Two adjacent horizontal tile cells.</summary>
        Horizontal,
        /// <summary>Two vertically adjacent tile cells.</summary>
        Vertical,
        /// <summary>Four tile cells arranged as a two-by-two block.</summary>
        Square
    }

    /// <summary>Bounded native draw: four breakup stages per shape at $84:A345-$A3DC,
    /// or parent/child restoration at $84:A47B-$A49A. Breakup uses consecutive
    /// visual blocks $53-$55 then air; restoration uses the shape's base tile,
    /// consecutive columns and a 32-index row stride, with native collision links.</summary>
    /// <param name="Pointer">Bank-$84 address selecting the native draw routine.</param>
    /// <param name="Layout">Footprint determining how many level words each run contains.</param>
    /// <param name="Frame">Zero-based breakup stage, or zero for a restoration routine.</param>
    /// <param name="Restore">Whether this draw reinstates the shootable parent and collision-linked children.</param>
    internal readonly record struct Draw(ushort Pointer, Shape Layout, int Frame, bool Restore)
    {
        /// <summary>Number of horizontal runs needed to cover this footprint.</summary>
        internal int RunCount => Layout == Shape.Square ? 2 : 1;
        /// <summary>Number of block words in each run: one for a single block and two for paired shapes.</summary>
        internal int WordsPerRun => Layout == Shape.Single ? 1 : 2;
        /// <summary>Whether successive runs advance vertically rather than horizontally.</summary>
        internal bool Vertical => Layout == Shape.Vertical;

        /// <summary>Calculates the complete native level word for one run/block position.</summary>
        /// <param name="run">Zero-based run within the footprint.</param>
        /// <param name="block">Zero-based block word within that run.</param>
        /// <exception cref="IndexOutOfRangeException">Either index lies outside the draw's footprint.</exception>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount || (uint)block >= WordsPerRun)
                throw new IndexOutOfRangeException();
            if (!Restore) return Frame == 3 ? (ushort)0x00ff : (ushort)(0x0053 + Frame);
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            int tile = Layout switch { Shape.Horizontal => 0x96, Shape.Vertical => 0x98, _ => 0x99 };
            // Parent is shootable; children link left on the first row and up below.
            int collision = row == 0 && column == 0 ? 0xc000 : row == 0 ? 0x5000 : 0xd000;
            return (ushort)(collision | (tile + 32 * row + column));
        }
    }

    /// <summary>Resolves a native breakup-frame or parent-restoration routine address to its calculated mutation.</summary>
    /// <param name="pointer">Bank-$84 draw-routine address.</param>
    /// <param name="draw">Calculated draw on success; the default value when the address is not in this catalog.</param>
    /// <returns>Whether the address identifies a supported shot-block draw routine.</returns>
    internal static bool TryGet(ushort pointer, out Draw draw)
    {
        if (TryFrames(pointer, SingleFrame0, 6, Shape.Single, out draw) ||
            TryFrames(pointer, HorizontalFrame0, 8, Shape.Horizontal, out draw) ||
            TryFrames(pointer, VerticalFrame0, 8, Shape.Vertical, out draw) ||
            TryFrames(pointer, SquareFrame0, 16, Shape.Square, out draw)) return true;
        switch (pointer)
        {
            case RestoreHorizontal: draw = new(pointer, Shape.Horizontal, 0, true); return true;
            case RestoreVertical: draw = new(pointer, Shape.Vertical, 0, true); return true;
            case RestoreSquare: draw = new(pointer, Shape.Square, 0, true); return true;
            default: draw = default; return false;
        }
    }

    /// <summary>Resolves one address in a regular four-frame breakup sequence.</summary>
    /// <param name="pointer">Address being queried.</param>
    /// <param name="first">Address of frame zero.</param>
    /// <param name="stride">Address distance between consecutive frames.</param>
    /// <param name="shape">Block footprint associated with this sequence.</param>
    /// <param name="draw">Calculated frame on success, or the default value otherwise.</param>
    /// <returns>Whether the address is an exact frame start in this sequence.</returns>
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

    // Preserve the published interleaved export order; materialize DTOs only for
    // asset tooling. Runtime uses the calculated draw directly.
    /// <summary>Enumerates all calculated mutations in the stable interleaved asset-export order.</summary>
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
            yield return Describe(RestoreHorizontal);
            yield return Describe(RestoreVertical);
            yield return Describe(RestoreSquare);
        }
    }

    /// <summary>Projects each calculated draw into the run-based representation consumed by asset tooling.</summary>
    internal static IEnumerable<DrawList> All => Calculated.Select(Export);

    /// <summary>Resolves a known catalog address or rejects an address outside the shot-block draw set.</summary>
    /// <param name="pointer">Bank-$84 draw-routine address to describe.</param>
    /// <returns>The calculated mutation for that address.</returns>
    /// <exception cref="InvalidDataException">The address is not one of the catalogued routines.</exception>
    private static Draw Describe(ushort pointer)
    {
        if (!TryGet(pointer, out Draw draw)) throw new InvalidDataException($"Unknown shot-block draw ${pointer:X4}.");
        return draw;
    }

    /// <summary>Materializes a calculated draw as ordered direction/count and level-word runs.</summary>
    /// <param name="draw">Calculated mutation to export.</param>
    /// <returns>The native draw address and its corresponding level-word runs.</returns>
    private static DrawList Export(Draw draw)
    {
        var runs = new Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordsPerRun];
            for (int block = 0; block < words.Length; block++) words[block] = draw.WordAt(run, block);
            runs[run] = new((ushort)((draw.Vertical ? 0x8000 : 0) | words.Length), words,
                0, (sbyte)(run + 1 < runs.Length ? 1 : 0));
        }
        return new(draw.Pointer, runs);
    }
}
