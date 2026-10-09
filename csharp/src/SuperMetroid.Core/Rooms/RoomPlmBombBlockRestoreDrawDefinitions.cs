namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Complete native level-word restoration lists for linked bomb blocks. Their
/// shape shares the compiled PLM row/column representation with shot blocks,
/// while the distinct type-F parent and type-5/type-D children stay immutable.
/// </summary>
internal static class RoomPlmBombBlockRestoreDrawDefinitions
{
    /// <summary>Horizontal parent and child restoration at $84:A4C7.</summary>
    internal const ushort Horizontal = 0xa4c7;
    /// <summary>Vertical parent and child restoration at $84:A4CF.</summary>
    internal const ushort Vertical = 0xa4cf;
    /// <summary>Four-block parent and child restoration at $84:A4D7.</summary>
    internal const ushort Square = 0xa4d7;

    /// <summary>Describes one linked bomb-block restoration list independently of its exported draw runs.</summary>
    /// <param name="Pointer">ROM list pointer identifying the horizontal, vertical, or square restoration layout.</param>
    /// <param name="Vertical">Whether each run represents a successive row rather than a horizontal pair.</param>
    /// <param name="Square">Whether the layout contains two runs, forming a four-block square.</param>
    internal readonly record struct Draw(ushort Pointer, bool Vertical, bool Square)
    {
        /// <summary>Number of two-block runs required by this layout: two for a square and one otherwise.</summary>
        internal int RunCount => Square ? 2 : 1;

        /// <summary>Builds the native level word for one cell in a restoration run.</summary>
        /// <param name="run">Zero-based run index, limited by <see cref="RunCount"/>.</param>
        /// <param name="block">Zero-based cell index within the run; each run has two cells.</param>
        /// <returns>The collision type and bomb-parent visual index encoded as one level word.</returns>
        /// <exception cref="IndexOutOfRangeException">The run or block index is outside its supported range.</exception>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount || (uint)block >= 2)
                throw new IndexOutOfRangeException();
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            // Every cell shows the same bomb tile. Children link left on the
            // parent's row and up on later rows; only the top-left cell is type F.
            int collision = row == 0 && column == 0 ? 0xf000 : row == 0 ? 0x5000 : 0xd000;
            return (ushort)(collision | RoomPlmVisualBlockIndexes.CollisionBombParent);
        }
    }

    /// <summary>Looks up a supported native restoration-list pointer and returns its layout descriptor.</summary>
    /// <param name="pointer">ROM list pointer to classify.</param>
    /// <param name="draw">Receives the matching layout, or the default descriptor when no pointer matches.</param>
    /// <returns><see langword="true"/> when <paramref name="pointer"/> identifies a known restoration layout.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        switch (pointer)
        {
            case Horizontal: draw = new(pointer, false, false); return true;
            case Vertical: draw = new(pointer, true, false); return true;
            case Square: draw = new(pointer, false, true); return true;
            default: draw = default; return false;
        }
    }

    // Temporary draw DTOs support existing artwork import/export consumers.
    // Runtime draws the calculated descriptor directly; no generated cache remains.
    /// <summary>Exports all three supported layouts for consumers that still require the shot-block draw-list shape.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            yield return Export(new(Horizontal, false, false));
            yield return Export(new(Vertical, true, false));
            yield return Export(new(Square, false, true));
        }
    }

    /// <summary>Converts a compact restoration descriptor into the legacy draw-list representation used by import/export tools.</summary>
    /// <param name="draw">Layout descriptor whose rows and linked bomb-block words are emitted.</param>
    /// <returns>A draw list containing the cells and row links for the selected layout.</returns>
    private static RoomPlmShotBlockDrawDefinitions.DrawList Export(Draw draw)
    {
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
            runs[run] = new(draw.Vertical ? (ushort)0x8002 : (ushort)2,
                new ushort[] {draw.WordAt(run, 0), draw.WordAt(run, 1)},
                0, run + 1 < runs.Length ? (sbyte)1 : (sbyte)0);
        return new(draw.Pointer, runs);
    }
}
