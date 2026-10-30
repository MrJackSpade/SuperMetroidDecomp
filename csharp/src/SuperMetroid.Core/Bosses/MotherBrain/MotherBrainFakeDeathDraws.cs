using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room block draws: the wall fill and escape door, the twelve mural rows and the fill rows
/// and tube clears, their run geometry and cells from <see cref="MotherBrainFakeDeathPlmDrawDefinitions"/>, and their
/// editable artwork identities.
/// </summary>
internal static class MotherBrainFakeDeathDraws
{
    /// <summary>Enumerates wall, door, mural rows and tube-clear draws in native family order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        BoundaryDraws().Concat(BackgroundDraws()).Concat(RegularDraws());

    /// <summary>Validates a native draw pointer before assigning its editable artwork ID.</summary>
    internal static string VisualId(PlmDraw request) => VisualId(
        PlmDraw.FrameOf<MotherBrainFakeDeathDraw>(request, "Mother Brain fake-death draw with a visual ID"));

    /// <summary>Maps the closed bank-$84 draw domain to stable visual document names.</summary>
    internal static string VisualId(MotherBrainFakeDeathDraw frame) =>
        MotherBrainFakeDeathPlmDrawDefinitions.VisualIds.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined MotherBrainFakeDeathDraw {frame}.");

    /// <summary>Finds a compiled physical draw by its case-sensitive editable visual ID.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        ShotBlockPlmDraws.FindByVisualId(All, VisualId, id, out draw);

    /// <summary>
    /// One of the two thirteen-cell solid fill rows or six tube clears. Tube interiors become blank; ceiling and floor
    /// endpoints are restored. Side tubes clear one extra cell toward the center using signed origin-relative X.
    /// </summary>
    internal readonly record struct RegularDraw(MotherBrainFakeDeathDraw Frame, int RunLength, int RunCount, MotherBrainFakeDeathClear Clear, int Side)
    {
        /// <summary>Whether the list clears a tube hanging from the ceiling.</summary>
        internal bool Ceiling => Clear == MotherBrainFakeDeathClear.CeilingTube;

        /// <summary>Whether the list writes a solid fill row.</summary>
        internal bool Fill => Clear == MotherBrainFakeDeathClear.FillRow;

        /// <summary>Uses one cell for a side-tube's second run and the full authored length elsewhere.</summary>
        internal int Count(int run)
        {
            if ((uint)run >= RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return Side != 0 && run == 1 ? 1 : RunLength;
        }
        /// <summary>Tube clears run vertically except for a side-tube's extra centerward cell.</summary>
        internal bool Vertical(int run) => !Fill && !(Side != 0 && run == 1);
        /// <summary>Moves to the next paired tube column, using signed direction for the side clears.</summary>
        internal sbyte NextX(int run)
        {
            Count(run);
            return run + 1 == RunCount ? (sbyte)0 : (sbyte)(Side == 0 ? 1 : Side);
        }
        /// <summary>The solid fill row's cell, or a tube interior cleared between its restored endpoints.</summary>
        internal RoomTerrainBlock BlockAt(int run, int block)
        {
            int count = Count(run);
            if ((uint)block >= count) throw new ArgumentOutOfRangeException(nameof(block));
            return MotherBrainFakeDeathPlmDrawDefinitions.Cells[Frame][run][block];
        }
    }

    /// <summary>Recognizes only the two unused fill rows and six tube-clear lists in the shared draw-pointer space.</summary>
    internal static bool TryDescribeRegular(PlmDraw request, out RegularDraw draw)
    {
        draw = default;
        // Shared draw-pointer space: draws outside this family belong to other owners.
        if (request is not PlmFrameDraw<MotherBrainFakeDeathDraw>) return false;
        var list = PlmDraw.FrameOf<MotherBrainFakeDeathDraw>(request, "MotherBrainFakeDeathDraw");
        if (MotherBrainFakeDeathPlmDrawDefinitions.RegularShapes.TryGetValue(list, out draw))
            return true;
        // The boundary and background families own the other lists.
        if (Enum.IsDefined(list))
            return false;
        throw new InvalidOperationException($"Undefined MotherBrainFakeDeathDraw {list}.");
    }

    /// <summary>Expands a regular shape into ordered PLM runs with native vertical flags and continuation offsets.</summary>
    internal static bool TryGetRegular(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribeRegular(request, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[shape.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var blocks = new RoomTerrainBlock[shape.Count(run)];
            for (int block = 0; block < blocks.Length; block++) blocks[block] = shape.BlockAt(run, block);
            runs[run] = new(shape.Vertical(run) ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal, blocks, new(shape.NextX(run), 0));
        }
        draw = new(request, runs);
        return true;
    }

    /// <summary>The two unused fill rows and six tube clears, in the order the room's records list them.</summary>
    private static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> RegularDraws()
    {
        foreach (MotherBrainFakeDeathDraw frame in Enum.GetValues<MotherBrainFakeDeathDraw>())
        {
            if (TryGetRegular(PlmDraw.Of(frame), out var draw))
                yield return draw;
        }
    }

    /// <summary>
    /// Twelve horizontal thirteen-cell rows at 84:9505..966C, spaced thirty
    /// bytes apart. Top/bottom rows are slopes; interior rows are air.
    /// Appearances remain independent from these physical boundaries.
    /// </summary>
    internal readonly record struct BackgroundDraw([property: IntegerQuantity("tile row within the background draw")] int Row)
    {
        /// <summary>Combines a selected mural tile with solid collision only on its upper and lower border rows.</summary>
        internal RoomTerrainBlock BlockAt(int column)
        {
            if ((uint)column >= 13) throw new ArgumentOutOfRangeException(nameof(column));
            RoomTerrainBlock visual = MotherBrainFakeDeathPlmDrawDefinitions.BackgroundVisuals[Row][column];
            return Row is 0 or 11 ? visual.WithCollision(RoomCollisionType.Slope) : visual;
        }
    }

    /// <summary>Recognizes the twelve mural row draws.</summary>
    internal static bool TryDescribeBackground(PlmDraw request, out BackgroundDraw draw)
    {
        int row = request is PlmFrameDraw<MotherBrainFakeDeathDraw> { Frame: var frame }
            ? Array.IndexOf(MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRows, frame)
            : -1;
        draw = row >= 0 ? new(row) : default;
        return row >= 0;
    }

    /// <summary>Builds one thirteen-cell horizontal mural run from its selected row and collision boundary.</summary>
    internal static bool TryGetBackground(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribeBackground(request, out var row)) return false;
        var blocks = new RoomTerrainBlock[13];
        for (int column = 0; column < blocks.Length; column++) blocks[column] = row.BlockAt(column);
        draw = new(request, new RoomPlmShotBlockDrawDefinitions.Run[] { new(PlmRunDirection.Horizontal, blocks, new(0, 0)) });
        return true;
    }

    /// <summary>Enumerates all twelve mural rows, top to bottom.</summary>
    private static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> BackgroundDraws()
    {
        foreach (MotherBrainFakeDeathDraw row in MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRows)
        {
            TryGetBackground(PlmDraw.Of(row), out var draw);
            yield return draw;
        }
    }

    /// <summary>
    /// 84:94A3/94B1: a three-cell wall centered on the origin, or a two-column
    /// four-cell door. Both use vertical runs.
    /// </summary>
    internal readonly record struct BoundaryDraw(MotherBrainFakeDeathDraw Frame)
    {
        /// <summary>Chooses four cells per escape-door column or the wall's two-cell and one-cell runs.</summary>
        internal int Count(int run)
        {
            if ((uint)run >= 2) throw new ArgumentOutOfRangeException(nameof(run));
            return MotherBrainFakeDeathPlmDrawDefinitions.Cells[Frame][run].Length;
        }
        /// <summary>Steps from the escape door's left column to its right air column.</summary>
        internal sbyte NextX(int run)
        {
            Count(run);
            return Frame == MotherBrainFakeDeathDraw.EscapeDoor && run == 0 ? (sbyte)1 : (sbyte)0;
        }
        /// <summary>Raises the second wall cap above its first run's centered tile.</summary>
        internal sbyte NextY(int run)
        {
            Count(run);
            return Frame == MotherBrainFakeDeathDraw.FillWall && run == 0 ? (sbyte)-1 : (sbyte)0;
        }
        /// <summary>The solid wall caps or the escape door's parent, extensions and air-backed artwork.</summary>
        internal RoomTerrainBlock BlockAt(int run, int block)
        {
            if ((uint)block >= Count(run)) throw new ArgumentOutOfRangeException(nameof(block));
            return MotherBrainFakeDeathPlmDrawDefinitions.Cells[Frame][run][block];
        }
    }

    /// <summary>Recognizes only the wall-fill and escape-door pointers among the shared fake-death draws.</summary>
    internal static bool TryDescribeBoundary(PlmDraw request, out BoundaryDraw draw)
    {
        draw = default;
        // Shared draw-pointer space: draws outside this family belong to other owners.
        if (request is not PlmFrameDraw<MotherBrainFakeDeathDraw>) return false;
        var list = PlmDraw.FrameOf<MotherBrainFakeDeathDraw>(request, "MotherBrainFakeDeathDraw");
        bool owned = list is MotherBrainFakeDeathDraw.FillWall or MotherBrainFakeDeathDraw.EscapeDoor;
        if (owned) draw = new(list);
        return owned;
    }

    /// <summary>Expands a validated wall or escape-door shape into two vertical PLM runs.</summary>
    internal static bool TryGetBoundary(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribeBoundary(request, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[2];
        for (int run = 0; run < runs.Length; run++)
        {
            var blocks = new RoomTerrainBlock[shape.Count(run)];
            for (int block = 0; block < blocks.Length; block++) blocks[block] = shape.BlockAt(run, block);
            runs[run] = new(PlmRunDirection.Vertical, blocks, new(shape.NextX(run), shape.NextY(run)));
        }
        draw = new(request, runs);
        return true;
    }

    /// <summary>Yields the wall-fill then escape-door draws.</summary>
    private static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> BoundaryDraws()
    {
        TryGetBoundary(PlmDraw.Of(MotherBrainFakeDeathDraw.FillWall), out var wall);
        yield return wall;
        TryGetBoundary(PlmDraw.Of(MotherBrainFakeDeathDraw.EscapeDoor), out var door);
        yield return door;
    }
}
