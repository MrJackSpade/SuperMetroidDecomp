namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where the chamber's walls stand during Ceres Ridley's getaway (#1545): the room points the getaway scene places its
/// wall pictures at, and the places the retreat's two wall actors take.
/// </summary>
internal static class CeresGetawayChamberWallDefinitions
{
    /// <summary>The room point the chamber's left wall stands at.</summary>
    internal static Position16<RoomPixels> LeftWall { get; } = new(8, 127);

    /// <summary>The room point the chamber's right wall stands at.</summary>
    internal static Position16<RoomPixels> RightWall { get; } = new(248, 127);
}
