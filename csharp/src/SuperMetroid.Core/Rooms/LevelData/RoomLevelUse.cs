namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// A room using a level for one or more of its states (#2090): a save or map room's level is shared by several rooms, each
/// leading its door blocks through its own doors.
/// </summary>
/// <param name="Room">The room whose state names the level.</param>
/// <param name="Level">The level.</param>
public readonly record struct RoomLevelUse(RoomDefinition Room, RoomLevelId Level);
