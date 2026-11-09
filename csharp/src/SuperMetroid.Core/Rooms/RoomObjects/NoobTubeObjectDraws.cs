using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>How each noob tube draw is described.</summary>
internal static class NoobTubeObjectDraws
{
    /// <summary>Accepts only the seven noob tube draws.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw)
    {
        bool owned = request is RoomObjectFrameDraw<NoobTubeDraw>;
        draw = owned ? new(RoomObjectDraw.FrameOf<NoobTubeDraw>(request, "NoobTubeDraw")) : default;
        return owned;
    }

    /// <summary>
    /// Seven named layouts. Wide runs are twelve cells: the rim and floor rows' pipe edges are solid around air, the
    /// floor with a second solid edge cell; the panel rows between are air. Each run starts at its own offset from the
    /// room-object origin; the list ends at its last run. What each frame shows is its installed picture (#1557).
    /// </summary>
    internal readonly record struct Draw(NoobTubeDraw Frame)
    {
        /// <summary>Single-block states use one run; the opened and broken panels span three or four rows.</summary>
        internal int RunCount => Frame switch
        {
            NoobTubeDraw.Cleared or NoobTubeDraw.BrokenFull => 4,
            NoobTubeDraw.BrokenLate or NoobTubeDraw.OpenedRows => 3,
            NoobTubeDraw.Intact or NoobTubeDraw.Damaged or NoobTubeDraw.Opened => 1,
            _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {Frame}."),
        };

        /// <summary>Rejects run indexes outside the selected tube layout before reading row geometry.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }

        /// <summary>Counts one origin block or a twelve-cell horizontal panel row for the selected run.</summary>
        internal int BlockCount(int run)
        {
            CheckRun(run);
            return Frame is NoobTubeDraw.Intact or NoobTubeDraw.Damaged or NoobTubeDraw.Opened ||
                (Frame is NoobTubeDraw.BrokenLate or NoobTubeDraw.BrokenFull && run == 0) ? 1 : 12;
        }

        /// <summary>Maps a run to its absolute vertical row, shifting late broken panels below the origin.</summary>
        private int Row(int run) => Frame switch
        {
            NoobTubeDraw.BrokenLate => run + 3,
            NoobTubeDraw.BrokenFull => run + 2,
            NoobTubeDraw.Intact or NoobTubeDraw.Damaged or NoobTubeDraw.Opened or
                NoobTubeDraw.Cleared or NoobTubeDraw.OpenedRows => run,
            _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {Frame}."),
        };

        /// <summary>Where a run starts, in blocks from the room-object origin: the first at the origin, each later one at its row.</summary>
        internal Offset16<RoomBlocks> Start(int run)
        {
            CheckRun(run);
            return new(0, run == 0 ? 0 : Row(run));
        }

        /// <summary>How one tube cell collides: the glass, the rim, a pipe edge, or air.</summary>
        internal RoomCollisionType CollisionAt(int run, int cell)
        {
            if ((uint)cell >= (uint)BlockCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            if (BlockCount(run) == 1)
                return Frame switch
                {
                    NoobTubeDraw.Intact => RoomCollisionType.ShootableBlock, // The projectile trigger.
                    NoobTubeDraw.Damaged => RoomCollisionType.SolidBlock, // Solid collision after activation.
                    NoobTubeDraw.Opened => RoomCollisionType.SolidBlock, // The solid upper rim at the origin.
                    NoobTubeDraw.BrokenLate or NoobTubeDraw.BrokenFull => RoomCollisionType.Air, // Broken frames clear collision at the origin.
                    NoobTubeDraw.Cleared or NoobTubeDraw.OpenedRows =>
                        throw new InvalidOperationException($"NoobTubeDraw {Frame} has no single-block run."),
                    _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {Frame}."),
                };
            int row = Row(run);
            int edgeDistance = Math.Min(cell, 11 - cell);
            return row is 0 or 5 && edgeDistance <= (row == 5 ? 1 : 0) ? RoomCollisionType.SolidBlock : RoomCollisionType.Air;
        }
    }
}
