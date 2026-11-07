using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A PLM slot and its eye door state, if it is one.</summary>
/// <param name="Slot">The slot.</param>
/// <param name="Door">The door.</param>
internal readonly record struct EyeDoorSlot(RoomObjectSlot Slot, EyeDoorObjects.EyeDoorObjectState? Door);
