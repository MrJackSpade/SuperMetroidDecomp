namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// A cap that has been opened for good: its door bit is set, and the cap keeps the opened-door flags of the sixteen
/// doors in the bit's window as the opening left them. It is never checked again.
/// </summary>
public sealed record SpentDoorArgument : PlmRoomArgument
{
    /// <summary>The window's opened-door flags at the opening, one bit per door from the window's first.</summary>
    internal readonly int OpenedDoorWindow;

    /// <summary>Keeps the window's opened-door flags as the opening left them.</summary>
    internal SpentDoorArgument(int openedDoorWindow) => OpenedDoorWindow = openedDoorWindow;
}
