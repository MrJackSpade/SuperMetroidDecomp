namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Complete native restoration words for linked Samus-contact crumble blocks.
/// The type-B parent and type-5/type-D children are physical room mutations;
/// artwork replacement may only alter their visual block composition.
/// </summary>
internal static class RoomPlmContactCrumbleRestoreDrawDefinitions
{
    /// <summary>Horizontal parent and child restoration at $84:A4A1.</summary>
    internal const ushort Horizontal = 0xa4a1;
    /// <summary>Vertical parent and child restoration at $84:A4A9.</summary>
    internal const ushort Vertical = 0xa4a9;
    /// <summary>Four-block parent and children restoration at $84:A4B1.</summary>
    internal const ushort Square = 0xa4b1;

    /// <summary>Calculated restoration layout for one linked crumble PLM family.</summary>
    /// <param name="Pointer">Native restoration-program pointer selecting this layout.</param>
    /// <param name="Vertical">Whether the two-block variant stacks its cells vertically.</param>
    /// <param name="Square">Whether the parent and children occupy a two-by-two block footprint.</param>
    internal readonly record struct Draw(ushort Pointer, bool Vertical, bool Square)
    {
        /// <summary>Gets the number of horizontal runs in this restoration footprint.</summary>
        internal int RunCount => Square ? 2 : 1;

        /// <summary>Calculates the packed restoration word at a run and block position.</summary>
        /// <param name="run">Zero-based horizontal run, bounded by <see cref="RunCount"/>.</param>
        /// <param name="block">Zero-based block within the run, from 0 through 1.</param>
        /// <returns>A collision-type and crumble-tile word for the selected cell.</returns>
        /// <exception cref="IndexOutOfRangeException">Either coordinate is outside this layout.</exception>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount || (uint)block >= 2)
                throw new IndexOutOfRangeException();
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            // Every cell shows the same crumble tile. Children link left on the
            // parent's row and up on later rows; only the top-left cell is type B.
            int collision = row == 0 && column == 0 ? 0xb000 : row == 0 ? 0x5000 : 0xd000;
            return (ushort)(collision | RoomPlmVisualBlockIndexes.ContactCrumbleParent);
        }
    }

    /// <summary>Resolves a native restoration pointer to its calculated linked-crumble layout.</summary>
    /// <param name="pointer">Native program pointer to classify.</param>
    /// <param name="draw">Receives the matching layout, or the default value if no layout is defined.</param>
    /// <returns><see langword="true"/> when <paramref name="pointer"/> selects a known layout.</returns>
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
    /// <summary>Exports each supported parent restoration layout in horizontal, vertical, then square order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            yield return Export(new(Horizontal, false, false));
            yield return Export(new(Vertical, true, false));
            yield return Export(new(Square, false, true));
        }
    }

    /// <summary>Converts a calculated layout to the legacy draw-list shape used by artwork tooling.</summary>
    /// <param name="draw">Linked-crumble layout to represent as ordered row runs.</param>
    /// <returns>A draw list carrying the native pointer and its packed cell words.</returns>
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
