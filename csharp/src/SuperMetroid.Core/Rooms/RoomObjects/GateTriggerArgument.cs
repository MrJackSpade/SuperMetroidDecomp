namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The colour and side of a downward gate's shot-block trigger.</summary>
/// <param name="Trigger">The trigger the placement installs.</param>
internal sealed record GateTriggerArgument(DownwardGateTriggerBehavior Trigger) : RoomObjectRoomArgument;
