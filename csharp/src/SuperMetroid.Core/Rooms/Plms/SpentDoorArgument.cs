namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// A cap that has been opened for good: its door bit is set, and the cap keeps the opened-door flags of the sixteen
/// doors in the bit's window as the opening left them. It is never checked again.
/// </summary>
/// <param name="DoorBit">The bit the opening set.</param>
/// <param name="OpenedDoorWindow">The window's opened-door flags at the opening, one bit per door from the window's first.</param>
public sealed record SpentDoorArgument(int DoorBit, int OpenedDoorWindow) : PlmRoomArgument;
