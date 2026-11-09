using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A room-object place and the grey door cap it holds, if it holds one.</summary>
/// <param name="Slot">The slot.</param>
/// <param name="Door">The door.</param>
internal readonly record struct GreyDoorSlot(RoomObjectSlot Slot, GreyDoorObject? Door);
