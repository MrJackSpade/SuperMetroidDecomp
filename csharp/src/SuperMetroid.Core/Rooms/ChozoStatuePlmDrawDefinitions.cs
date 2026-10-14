using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical bank-$84 draw layouts used by the translated Chozo statue PLMs.
/// The crumble plug shares the already-compiled shot-block breakup frames.
/// Complete level words, run geometry and signed offsets are cartridge mechanics;
/// installed artwork may replace only their visual block references.
/// </summary>
internal static class ChozoStatuePlmDrawDefinitions
{
    /// <summary>
    /// The two slope-access states share five horizontal runs and identical art.
    /// Clearing leaves slope endpoints; blocking makes the access solid, with
    /// spikes across the long row except its final cell. The hand is one air cell.
    /// </summary>
    internal readonly record struct Draw(ushort Pointer)
    {
        /// <summary>Lower Norfair hand clearing is one air cell; Wrecked Ship slope access has five runs.</summary>
        private bool Hand => Pointer == (ushort)ChozoStatueDraw.LowerNorfairClearedHand;
        /// <summary>Distinguishes the single hand cell from the five-run Wrecked Ship slope layout.</summary>
        internal int RunCount => Hand ? 1 : 5;
        /// <summary>Rejects run indexes outside the selected hand or slope geometry.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        /// <summary>Uses a fourteen-cell ledge, nine-cell fill, two-cell transition, and two single-cell stair runs.</summary>
        internal int WordCount(int run)
        {
            CheckRun(run);
            return Hand ? 1 : run switch { 0 => 14, 1 => 9, 2 => 2, _ => 1 };
        }
        /// <summary>Moves five cells right between interior slope-access runs.</summary>
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            return !Hand && run is > 0 and < 4 ? (sbyte)5 : (sbyte)0;
        }
        /// <summary>Offsets each slope-access run downward from the draw origin; the final run stays level.</summary>
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return !Hand && run < 4 ? (sbyte)(run + 5) : (sbyte)0;
        }
        /// <summary>Combines shared visual tiles with air, slope-endpoint, spike, or solid collision words for the selected statue state.</summary>
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
            int collision = Pointer == (ushort)ChozoStatueDraw.BlockSlopeAccess
                ? run == 0 && !endpoint ? 10 : 8
                : (run is 0 or 1 or 4) && endpoint ? 1 : 0;
            return (ushort)(collision << 12 | tile);
        }
    }

    /// <summary>Accepts the two Wrecked Ship slope-access draws and Lower Norfair's cleared hand.</summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        bool owned = Enum.IsDefined((ChozoStatueDraw)pointer);
        draw = owned ? new(pointer) : default;
        return owned;
    }
    /// <summary>Enumerates the three supported physical draw identities for artwork export.</summary>
    private static IEnumerable<ushort> Pointers()
    {
        yield return (ushort)ChozoStatueDraw.LowerNorfairClearedHand;
        yield return (ushort)ChozoStatueDraw.ClearSlopeAccess;
        yield return (ushort)ChozoStatueDraw.BlockSlopeAccess;
    }
    /// <summary>Projects calculated statue cells into temporary artwork-interface draw records.</summary>
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
    /// <summary>Builds a temporary run record from calculated cells; runtime evaluates the physical cells directly.</summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new((ushort)words.Length, words, new(draw.NextX(run), draw.NextY(run)));
        }
        list = new(pointer, runs);
        return true;
    }
    /// <summary>Finds a compiled statue draw by its published visual resource identity.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (ushort pointer in Pointers())
            if (string.Equals(id, VisualId(pointer), StringComparison.Ordinal)) return TryGet(pointer, out list);
        list = default;
        return false;
    }

    /// <summary>Names the cleared hand or blocked/clear slope layout without changing collision ownership.</summary>
    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<ChozoStatueDraw>(pointer, "Chozo statue draw with a visual ID") switch
        {
            ChozoStatueDraw.LowerNorfairClearedHand => "lower-norfair-cleared-hand",
            ChozoStatueDraw.ClearSlopeAccess => "wrecked-ship-clear-slope-access",
            ChozoStatueDraw.BlockSlopeAccess => "wrecked-ship-block-slope-access",
            _ => throw new InvalidOperationException($"Undefined {nameof(ChozoStatueDraw)} {pointer:X4}."),
        };

}
