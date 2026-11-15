using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A room-object place and the coloured door cap it holds, if it holds one.</summary>
/// <param name="Place">The slot.</param>
/// <param name="Door">The door.</param>
internal readonly record struct ColoredDoorPlace(RoomObjectPlace Place, ColoredDoorObject? Door);
