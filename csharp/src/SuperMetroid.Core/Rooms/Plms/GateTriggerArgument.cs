namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The colour and side of a downward gate's shot-block trigger.</summary>
/// <param name="Trigger">The trigger's position in the gate's trigger table.</param>
public sealed record GateTriggerArgument(int Trigger) : PlmRoomArgument;
