using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb Torizo's hand block draws: which room-object draws are the hand's, the strips each lays from
/// <see cref="BombTorizoHandObjectDrawDefinitions"/>.
/// </summary>
internal static class BombTorizoHandDraws
{
    /// <summary>
    /// An intact or cleared hand draw: its strips in the original run order, each starting at its own offset from the
    /// hand's block.
    /// </summary>
    internal readonly record struct Draw(BombTorizoHandDraw Frame)
    {
        /// <summary>The draw's strips in authored run order.</summary>
        private RoomObjectBlockStrip[] Strips => BombTorizoHandObjectDrawDefinitions.Strips[Frame];
        /// <summary>Gets the number of horizontal strips in the selected draw list.</summary>
        internal int RunCount => Strips.Length;
        /// <summary>Gets the block count in a selected strip, rejecting an invalid run index.</summary>
        internal int BlockCount(int run) => Strip(run).Cells.Length;
        /// <summary>Where a strip starts, in blocks from the hand's block.</summary>
        internal Offset16<RoomBlocks> Start(int run) => Strip(run).Start;
        /// <summary>How one strip cell collides.</summary>
        internal RoomCollisionType CollisionAt(int run, int cell)
        {
            RoomObjectBlockStrip strip = Strip(run);
            if ((uint)cell >= strip.Cells.Length) throw new ArgumentOutOfRangeException(nameof(cell));
            return strip.Cells[cell];
        }
        /// <summary>Selects a strip, rejecting indices outside the selected intact or cleared list.</summary>
        private RoomObjectBlockStrip Strip(int run) =>
            (uint)run < RunCount ? Strips[run] : throw new ArgumentOutOfRangeException(nameof(run));
    }

    /// <summary>Recognizes one of the two hand draws and returns its calculated descriptor.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw)
    {
        bool owned = request is RoomObjectFrameDraw<BombTorizoHandDraw>;
        draw = owned ? new(RoomObjectDraw.FrameOf<BombTorizoHandDraw>(request, "BombTorizoHandDraw")) : default;
        return owned;
    }
}
