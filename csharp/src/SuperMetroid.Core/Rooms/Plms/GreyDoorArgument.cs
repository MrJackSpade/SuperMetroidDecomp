namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A grey door's unlock condition and the door bit it keeps once set up.</summary>
/// <param name="ConditionIndex">The condition's position in the grey-door condition table.</param>
/// <param name="Door">The door bit the cap keeps.</param>
public sealed record GreyDoorArgument(int ConditionIndex, DoorBitArgument Door) : PlmRoomArgument;
