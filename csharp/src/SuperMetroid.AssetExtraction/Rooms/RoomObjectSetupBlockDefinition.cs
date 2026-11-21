using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>What one room-object setup block's level words carry (<see cref="RoomObjectSetupBlockDefinitions"/>).</summary>
/// <param name="WordCollision">The collision the level word holding the block carries.</param>
/// <param name="KeepsStruckMirroring">Whether the game shows the block with the struck block's own mirroring, so its picture names a block only.</param>
internal sealed record RoomObjectSetupBlockDefinition(RoomCollisionType WordCollision, bool KeepsStruckMirroring);
