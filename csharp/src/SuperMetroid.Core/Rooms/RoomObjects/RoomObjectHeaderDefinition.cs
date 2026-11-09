namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One room object a retail room population places. Its setup and first program are the room-object family's own knowledge (#1414: the
/// setup and first-script references this record carried are verification's, not the simulation's).
/// </summary>
/// <param name="Header">The room object.</param>
public readonly record struct RoomObjectHeaderDefinition(RoomObjectHeaderId Header);
