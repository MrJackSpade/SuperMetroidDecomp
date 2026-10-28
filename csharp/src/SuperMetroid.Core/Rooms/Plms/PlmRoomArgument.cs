namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// What a room places a PLM with, decoded at import (#1452) into what the PLM's setup and state read: an item or door
/// bit, a grey door's unlock condition, a station, a Metroid room's entry or a gate trigger. A PLM placed without an
/// argument, or with a scroll program or cannon control its placement carries instead, holds none.
/// </summary>
public abstract record PlmRoomArgument;

/// <summary>The collected-item bit a collectible sets when Samus picks it up.</summary>
/// <param name="ItemBit">The bit's index in the collected-items record.</param>
public sealed record CollectedItemArgument(int ItemBit) : PlmRoomArgument;

/// <summary>The opened-door bit a door cap records, and whether opening the cap records it.</summary>
/// <param name="DoorBit">The bit's index in the opened-doors record.</param>
/// <param name="Persists">Whether opening the cap sets the bit and an already set bit keeps the cap open.</param>
public sealed record DoorBitArgument(int DoorBit, bool Persists) : PlmRoomArgument;

/// <summary>A grey door's unlock condition and the door bit it keeps once set up.</summary>
/// <param name="ConditionIndex">The condition's position in the grey-door condition table.</param>
/// <param name="Door">The door bit the cap keeps.</param>
public sealed record GreyDoorArgument(int ConditionIndex, DoorBitArgument Door) : PlmRoomArgument;

/// <summary>
/// A cap that has been opened for good: its door bit is set, and the cap keeps the opened-door flags of the sixteen
/// doors in the bit's window as the opening left them. It is never checked again.
/// </summary>
/// <param name="DoorBit">The bit the opening set.</param>
/// <param name="OpenedDoorWindow">The window's opened-door flags at the opening, one bit per door from the window's first.</param>
public sealed record SpentDoorArgument(int DoorBit, int OpenedDoorWindow) : PlmRoomArgument;

/// <summary>A station's number, which its activation events carry.</summary>
/// <param name="Station">The station's number.</param>
public sealed record StationArgument(int Station) : PlmRoomArgument;

/// <summary>The entry of the Metroids-cleared table a Tourian PLM selects.</summary>
/// <param name="Entry">The table entry, 0 through 12.</param>
public sealed record MetroidRoomArgument(int Entry) : PlmRoomArgument;

/// <summary>The colour and side of a downward gate's shot-block trigger.</summary>
/// <param name="Trigger">The trigger's position in the gate's trigger table.</param>
public sealed record GateTriggerArgument(int Trigger) : PlmRoomArgument;
