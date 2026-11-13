namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A refill station's room argument, which room data gives every energy and missile station and nothing in the game reads.
/// </summary>
/// <param name="Station">The number room data gives the refill station.</param>
public sealed record StationArgument(int Station) : RoomObjectRoomArgument;
