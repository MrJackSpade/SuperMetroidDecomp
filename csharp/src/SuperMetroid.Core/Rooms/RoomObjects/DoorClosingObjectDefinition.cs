using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room object a door transition places to close the door behind Samus when nothing at the door closes.</summary>
/// <param name="Header">The object's identity.</param>
/// <param name="BlueCapFacing">The facing of the blue cap that closes, or null for Mother Brain's escape gate closing.</param>
internal readonly record struct DoorClosingObjectDefinition(
    RoomObjectHeaderId Header,
    ColoredDoorOrientation? BlueCapFacing);
