using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.NoobTubePlmDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>How each noob tube draw is described.</summary>
internal static class NoobTubePlmDraws
{
    /// <summary>Accepts only the seven known tube draw pointers.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        bool owned = request is PlmFrameDraw<NoobTubeDraw>;
        draw = owned ? new(PlmDraw.FrameOf<NoobTubeDraw>(request, "NoobTubeDraw")) : default;
        return owned;
    }

    /// <summary>
    /// Seven named NTSC layouts at 84:98D1..9A3E. Wide runs are twelve cells:
    /// symmetric pipe edges surround air; the floor adds a second solid edge cell.
    /// Panel rows use adjacent tiles, vertically reflected below the centre.
    /// Continuations are absolute offsets from the PLM origin, not accumulated steps.
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

        /// <summary>Gives the next run's absolute vertical offset from the PLM origin.</summary>
        internal int NextY(int run)
        {
            CheckRun(run);
            return run == RunCount - 1 ? 0 : WordWrap.SignedLowByte(Row(run + 1));
        }

        /// <summary>The glass, rim, pipe-edge, or air block, with its collision and mirroring, of one tube cell.</summary>
        internal RoomTerrainBlock BlockAt(int run, int cell)
        {
            if ((uint)cell >= (uint)BlockCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            if (BlockCount(run) == 1)
                return Frame switch
                {
                    NoobTubeDraw.Intact => RoomTerrainBlock.Of(GlassBlock, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock), // Projectile trigger, mirrored intact glass tile.
                    NoobTubeDraw.Damaged => RoomTerrainBlock.Of(GlassBlock, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock), // Same glass tile, solid collision after activation.
                    NoobTubeDraw.Opened => RoomTerrainBlock.Of(RimBlock, BlockMirroring.None, RoomCollisionType.SolidBlock), // Solid upper rim at the origin.
                    NoobTubeDraw.BrokenLate or NoobTubeDraw.BrokenFull => RoomTerrainBlock.Of(RimBlock, BlockMirroring.None, RoomCollisionType.Air), // Broken frames clear collision at the origin.
                    NoobTubeDraw.Cleared or NoobTubeDraw.OpenedRows =>
                        throw new InvalidOperationException($"NoobTubeDraw {Frame} has no single-block run."),
                    _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {Frame}."),
                };
            int row = Row(run);
            int edgeDistance = Math.Min(cell, 11 - cell);
            if (edgeDistance > (row == 5 ? 1 : 0)) return RoomTerrainBlocks.BlankAir;
            int tile = row switch
            {
                0 => RimBlock,
                5 => FloorEdgeFirstBlock + edgeDistance,
                _ => PanelFirstBlock + Math.Min(row - 1, 4 - row),
            };
            BlockMirroring flips = (cell >= 6 ? BlockMirroring.Horizontal : BlockMirroring.None) |
                (row is 3 or 4 ? BlockMirroring.Vertical : BlockMirroring.None);
            return RoomTerrainBlock.Of(tile, flips, row is 0 or 5 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air);
        }
    }
}
