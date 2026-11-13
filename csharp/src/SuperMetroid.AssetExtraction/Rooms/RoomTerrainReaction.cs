using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// How one block of a room's installed terrain collides and the behaviour it holds (#1636, #1722): what decides which
/// block reactions, and which of the X-ray reveal's copies, the block can start.
/// </summary>
/// <param name="Collision">How the block collides.</param>
/// <param name="Behavior">The block's behaviour, whose reading for <paramref name="Collision"/> selects its reaction.</param>
internal readonly record struct RoomTerrainReaction(RoomCollisionType Collision, RoomBlockBehavior Behavior);
