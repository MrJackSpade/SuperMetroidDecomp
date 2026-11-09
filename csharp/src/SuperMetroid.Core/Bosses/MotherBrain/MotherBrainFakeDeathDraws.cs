using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room block draws: the wall fill and escape door, the twelve mural rows and the fill rows
/// and tube clears, and their run geometry and cells from <see cref="MotherBrainFakeDeathObjectDrawDefinitions"/>.
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
        /// <summary>
        /// Where a run starts, in blocks from the draw's origin: a later run starts in the paired tube column beside the
        /// origin, toward the centre for a side clear.
        /// </summary>
        internal Offset16<RoomBlocks> Start(int run)
        {
            Count(run);
            return new(run == 0 ? 0 : Side == 0 ? 1 : Side, 0);
        }
        /// <summary>How the cell collides: a solid fill row's cell, or a tube interior cleared between its restored endpoints.</summary>
        internal RoomCollisionType CollisionAt(int run, int block)
        {
            int count = Count(run);
            if ((uint)block >= count) throw new ArgumentOutOfRangeException(nameof(block));
            return MotherBrainFakeDeathObjectDrawDefinitions.Collisions[Frame][run][block];
        }
    }

    /// <summary>Recognizes only the two unused fill rows and six tube-clear lists in the shared draw-pointer space.</summary>
    internal static bool TryDescribeRegular(RoomObjectDraw request, out RegularDraw draw)
    {
        draw = default;
        // Shared draw-pointer space: draws outside this family belong to other owners.
        if (request is not RoomObjectFrameDraw<MotherBrainFakeDeathDraw>) return false;
        var list = RoomObjectDraw.FrameOf<MotherBrainFakeDeathDraw>(request, "MotherBrainFakeDeathDraw");
        if (MotherBrainFakeDeathObjectDrawDefinitions.RegularShapes.TryGetValue(list, out draw))
            return true;
        // The boundary and background families own the other lists.
        if (Enum.IsDefined(list))
            return false;
        throw new InvalidOperationException($"Undefined MotherBrainFakeDeathDraw {list}.");
    }

    /// <summary>
    /// Twelve horizontal thirteen-cell rows of the fake-death background draw, spaced thirty
    /// bytes apart. Top/bottom rows are slopes; interior rows are air.
    /// What each row shows is its installed picture (#1557).
    /// </summary>
    internal readonly record struct BackgroundDraw([property: IntegerQuantity("tile row within the background draw")] int Row)
    {
        /// <summary>How a mural cell collides: as a slope on the upper and lower border rows, as air between them.</summary>
        internal RoomCollisionType CollisionAt(int column)
        {
            if ((uint)column >= MotherBrainFakeDeathObjectDrawDefinitions.BackgroundWidth) throw new ArgumentOutOfRangeException(nameof(column));
            return Row is 0 or 11 ? RoomCollisionType.Slope : RoomCollisionType.Air;
        }
    }

    /// <summary>Recognizes the twelve mural row draws.</summary>
    internal static bool TryDescribeBackground(RoomObjectDraw request, out BackgroundDraw draw)
    {
        int row = request is RoomObjectFrameDraw<MotherBrainFakeDeathDraw> { Frame: var frame }
            ? Array.IndexOf(MotherBrainFakeDeathObjectDrawDefinitions.BackgroundRows, frame)
            : -1;
        draw = row >= 0 ? new(row) : default;
        return row >= 0;
    }

    /// <summary>
    /// The wall-fill and escape-door draws: a three-cell wall centered on the origin, or a two-column
    /// four-cell door. Both use vertical runs.
    /// </summary>
    internal readonly record struct BoundaryDraw(MotherBrainFakeDeathDraw Frame)
    {
        /// <summary>Chooses four cells per escape-door column or the wall's two-cell and one-cell runs.</summary>
        internal int Count(int run)
        {
            if ((uint)run >= RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return MotherBrainFakeDeathObjectDrawDefinitions.Collisions[Frame][run].Length;
        }
        /// <summary>
        /// Where a run starts, in blocks from the draw's origin: the escape door's right air column one block right of
        /// its left column, and the wall's second cap one block above its first run's centred tile.
        /// </summary>
        internal Offset16<RoomBlocks> Start(int run)
        {
            Count(run);
            if (run == 0) return Offset16<RoomBlocks>.Zero;
            return Frame == MotherBrainFakeDeathDraw.EscapeDoor ? new Offset16<RoomBlocks>(1, 0)
                : Frame == MotherBrainFakeDeathDraw.FillWall ? new Offset16<RoomBlocks>(0, -1)
                : throw new InvalidOperationException($"MotherBrainFakeDeathDraw {Frame} is not a boundary draw.");
        }

        /// <summary>The runs a wall-fill or escape-door draw writes.</summary>
        internal const int RunCount = 2;
        /// <summary>How the cell collides: the solid wall, or the escape door's parent, extensions and air.</summary>
        internal RoomCollisionType CollisionAt(int run, int block)
        {
            if ((uint)block >= Count(run)) throw new ArgumentOutOfRangeException(nameof(block));
            return MotherBrainFakeDeathObjectDrawDefinitions.Collisions[Frame][run][block];
        }
    }

    /// <summary>Recognizes only the wall-fill and escape-door pointers among the shared fake-death draws.</summary>
    internal static bool TryDescribeBoundary(RoomObjectDraw request, out BoundaryDraw draw)
    {
        draw = default;
        // Shared draw-pointer space: draws outside this family belong to other owners.
        if (request is not RoomObjectFrameDraw<MotherBrainFakeDeathDraw>) return false;
        var list = RoomObjectDraw.FrameOf<MotherBrainFakeDeathDraw>(request, "MotherBrainFakeDeathDraw");
        bool owned = list is MotherBrainFakeDeathDraw.FillWall or MotherBrainFakeDeathDraw.EscapeDoor;
        if (owned) draw = new(list);
        return owned;
    }
}
