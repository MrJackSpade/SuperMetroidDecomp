namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The colour and side of a downward gate's shot-block trigger.</summary>
/// <param name="ShotBlock">The trigger's shot-block setup, one of <see cref="DownwardGateShotBlockDefinitions"/>.</param>
internal sealed record GateTriggerArgument(DownwardGateShotBlockDefinition ShotBlock) : RoomObjectRoomArgument;
