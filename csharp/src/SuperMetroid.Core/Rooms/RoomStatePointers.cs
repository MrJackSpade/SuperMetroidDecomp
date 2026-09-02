namespace SuperMetroid.Core.Rooms;

/// <summary>Named 16-bit room-state pointers within cartridge bank $8F.</summary>
public static class RoomStatePointers
{
    /// <summary>Parlor state selected after event zero wakes Zebes.</summary>
    public const ushort AwakenedParlor = 0x932e;

    /// <summary>Blue Brinstar elevator state selected after Morph Ball and Missiles.</summary>
    public const ushort BlueBrinstarElevatorAfterItems = 0x97e0;
}
