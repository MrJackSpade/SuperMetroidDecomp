using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The item location a collectible stands at, which picking it up records as collected.</summary>
/// <param name="Location">The item location.</param>
public sealed record CollectedItemArgument(ItemLocation Location) : RoomObjectRoomArgument;
