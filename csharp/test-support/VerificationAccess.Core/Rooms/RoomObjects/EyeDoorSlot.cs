using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A room-object place and the eye-door eye, body or bottom it holds, if it holds one.</summary>
/// <param name="Slot">The place.</param>
/// <param name="Door">The eye, if the place holds it.</param>
/// <param name="Part">The body or bottom, if the place holds one.</param>
internal readonly record struct EyeDoorSlot(RoomObjectSlot Slot, EyeDoorEyeObject? Door, EyeDoorPartObject? Part);
