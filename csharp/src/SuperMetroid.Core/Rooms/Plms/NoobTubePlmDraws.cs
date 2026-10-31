using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.NoobTubePlmDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>How each noob tube draw is described, found and named.</summary>
internal static class NoobTubePlmDraws
{
    /// <summary>Accepts only the seven known bank-$84 tube draw pointers.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        bool owned = request is PlmFrameDraw<NoobTubeDraw>;
        draw = owned ? new(PlmDraw.FrameOf<NoobTubeDraw>(request, "NoobTubeDraw")) : default;
        return owned;
    }

    /// <summary>Enumerates tube states in intact-to-cleared and broken presentation order.</summary>
    private static IEnumerable<NoobTubeDraw> Draws()
    {
        yield return NoobTubeDraw.Intact;
        yield return NoobTubeDraw.Damaged;
        yield return NoobTubeDraw.Opened;
        yield return NoobTubeDraw.Cleared;
        yield return NoobTubeDraw.BrokenLate;
        yield return NoobTubeDraw.OpenedRows;
        yield return NoobTubeDraw.BrokenFull;
    }

    /// <summary>Builds a temporary draw-list DTO from computed tube cells; gameplay reads the computed cells directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(request, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var blocks = new RoomTerrainBlock[draw.BlockCount(run)];
            for (int cell = 0; cell < blocks.Length; cell++) blocks[cell] = draw.BlockAt(run, cell);
            runs[run] = new(PlmRunDirection.Horizontal, blocks, new(0, draw.NextY(run)));
        }
        list = new(request, runs);
        return true;
    }

    /// <summary>Converts a validated native tube pointer to its artwork catalog ID.</summary>
    internal static string VisualId(PlmDraw request) =>
        VisualId(PlmDraw.FrameOf<NoobTubeDraw>(request, "n00b-tube draw with a visual ID"));

    /// <summary>Names the typed intact, damaged, opened, cleared, or broken presentation state.</summary>
    internal static string VisualId(NoobTubeDraw frame) => frame switch
    {
        NoobTubeDraw.Intact => "intact",
        NoobTubeDraw.Damaged => "damaged",
        NoobTubeDraw.Opened => "opened",
        NoobTubeDraw.Cleared => "cleared",
        NoobTubeDraw.BrokenLate => "broken-late",
        NoobTubeDraw.OpenedRows => "opened-rows",
        NoobTubeDraw.BrokenFull => "broken-full",
        _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {frame}."),
    };

    /// <summary>Finds and materializes the native tube layout corresponding to an artwork catalog ID.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (NoobTubeDraw frame in Draws())
            if (string.Equals(id, VisualId(frame), StringComparison.Ordinal)) return TryGet(PlmDraw.Of(frame), out list);
        list = default;
        return false;
    }

    /// <summary>Materializes each known tube layout for the artwork import and export interface.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (NoobTubeDraw frame in Draws())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
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
