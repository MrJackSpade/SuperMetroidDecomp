using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The terrain a Draygon cannon writes into one of its two room blocks: a collision type and its behaviour.</summary>
/// <param name="Type">The block's collision type; the block keeps its own tile.</param>
/// <param name="Behavior">The behaviour the collision type gives the block.</param>
internal readonly record struct DraygonCannonTerrain(RoomCollisionType Type, RoomBlockBehavior Behavior);
