namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical bank-$84 draw lists selected by the n00b-tube PLM. The native
/// coroutine still owns power-bomb gating, shard release, room FX, the event
/// bit and Samus's input lock; these definitions only replace immutable draw
/// payload reads.
/// </summary>
internal static class NoobTubePlmDrawDefinitions
{
    /// <summary>Intact projectile-trigger block at $84:98D1.</summary>
    internal const ushort Intact = 0x98d1;
    /// <summary>First damaged origin block at $84:98D7.</summary>
    internal const ushort Damaged = 0x98d7;
    /// <summary>Opened origin block at $84:98DD.</summary>
    internal const ushort Opened = 0x98dd;
    /// <summary>Four-row cleared tube at $84:98E3.</summary>
    private const ushort Cleared = 0x98e3;
    /// <summary>Late broken-tube panel at $84:9953.</summary>
    private const ushort BrokenLate = 0x9953;
    /// <summary>Three-row opened tube at $84:9991.</summary>
    internal const ushort OpenedRows = 0x9991;
    /// <summary>Full broken-tube panel at $84:99E5.</summary>
    internal const ushort BrokenFull = 0x99e5;

    /// <summary>
    /// Seven named NTSC layouts at 84:98D1..9A3E. Wide runs are twelve cells:
    /// symmetric pipe edges surround air; the floor adds a second solid edge cell.
    /// Panel rows use adjacent tiles, vertically reflected below the centre.
    /// Continuations are absolute offsets from the PLM origin, not accumulated steps.
    /// </summary>
    /// <param name="Pointer">Bank-$84 identity selecting one of the seven compiled tube draw layouts.</param>
    internal readonly record struct Draw(ushort Pointer)
    {
        /// <summary>Number of native tilemap runs emitted by this layout.</summary>
        internal int RunCount => Pointer switch
        {
            Cleared or BrokenFull => 4,
            BrokenLate or OpenedRows => 3,
            _ => 1,
        };

        /// <summary>Validates a zero-based run index against this layout's run count.</summary>
        /// <param name="run">Run index to validate.</param>
        /// <exception cref="IndexOutOfRangeException">The index does not identify a run in this layout.</exception>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }

        /// <summary>Returns one origin word for single-block states or twelve cells for a tube row.</summary>
        /// <param name="run">Run whose horizontal word count is requested.</param>
        /// <returns>Number of tilemap words in the selected run.</returns>
        /// <exception cref="IndexOutOfRangeException">The run index is outside this layout.</exception>
        internal int WordCount(int run)
        {
            CheckRun(run);
            return Pointer is Intact or Damaged or Opened ||
                (Pointer is BrokenLate or BrokenFull && run == 0) ? 1 : 12;
        }

        /// <summary>Maps a run index to its authored vertical row, including the broken-panel row offsets.</summary>
        /// <param name="run">Zero-based run index.</param>
        /// <returns>Vertical row relative to the PLM origin.</returns>
        private int Row(int run) => Pointer switch
        {
            BrokenLate => run + 3,
            BrokenFull => run + 2,
            _ => run,
        };

        /// <summary>Returns the next run's vertical offset from the PLM origin, or zero after the final row.</summary>
        /// <param name="run">Current run index.</param>
        /// <returns>Native signed vertical continuation offset.</returns>
        /// <exception cref="IndexOutOfRangeException">The run index is outside this layout.</exception>
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return run == RunCount - 1 ? (sbyte)0 : (sbyte)Row(run + 1);
        }

        /// <summary>Calculates one SNES tilemap word from the selected state, row, and horizontal cell.</summary>
        /// <param name="run">Zero-based vertical run index.</param>
        /// <param name="cell">Zero-based cell within the run.</param>
        /// <returns>Tile, flip, and collision bits for the requested tube cell.</returns>
        /// <exception cref="IndexOutOfRangeException">The run or cell index is outside the layout.</exception>
        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new IndexOutOfRangeException();
            if (WordCount(run) == 1)
                return Pointer switch
                {
                    Intact => 0xc540, // Projectile trigger, mirrored intact glass tile.
                    Damaged => 0x8540, // Same glass tile, solid collision after activation.
                    Opened => 0x8141, // Solid upper rim at the origin.
                    _ => 0x0141, // Broken frames clear collision at the origin.
                };
            int row = Row(run);
            int edgeDistance = Math.Min(cell, 11 - cell);
            if (edgeDistance > (row == 5 ? 1 : 0)) return 0x00ff;
            int tile = row switch
            {
                0 => 0x141,
                5 => 0x14e + edgeDistance,
                _ => 0x322 + Math.Min(row - 1, 4 - row),
            };
            int flips = (cell >= 6 ? 0x400 : 0) | (row is 3 or 4 ? 0x800 : 0);
            int collision = row is 0 or 5 ? 0x8000 : 0;
            return (ushort)(collision | flips | tile);
        }
    }

    /// <summary>Recognizes a native tube draw-list pointer and returns its calculated layout.</summary>
    /// <param name="pointer">Bank-$84 draw-list identity to resolve.</param>
    /// <param name="draw">Receives the layout for a supported pointer, or the default value on failure.</param>
    /// <returns><see langword="true"/> when the pointer belongs to the compiled tube layouts.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        bool owned = pointer is Intact or Damaged or Opened or Cleared or BrokenLate or OpenedRows or BrokenFull;
        draw = owned ? new(pointer) : default;
        return owned;
    }

    /// <summary>Enumerates the seven exported draw lists in native pointer order.</summary>
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

    /// <summary>Yields each supported draw-list pointer in the stable native order used by exports.</summary>
    /// <returns>The intact, damaged, opened, and broken tube layout identities.</returns>
    private static IEnumerable<ushort> Pointers()
    {
        yield return Intact;
        yield return Damaged;
        yield return Opened;
        yield return Cleared;
        yield return BrokenLate;
        yield return OpenedRows;
        yield return BrokenFull;
    }

    // Temporary artwork import/export DTOs; gameplay calculates cells directly.
    /// <summary>Builds an export draw list from calculated words for a supported native pointer.</summary>
    /// <param name="pointer">Bank-$84 draw-list identity to export.</param>
    /// <param name="list">Receives the calculated list, or the default value when the pointer is unsupported.</param>
    /// <returns><see langword="true"/> when a compiled layout exists for the pointer.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new((ushort)words.Length, words, 0, draw.NextY(run));
        }
        list = new(pointer, runs);
        return true;
    }

    /// <summary>Maps a native tube draw-list pointer to its stable editable-artwork key.</summary>
    /// <param name="pointer">Supported bank-$84 draw-list identity.</param>
    /// <returns>The lowercase visual identifier used by presentation data.</returns>
    /// <exception cref="InvalidDataException">The pointer has no compiled visual identity.</exception>
    internal static string VisualId(ushort pointer) => pointer switch
    {
        Intact => "intact",
        Damaged => "damaged",
        Opened => "opened",
        Cleared => "cleared",
        BrokenLate => "broken-late",
        OpenedRows => "opened-rows",
        BrokenFull => "broken-full",
        _ => throw new InvalidDataException($"N00b-tube draw ${pointer:X4} has no visual ID."),
    };

    /// <summary>Resolves an editable-artwork key to its compiled export draw list using ordinal matching.</summary>
    /// <param name="id">Case-sensitive visual identifier such as <c>broken-late</c>.</param>
    /// <param name="list">Receives the corresponding draw list, or the default value when no key matches.</param>
    /// <returns><see langword="true"/> when the identifier names a supported tube layout.</returns>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (ushort pointer in Pointers())
            if (string.Equals(id, VisualId(pointer), StringComparison.Ordinal)) return TryGet(pointer, out list);
        list = default;
        return false;
    }
}
