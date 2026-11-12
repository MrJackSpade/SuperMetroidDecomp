using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// How one block of a room's installed terrain collides and the reaction it selects (#1636): what decides which block
/// reactions, and which of the X-ray reveal's copies, the block can start.
/// </summary>
/// <param name="Collision">How the block collides.</param>
/// <param name="Reaction">The reaction the block's behaviour selects.</param>
internal readonly record struct RoomTerrainReaction(RoomCollisionType Collision, BlockReaction Reaction);
