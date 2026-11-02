using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb Torizo's hand block draws: which PLM draws are the hand's, the strips each lays from
/// <see cref="BombTorizoHandPlmDrawDefinitions"/>.
/// </summary>
internal static class BombTorizoHandDraws
{
    /// <summary>
    /// An intact or cleared hand draw. Original run order and origin-relative continuations are preserved.
    /// </summary>
    internal readonly record struct Draw(BombTorizoHandDraw Frame)
    {
        /// <summary>The draw's strips in authored run order.</summary>
        private BombTorizoHandStrip[] Strips => BombTorizoHandPlmDrawDefinitions.Strips[Frame];
        /// <summary>Gets the number of horizontal strips in the selected draw list.</summary>
        internal int RunCount => Strips.Length;
        /// <summary>Gets the block count in a selected strip, rejecting an invalid run index.</summary>
        internal int BlockCount(int run) => Strip(run).Cells.Length;
        /// <summary>Gets a strip's horizontal origin relative to the hand PLM block.</summary>
        internal int OriginX(int run) => Ensure.BetweenInclusive(sbyte.MinValue, sbyte.MaxValue, Strip(run).Origin.X);
        /// <summary>Gets a strip's vertical origin relative to the hand PLM block.</summary>
        internal int OriginY(int run) => Ensure.BetweenInclusive(sbyte.MinValue, sbyte.MaxValue, Strip(run).Origin.Y);
        /// <summary>Gets one strip cell's terrain block.</summary>
        internal RoomTerrainBlock BlockAt(int run, int cell)
        {
            BombTorizoHandStrip strip = Strip(run);
            if ((uint)cell >= strip.Cells.Length) throw new ArgumentOutOfRangeException(nameof(cell));
            return strip.Cells[cell];
        }
        /// <summary>Selects a strip, rejecting indices outside the selected intact or cleared list.</summary>
        private BombTorizoHandStrip Strip(int run) =>
            (uint)run < RunCount ? Strips[run] : throw new ArgumentOutOfRangeException(nameof(run));
    }

    /// <summary>Recognizes one of the two hand draw pointers and returns its calculated descriptor.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        bool owned = request is PlmFrameDraw<BombTorizoHandDraw>;
        draw = owned ? new(PlmDraw.FrameOf<BombTorizoHandDraw>(request, "BombTorizoHandDraw")) : default;
        return owned;
    }
}
