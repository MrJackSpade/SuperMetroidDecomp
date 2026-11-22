using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The terrain of the SR388 discovery room, distinct from the room's picture (#2204): open air down to a full-width row
/// of slope blocks lying on a full-width solid floor, with open air below the floor to the room's bottom.
/// </summary>
internal static partial class IntroBabyDiscoveryCollisionDefinitions
{
    /// <summary>The room's width in blocks.</summary>
    internal const int Columns = 32;
    /// <summary>The room's height in blocks.</summary>
    internal const int RoomRows = 16;
    /// <summary>The row of slope blocks, counted from the room's top, that lies on the floor across the whole room.</summary>
    internal const int SlopeRow = 10;
    /// <summary>The row of the room's solid floor, counted from its top: the row under <see cref="SlopeRow"/>.</summary>
    internal const int FloorRow = SlopeRow + 1;
    /// <summary>How every block of <see cref="SlopeRow"/> collides: as a slope.</summary>
    internal const PlainTerrainBlock SlopeBlock = PlainTerrainBlock.Slope;
    /// <summary>How every block of <see cref="FloorRow"/> collides: as a solid block.</summary>
    internal const PlainTerrainBlock FloorBlock = PlainTerrainBlock.Solid;
    /// <summary>How every other block of the room collides: as air.</summary>
    internal const PlainTerrainBlock EmptyBlock = PlainTerrainBlock.Air;
}
