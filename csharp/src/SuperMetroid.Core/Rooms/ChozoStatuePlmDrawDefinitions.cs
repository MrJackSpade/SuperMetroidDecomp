namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical bank-$84 draw layouts used by the translated Chozo statue PLMs.
/// The crumble plug shares the already-compiled shot-block breakup frames.
/// Complete level words, run geometry and signed offsets are cartridge mechanics;
/// installed artwork may replace only their visual block references.
/// </summary>
internal static class ChozoStatuePlmDrawDefinitions
{
    /// <summary>Blank Lower Norfair hand after the acid-lowered event, $84:A2B5.</summary>
    internal const ushort LowerNorfairClearedHand = 0xa2b5;
    /// <summary>Wrecked Ship statue's clear-slope-access draw, $84:9CC5.</summary>
    internal const ushort ClearSlopeAccess = 0x9cc5;
    /// <summary>Wrecked Ship statue's block-slope-access draw, $84:9D0F.</summary>
    internal const ushort BlockSlopeAccess = 0x9d0f;

    /// <summary>
    /// The two slope-access states share five horizontal runs and identical art.
    /// Clearing leaves slope endpoints; blocking makes the access solid, with
    /// spikes across the long row except its final cell. The hand is one air cell.
    /// </summary>
    /// <param name="Pointer">Native bank-$84 draw-list address selecting the cleared hand or one of the slope-access states.</param>
    internal readonly record struct Draw(ushort Pointer)
    {
        /// <summary>Whether this layout is the single-cell cleared hand rather than a slope-access draw.</summary>
        private bool Hand => Pointer == LowerNorfairClearedHand;

        /// <summary>Number of horizontal runs in this draw: one for the hand and five for either slope state.</summary>
        internal int RunCount => Hand ? 1 : 5;

        /// <summary>Validates a zero-based run index against this pointer's layout.</summary>
        /// <param name="run">Horizontal run position to check.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside this draw's runs.</exception>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        /// <summary>Returns the number of complete level words emitted by one horizontal run.</summary>
        /// <param name="run">Zero-based run position.</param>
        /// <returns>The run's cell count.</returns>
        internal int WordCount(int run)
        {
            CheckRun(run);
            return Hand ? 1 : run switch { 0 => 14, 1 => 9, 2 => 2, _ => 1 };
        }
        /// <summary>Returns the signed horizontal block offset to the next run.</summary>
        /// <param name="run">Zero-based run position.</param>
        /// <returns>The native X advance, or zero when the run has no horizontal offset.</returns>
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            return !Hand && run is > 0 and < 4 ? (sbyte)5 : (sbyte)0;
        }
        /// <summary>Returns the signed vertical block offset to the next run.</summary>
        /// <param name="run">Zero-based run position.</param>
        /// <returns>The native Y advance, or zero when the run has no vertical offset.</returns>
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return !Hand && run < 4 ? (sbyte)(run + 5) : (sbyte)0;
        }
        /// <summary>Calculates the complete collision-and-tile word for one cell in a run.</summary>
        /// <param name="run">Zero-based horizontal run position.</param>
        /// <param name="cell">Zero-based cell position within that run.</param>
        /// <returns>The level word, including its collision type and visual tile reference.</returns>
        /// <exception cref="IndexOutOfRangeException">The run or cell is outside this draw.</exception>
        internal ushort WordAt(int run, int cell)
        {
            int count = WordCount(run);
            if ((uint)cell >= (uint)count) throw new IndexOutOfRangeException();
            if (Hand) return 0x00ff;
            // Horizontal ledge, lower fill/transition, then the narrow stair wall.
            int tile = run switch
            {
                0 => 0x12b,
                1 => cell < 5 ? 0x111 : cell == 5 ? 0x19b : 0x129,
                2 => cell == 0 ? 0x1bb : 0x129,
                _ => 0x1bb,
            };
            bool endpoint = cell == count - 1;
            int collision = Pointer == BlockSlopeAccess
                ? run == 0 && !endpoint ? 10 : 8
                : (run is 0 or 1 or 4) && endpoint ? 1 : 0;
            return (ushort)(collision << 12 | tile);
        }
    }

    /// <summary>Identifies one of the three draw lists implemented by this catalog.</summary>
    /// <param name="pointer">Bank-relative native PLM draw-list address.</param>
    /// <param name="draw">Receives the calculated run layout when the address is recognized.</param>
    /// <returns><see langword="true"/> when the pointer selects a supported Chozo statue draw.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        bool owned = pointer is LowerNorfairClearedHand or ClearSlopeAccess or BlockSlopeAccess;
        draw = owned ? new(pointer) : default;
        return owned;
    }
    /// <summary>Enumerates the native addresses for the cleared hand and both slope-access layouts.</summary>
    private static IEnumerable<ushort> Pointers()
    {
        yield return LowerNorfairClearedHand;
        yield return ClearSlopeAccess;
        yield return BlockSlopeAccess;
    }
    /// <summary>Enumerates compiled draw lists for every native pointer owned by this catalog.</summary>
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
    // Temporary artwork DTOs; runtime cells are calculated directly.
    /// <summary>Builds the temporary draw-list representation for a supported native pointer.</summary>
    /// <param name="pointer">Bank-relative native PLM draw-list address.</param>
    /// <param name="list">Receives the calculated runs and complete level words when recognized.</param>
    /// <returns><see langword="true"/> when this catalog owns the pointer.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new((ushort)words.Length, words, draw.NextX(run), draw.NextY(run));
        }
        list = new(pointer, runs);
        return true;
    }
    /// <summary>Resolves a stable visual asset ID to its compiled draw list using ordinal comparison.</summary>
    /// <param name="id">Visual ID associated with one supported statue layout.</param>
    /// <param name="list">Receives the draw list when the ID matches.</param>
    /// <returns><see langword="true"/> when the ID identifies one of this catalog's layouts.</returns>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (ushort pointer in Pointers())
            if (string.Equals(id, VisualId(pointer), StringComparison.Ordinal)) return TryGet(pointer, out list);
        list = default;
        return false;
    }

    /// <summary>Returns the stable visual asset ID associated with a native draw-list address.</summary>
    /// <param name="pointer">Bank-relative native PLM draw-list address.</param>
    /// <returns>The catalog's asset ID for that layout.</returns>
    /// <exception cref="InvalidDataException">The pointer is not one of the catalog's supported layouts.</exception>
    internal static string VisualId(ushort pointer) => pointer switch
    {
        LowerNorfairClearedHand => "lower-norfair-cleared-hand",
        ClearSlopeAccess => "wrecked-ship-clear-slope-access",
        BlockSlopeAccess => "wrecked-ship-block-slope-access",
        _ => throw new InvalidDataException($"Chozo statue draw ${pointer:X4} has no visual ID."),
    };

}
