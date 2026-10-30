using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One downward-gate shot-block setup action.
/// </summary>
/// <param name="Program">The gate program the trigger wakes.</param>
/// <param name="LeftBlock">The terrain written left of the trigger, or null for none.</param>
/// <param name="RightBlock">The terrain written right of the trigger, or null for none.</param>
internal readonly record struct DownwardGateShotBlockDefinition(
    DownwardGateProgram Program,
    RoomTerrainBlock? LeftBlock,
    RoomTerrainBlock? RightBlock);
