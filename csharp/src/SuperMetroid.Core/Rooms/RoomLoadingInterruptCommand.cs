namespace SuperMetroid.Core.Rooms;

/// <summary>The room-loading interrupt command a boss's instruction stream selects for the next IRQ.</summary>
public enum RoomLoadingInterruptCommand : ushort
{
    /// <summary>IRQ command $0C: begin the HUD draw.</summary>
    BeginHudDraw = 12,
}
