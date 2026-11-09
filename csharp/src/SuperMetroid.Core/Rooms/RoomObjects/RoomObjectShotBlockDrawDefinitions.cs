using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The geometry room-object draws write by: runs of cells, each with how it collides (#1557). What a cell shows is the
/// frame's installed picture; no draw here carries an appearance.
/// </summary>
internal static class RoomObjectShotBlockDrawDefinitions
{
    /// <summary>
    /// One run: its direction, how each of its cells collides and where it starts. A draw's runs are a list whose end is
    /// its last run (#1546).
    /// </summary>
    /// <param name="Direction">The direction the run writes its cells in.</param>
    /// <param name="Collisions">How each of the run's cells collides, in write order.</param>
    /// <param name="Start">Where the run's first cell lies, in blocks from the draw's origin; the first run starts at the origin.</param>
    internal readonly record struct Run(
        RoomObjectRunDirection Direction,
        ReadOnlyMemory<RoomCollisionType> Collisions,
        Offset16<RoomBlocks> Start)
    {
        /// <summary>The number of cells the run writes.</summary>
        internal int Count => Collisions.Length;

        /// <summary>Where the run's cell <paramref name="index"/> lies from the draw's origin.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not one of the run's cells.</exception>
        internal Offset16<RoomBlocks> CellAt(int index)
        {
            if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
            return Start + (Direction == RoomObjectRunDirection.Vertical
                ? new Offset16<RoomBlocks>(0, index)
                : new Offset16<RoomBlocks>(index, 0));
        }
    }

    /// <summary>One PLM draw with its ordered, immutable runs.</summary>
    internal readonly record struct DrawList(RoomObjectDraw Draw, ReadOnlyMemory<Run> Runs)
    {
    }

    /// <summary>Physical block arrangement affected by a shot-block draw list.</summary>
    internal enum Shape
    {
        /// <summary>One breakable block.</summary>
        Single,
        /// <summary>Two adjacent blocks in a row.</summary>
        Horizontal,
        /// <summary>Two stacked blocks in a column.</summary>
        Vertical,
        /// <summary>Four blocks arranged in two rows.</summary>
        Square,
    }

    /// <summary>The visual block of the first breakup frame; the next two frames step one block each.</summary>
    internal const int BreakupFirstBlock = 83;

    /// <summary>The visual blocks a restored shape's rows advance by.</summary>
    internal const int RestoreRowStride = 32;

    /// <summary>The visual block of each restored shape's parent.</summary>
    internal static readonly IReadOnlyDictionary<Shape, int> RestoreBlocks = new Dictionary<Shape, int>
    {
        [Shape.Horizontal] = 150,
        [Shape.Vertical] = 152,
        [Shape.Single] = 153,
        [Shape.Square] = 153,
    };

    /// <summary>Breakup frames per shape.</summary>
    internal const int BreakupFrameCount = 4;
}
