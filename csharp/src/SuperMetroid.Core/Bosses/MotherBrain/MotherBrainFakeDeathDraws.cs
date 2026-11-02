using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room block draws: the wall fill and escape door, the twelve mural rows and the fill rows
/// and tube clears, and their run geometry and cells from <see cref="MotherBrainFakeDeathPlmDrawDefinitions"/>.
/// </summary>
internal static class MotherBrainFakeDeathDraws
{
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
        internal int NextX(int run)
        {
            Count(run);
            return run + 1 == RunCount ? 0 : WordWrap.SignedLowByte(Side == 0 ? 1 : Side);
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
        internal int NextX(int run)
        {
            Count(run);
            return Frame == MotherBrainFakeDeathDraw.EscapeDoor && run == 0 ? 1 : 0;
        }
        /// <summary>Raises the second wall cap above its first run's centered tile.</summary>
        internal int NextY(int run)
        {
            Count(run);
            return Frame == MotherBrainFakeDeathDraw.FillWall && run == 0 ? -1 : 0;
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
}
