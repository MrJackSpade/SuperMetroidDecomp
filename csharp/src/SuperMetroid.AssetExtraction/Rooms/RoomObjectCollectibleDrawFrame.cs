using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>One one-block collectible draw, how the block it writes collides and its artwork identity.</summary>
internal readonly record struct RoomObjectCollectibleDrawFrame(
    CollectibleDraw Draw, RoomCollisionType Collision, string Id);
