namespace SuperMetroid.Core.Game;

/// <summary>The room-loading interrupt command a boss's instruction stream selects for the next IRQ.</summary>
public enum RoomLoadingInterruptCommand : ushort
{
    /// <summary>No command is pending.</summary>
    None = 0,
    /// <summary>IRQ command $0C: begin the HUD draw.</summary>
    BeginHudDraw = 12,
}
