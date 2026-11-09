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
    }

    /// <summary>One room-object draw with its ordered, immutable runs.</summary>
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

    /// <summary>Breakup frames per shape.</summary>
    internal const int BreakupFrameCount = 4;
}
