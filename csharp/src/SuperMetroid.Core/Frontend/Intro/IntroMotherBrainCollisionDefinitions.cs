using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.DefinitionRun;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The terrain of the intro Mother Brain room (#2204): a full-width ceiling and floor, the left wall's contour between
/// them, the formation hanging from the middle of the ceiling, the pillar standing on the floor below it and the ledge
/// at the right wall, the pillar's and the ledge's tops being slopes. Rows are counted from the room's top.
/// </summary>
internal static partial class IntroMotherBrainCollisionDefinitions
{
    /// <summary>The room's width in blocks.</summary>
    internal const int Columns = 16;
    /// <summary>The room's height in blocks; the rows under the floor are open air.</summary>
    internal const int RoomRows = 16;
    /// <summary>A solid block.</summary>
    internal const RoomCollisionType Solid = RoomCollisionType.SolidBlock;
    /// <summary>A slope block: the pillar's and the ledge's tops.</summary>
    internal const RoomCollisionType PlatformTop = RoomCollisionType.Slope;
    /// <summary>An air block: every block the terrain leaves open.</summary>
    internal const RoomCollisionType Empty = RoomCollisionType.Air;
    /// <summary>The rows of the full-width ceiling and the full-width floor.</summary>
    internal const int CeilingRow = 1, FloorRow = 13;
    /// <summary>
    /// The formation hanging from the ceiling: one block at its tip on the row under the ceiling, three on the row below,
    /// centred on the column the pillar stands in.
    /// </summary>
    internal const int FormationTipRow = 2, CentralColumn = 9;
    /// <summary>The row of the pillar's and the ledge's tops, and the first column of the ledge, which reaches the right wall.</summary>
    internal const int PlatformRow = 8, RightPlatformColumn = 12;

    /// <summary>
    /// The left wall's width in blocks on each row from <see cref="FormationTipRow"/> down to the row above the floor: the
    /// tank, the notch above it and the pipe's base. They give the wall its blocking shape independently of the room's
    /// picture; a fitted curve would restate or alter it.
    /// </summary>
    internal static ImmutableArray<int> LeftContourWidths { get; } = [6, 6, 5, 6, .. Repeated(4, 3), 6, .. Repeated(7, 3)];
}
