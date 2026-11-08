using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The terrain the ordinary shot-block PLMs draw. Each block carries both its collision kind and its appearance;
/// keeping the whole terrain here prevents artwork replacement from changing terrain rules.
/// </summary>
internal static class RoomObjectShotBlockDrawDefinitions
{
    /// <summary>
    /// One run: its direction, the terrain it writes and where it starts. A draw's runs are a list whose end is its last
    /// run (#1546).
    /// </summary>
    /// <param name="Direction">The direction the run writes its blocks in.</param>
    /// <param name="Blocks">The terrain the run writes, one per block.</param>
    /// <param name="Start">Where the run's first block lies, in blocks from the draw's origin; the first run starts at the origin.</param>
    internal readonly record struct Run(
        RoomObjectRunDirection Direction,
        ReadOnlyMemory<RoomTerrainBlock> Blocks,
        Offset16<RoomBlocks> Start)
    {
        /// <summary>The number of blocks the run writes.</summary>
        internal int Count => Blocks.Length;
    }

    /// <summary>One PLM draw with its ordered, immutable terrain runs.</summary>
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
