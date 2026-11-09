using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One downward-gate shot-block setup action.
/// </summary>
/// <param name="Trigger">The trigger the written shot block selects.</param>
/// <param name="Change">What the trigger's room object draws: its shot block, once, before it leaves the room.</param>
/// <param name="LeftBlock">The terrain written left of the trigger, or null for none.</param>
/// <param name="RightBlock">The terrain written right of the trigger, or null for none.</param>
internal readonly record struct DownwardGateShotBlockDefinition(
    DownwardGateTriggerBehavior Trigger,
    TerrainChange Change,
    RoomTerrainBlock? LeftBlock,
    RoomTerrainBlock? RightBlock);
