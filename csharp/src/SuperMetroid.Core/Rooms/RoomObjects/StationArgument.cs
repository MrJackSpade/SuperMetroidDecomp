namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A station's number, which its activation events carry.</summary>
/// <param name="Station">The station's number.</param>
public sealed record StationArgument(int Station) : RoomObjectRoomArgument;
