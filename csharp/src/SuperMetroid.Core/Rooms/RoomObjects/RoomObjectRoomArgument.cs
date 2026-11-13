namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What a room places a room object with, decoded at import (#1452) into what the room object's setup and state read: an item or door
/// bit, a grey door's unlock condition, a station, a Metroid room's entry or a gate trigger. A room object placed without an
/// argument, or with a scroll change or cannon control its placement carries instead, holds none.
/// </summary>
public abstract record RoomObjectRoomArgument;
