using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The cartridge's form of a room-object frame (#1557, #1862): runs of cells, each with how it collides, written in the
/// cartridge's order. The import checks and pictures frames by it; the game draws each frame's block shape and never these
/// runs. What a cell shows is the frame's installed picture.
/// </summary>
internal static class RoomObjectDrawLists
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
}
