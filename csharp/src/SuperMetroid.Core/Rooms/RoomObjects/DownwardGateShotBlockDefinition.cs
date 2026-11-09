using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One downward-gate shot-block setup action.
/// </summary>
/// <param name="Trigger">The trigger the written shot block selects.</param>
/// <param name="Change">What the trigger's room object draws: its shot block, once, before it leaves the room.</param>
/// <param name="LeftCollision">
/// The collision kind the block left of the trigger is given, keeping what it shows, or null to leave it alone.
/// </param>
/// <param name="RightCollision">
/// The collision kind the block right of the trigger is given, keeping what it shows, or null to leave it alone.
/// </param>
internal readonly record struct DownwardGateShotBlockDefinition(
    DownwardGateTriggerBehavior Trigger,
    TerrainChange Change,
    RoomCollisionType? LeftCollision,
    RoomCollisionType? RightCollision);
