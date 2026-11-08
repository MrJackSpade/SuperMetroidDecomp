using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Chozo statue PLMs' block draws: which PLM draws are the statues', the runs each lays from
/// <see cref="ChozoStatueObjectDrawDefinitions"/>.
/// </summary>
internal static class ChozoStatueDraws
{
    /// <summary>A cleared hand or slope-access draw's runs.</summary>
    internal readonly record struct Draw(ChozoStatueDraw Frame)
    {
        /// <summary>The draw's runs in order.</summary>
        private RoomObjectBlockStrip[] Strips => ChozoStatueObjectDrawDefinitions.Strips[Frame];
        /// <summary>One run for the hand cell, five for the Wrecked Ship slope layout.</summary>
        internal int RunCount => Strips.Length;
        /// <summary>The cells in one run, rejecting an invalid run index.</summary>
        internal int BlockCount(int run) => Strip(run).Cells.Length;
        /// <summary>Where a run starts, in blocks from the draw's origin.</summary>
        internal Offset16<RoomBlocks> Start(int run) => Strip(run).Start;
        /// <summary>One cell's complete terrain block.</summary>
        internal RoomTerrainBlock BlockAt(int run, int cell)
        {
            RoomObjectBlockStrip strip = Strip(run);
            if ((uint)cell >= (uint)strip.Cells.Length) throw new ArgumentOutOfRangeException(nameof(cell));
            return strip.Cells[cell];
        }
        /// <summary>Selects a run, rejecting indexes outside the selected hand or slope geometry.</summary>
        private RoomObjectBlockStrip Strip(int run) =>
            (uint)run < (uint)RunCount ? Strips[run] : throw new ArgumentOutOfRangeException(nameof(run));
    }

    /// <summary>Accepts the two Wrecked Ship slope-access draws and Lower Norfair's cleared hand.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw)
    {
        bool found = RoomObjectDraw.TryFrame(request, out ChozoStatueDraw frame);
        draw = found ? new(frame) : default;
        return found;
    }
}
