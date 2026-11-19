using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>A door block of a room's grid and where it leads (#2090).</summary>
/// <param name="Block">The door block.</param>
/// <param name="Exit">The door it leads through, or the elevator ride.</param>
public readonly record struct RoomDoorBlock(Position16<RoomBlocks> Block, DoorBlockExit Exit);
