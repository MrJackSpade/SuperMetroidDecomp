namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// What a room places a PLM with, decoded at import (#1452) into what the PLM's setup and state read: an item or door
/// bit, a grey door's unlock condition, a station, a Metroid room's entry or a gate trigger. A PLM placed without an
/// argument, or with a scroll program or cannon control its placement carries instead, holds none.
/// </summary>
public abstract record PlmRoomArgument;
