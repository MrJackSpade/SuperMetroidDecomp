using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Immutable physical terrain of the SR388 discovery room. This 32-by-12 authored
/// region is distinct from the visible BG page; the final four 32-block rows of
/// the declared room are empty.
/// </summary>
internal static class IntroBabyDiscoveryCollisionDefinitions
{
    /// <summary>Thirty-two blocks per room row.</summary>
    internal const int Columns = 32;
    /// <summary>Twelve authored rows; the room itself is sixteen rows tall.</summary>
    internal const int SourceRows = 12;
    /// <summary>Ten empty rows precede the two authored terrain rows.</summary>
    internal const int ZeroRows = 10;
    /// <summary>The eleventh source row's block in every column: a slope block showing block zero.</summary>
    internal static readonly RoomTerrainBlock PenultimateRowBlock = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Slope);
    /// <summary>The final source row's block in every column: a solid block showing block zero.</summary>
    internal static readonly RoomTerrainBlock FinalRowBlock = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    /// <summary>Every other block of the room: air showing block zero.</summary>
    internal static readonly RoomTerrainBlock EmptyBlock = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air);

    /// <summary>Room height, including four empty rows beyond the authored region.</summary>
    internal const int RoomRows = 16;
}
