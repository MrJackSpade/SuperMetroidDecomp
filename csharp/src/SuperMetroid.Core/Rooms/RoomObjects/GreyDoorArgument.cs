using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A grey door's unlock condition and the door it keeps once set up.</summary>
/// <param name="Condition">What unlocks the cap.</param>
/// <param name="Door">The door the cap keeps.</param>
public sealed record GreyDoorArgument(GreyDoorCondition Condition, DoorBitArgument Door) : RoomObjectRoomArgument;
