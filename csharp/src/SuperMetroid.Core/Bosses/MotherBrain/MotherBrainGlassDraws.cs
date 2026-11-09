using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's glass block draws: which room-object draws are the glass's, their run geometry and how their cells collide,
/// from <see cref="MotherBrainGlassObjectDrawDefinitions"/>. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class MotherBrainGlassDraws
{
    /// <summary>
    /// One of eleven named glass states. Offsets are signed and absolute from the glass's origin block; panes draw down their
    /// columns, and the shifted pane writes its origin block separately before its inner columns.
    /// </summary>
    internal readonly record struct Draw(MotherBrainGlassDraw Frame)
    {
        /// <summary>Whether this frame is one of the two symmetric full-pane damage layouts.</summary>
        private bool Pane => Frame is MotherBrainGlassDraw.PaneDamage1 or MotherBrainGlassDraw.PaneDamage2;
        /// <summary>Whether the pane has shifted and uses separate inner-column runs.</summary>
        private bool Shifted => Frame is MotherBrainGlassDraw.ShiftedPane1 or MotherBrainGlassDraw.ShiftedPane2 or MotherBrainGlassDraw.ShiftedPane3;
        /// <summary>Draw-record count for the selected glass damage phase.</summary>
        internal int RunCount => MotherBrainGlassObjectDrawDefinitions.Collisions[Frame].Length;
        /// <summary>Rejects a run outside this frame's draw-record count.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
        /// <summary>Returns the number of blocks in one pane or shatter draw run.</summary>
        internal int BlockCount(int run)
        {
            CheckRun(run);
            return MotherBrainGlassObjectDrawDefinitions.Collisions[Frame][run].Length;
        }
        /// <summary>Selects vertical draw traversal except for the initial and shifted origin blocks.</summary>
        internal bool Vertical(int run)
        {
            CheckRun(run);
            return Frame != MotherBrainGlassDraw.Initial && !(Shifted && run == 0);
        }
        /// <summary>
        /// Where a run starts, in blocks from the glass's origin; the first run starts at the origin. A pane's later
        /// columns start one block left and one down; every other layout's later runs start four blocks left of their run
        /// number, one row down for a shifted pane's runs (except the first shifted pane's second run) and for the first
        /// shatter's runs from the third on.
        /// </summary>
        internal Offset16<RoomBlocks> Start(int run)
        {
            CheckRun(run);
            if (run == 0) return Offset16<RoomBlocks>.Zero;
            bool lowered = Pane || Shifted && (run != 1 || Frame != MotherBrainGlassDraw.ShiftedPane1) ||
                Frame == MotherBrainGlassDraw.Shatter1 && run >= 2;
            return new(Pane ? -1 : run - 4, lowered ? 1 : 0);
        }
        /// <summary>How the glass cell collides.</summary>
        internal RoomCollisionType CollisionAt(int run, int cell)
        {
            if ((uint)cell >= (uint)BlockCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            return MotherBrainGlassObjectDrawDefinitions.Collisions[Frame][run][cell];
        }
    }

    /// <summary>Recognizes one of the eleven glass draws without claiming adjacent room-object families.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw)
    {
        // Draws outside the eleven glass states belong to other draw families.
        bool owned = request is RoomObjectFrameDraw<MotherBrainGlassDraw>;
        draw = owned ? new(RoomObjectDraw.FrameOf<MotherBrainGlassDraw>(request, "MotherBrainGlassDraw")) : default;
        return owned;
    }
}
