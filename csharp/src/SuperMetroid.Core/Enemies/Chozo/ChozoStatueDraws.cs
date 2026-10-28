using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Chozo statue PLMs' block draws: which PLM draws are the statues', the runs each lays from
/// <see cref="ChozoStatuePlmDrawDefinitions"/>, and their visual identities.
/// </summary>
internal static class ChozoStatueDraws
{
    /// <summary>A cleared hand or slope-access draw's runs.</summary>
    internal readonly record struct Draw(ChozoStatueDraw Frame) : IPlmRunDrawShape
    {
        /// <summary>The draw's runs in native order.</summary>
        private PlmBlockStrip[] Strips => ChozoStatuePlmDrawDefinitions.Strips[Frame];
        /// <summary>One run for the hand cell, five for the Wrecked Ship slope layout.</summary>
        public int RunCount => Strips.Length;
        /// <summary>The cells in one run, rejecting an invalid run index.</summary>
        public int WordCount(int run) => Strip(run).Cells.Length;
        /// <summary>The next run's horizontal offset.</summary>
        public sbyte NextX(int run) => checked((sbyte)Strip(run).Next.X);
        /// <summary>The next run's vertical offset.</summary>
        public sbyte NextY(int run) => checked((sbyte)Strip(run).Next.Y);
        /// <summary>One cell's complete level word.</summary>
        public ushort WordAt(int run, int cell)
        {
            PlmBlockStrip strip = Strip(run);
            if ((uint)cell >= (uint)strip.Cells.Length) throw new ArgumentOutOfRangeException(nameof(cell));
            return strip.Cells[cell].Raw;
        }
        /// <summary>Selects a run, rejecting indexes outside the selected hand or slope geometry.</summary>
        private PlmBlockStrip Strip(int run) =>
            (uint)run < (uint)RunCount ? Strips[run] : throw new ArgumentOutOfRangeException(nameof(run));
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
    /// <summary>Projects the statue cells into temporary artwork-interface draw records.</summary>
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
    /// <summary>Builds a temporary run record from the cells; runtime evaluates the physical cells directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.TryCompile<Draw>(request, TryDescribe, out list);
    /// <summary>Finds a compiled statue draw by its published visual resource identity.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>Names the cleared hand or blocked/clear slope layout without changing collision ownership.</summary>
    internal static string VisualId(PlmDraw request) =>
        ChozoStatuePlmDrawDefinitions.VisualIds.TryGetValue(
            PlmDraw.FrameOf<ChozoStatueDraw>(request, "Chozo statue draw with a visual ID"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined {nameof(ChozoStatueDraw)} {request}.");
}
