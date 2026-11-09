using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The geometry and collision of the block draws Bomb Torizo's resident hand PLM selects. What each frame shows is its
/// installed picture (#1557).
/// </summary>
internal static class BombTorizoHandObjectDrawDefinitions
{
    /// <summary>A cleared cell, air.</summary>
    private const RoomCollisionType ClearedCell = RoomCollisionType.Air;

    /// <summary>
    /// Each draw's horizontal strips in authored run order, each placed where it starts from the hand PLM block. Intact is
    /// four strips of solid blocks: the origin pair, the left singleton, the upper pair and the lower triple. Cleared
    /// splits the origin row into two halves, then clears the lower, top and upper rows of a four-by-four area.
    /// </summary>
    internal static readonly Dictionary<BombTorizoHandDraw, RoomObjectBlockStrip[]> Strips = new()
    {
        [BombTorizoHandDraw.Intact] =
        [
            new(new(0, 0), [RoomCollisionType.SolidBlock, RoomCollisionType.SolidBlock]),
            new(new(-1, 0), [RoomCollisionType.SolidBlock]),
            new(new(0, -1), [RoomCollisionType.SolidBlock, RoomCollisionType.SolidBlock]),
            new(new(-1, 1), [RoomCollisionType.SolidBlock, RoomCollisionType.SolidBlock, RoomCollisionType.SolidBlock]),
        ],
        [BombTorizoHandDraw.Cleared] =
        [
            new(new(0, 0), [ClearedCell, ClearedCell]),
            new(new(-2, 0), [ClearedCell, ClearedCell]),
            new(new(-2, 1), [ClearedCell, ClearedCell, ClearedCell, ClearedCell]),
            new(new(-2, -2), [ClearedCell, ClearedCell, ClearedCell, ClearedCell]),
            new(new(-2, -1), [ClearedCell, ClearedCell, ClearedCell, ClearedCell]),
        ],
    };
}
