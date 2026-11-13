using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>A room effect selection's record for one entry door (#1546): a room entered through <c>Door</c> loads <c>Record</c>.</summary>
/// <param name="Door">The entry door that selects the record.</param>
/// <param name="Record">The record the door selects.</param>
public sealed record RoomFxDoorRecord(DoorDefinition Door, RoomFxRecordDefinition Record);
