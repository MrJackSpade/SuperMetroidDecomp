using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

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
    internal readonly record struct Draw(ChozoStatueDraw Frame) : IPlmRunDrawShape
    {
        /// <summary>Lower Norfair hand clearing is one air cell; Wrecked Ship slope access has five runs.</summary>
        private bool Hand => Frame == ChozoStatueDraw.LowerNorfairClearedHand;
        /// <summary>Distinguishes the single hand cell from the five-run Wrecked Ship slope layout.</summary>
        public int RunCount => Hand ? 1 : 5;
        /// <summary>Rejects run indexes outside the selected hand or slope geometry.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
        /// <summary>Uses a fourteen-cell ledge, nine-cell fill, two-cell transition, and two single-cell stair runs.</summary>
        public int WordCount(int run)
        {
            CheckRun(run);
            return Hand ? 1 : run switch { 0 => 14, 1 => 9, 2 => 2, _ => 1 };
        }
        /// <summary>Moves five cells right between interior slope-access runs.</summary>
        public sbyte NextX(int run)
        {
            CheckRun(run);
            return !Hand && run is > 0 and < 4 ? (sbyte)5 : (sbyte)0;
        }
        /// <summary>Offsets each slope-access run downward from the draw origin; the final run stays level.</summary>
        public sbyte NextY(int run)
        {
            CheckRun(run);
            return !Hand && run < 4 ? (sbyte)(run + 5) : (sbyte)0;
        }
        /// <summary>Combines shared visual tiles with air, slope-endpoint, spike, or solid collision words for the selected statue state.</summary>
        public ushort WordAt(int run, int cell)
        {
            int count = WordCount(run);
            if ((uint)cell >= (uint)count) throw new ArgumentOutOfRangeException(nameof(cell));
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
            int collision = Frame == ChozoStatueDraw.BlockSlopeAccess
                ? run == 0 && !endpoint ? 10 : 8
                : (run is 0 or 1 or 4) && endpoint ? 1 : 0;
            return (ushort)(collision << 12 | tile);
        }
    }

    /// <summary>Accepts the two Wrecked Ship slope-access draws and Lower Norfair's cleared hand.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        bool found = PlmDraw.TryFrame(request, out ChozoStatueDraw frame);
        draw = found ? new(frame) : default;
        return found;
    }
    /// <summary>Enumerates the three supported physical draw identities for artwork export.</summary>
    private static IEnumerable<PlmDraw> Draws()
    {
        yield return PlmDraw.Of(ChozoStatueDraw.LowerNorfairClearedHand);
        yield return PlmDraw.Of(ChozoStatueDraw.ClearSlopeAccess);
        yield return PlmDraw.Of(ChozoStatueDraw.BlockSlopeAccess);
    }
    /// <summary>Projects calculated statue cells into temporary artwork-interface draw records.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (PlmDraw draw in Draws())
            {
                TryGet(draw, out var list);
                yield return list;
            }
        }
    }
    /// <summary>Builds a temporary run record from calculated cells; runtime evaluates the physical cells directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.TryCompile<Draw>(request, TryDescribe, out list);
    /// <summary>Finds a compiled statue draw by its published visual resource identity.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>Names the cleared hand or blocked/clear slope layout without changing collision ownership.</summary>
    internal static string VisualId(PlmDraw request) =>
        PlmDraw.FrameOf<ChozoStatueDraw>(request, "Chozo statue draw with a visual ID") switch
        {
            ChozoStatueDraw.LowerNorfairClearedHand => "lower-norfair-cleared-hand",
            ChozoStatueDraw.ClearSlopeAccess => "wrecked-ship-clear-slope-access",
            ChozoStatueDraw.BlockSlopeAccess => "wrecked-ship-block-slope-access",
            _ => throw new InvalidOperationException($"Undefined {nameof(ChozoStatueDraw)} {request}."),
        };

}
