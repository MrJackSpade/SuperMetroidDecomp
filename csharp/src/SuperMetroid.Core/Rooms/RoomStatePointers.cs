namespace SuperMetroid.Core.Rooms;

/// <summary>Named 16-bit room-state pointers within cartridge bank $8F.</summary>
public static class RoomStatePointers
{
    /// <summary>Default pre-awakening Parlor state selected by the first Crateria visits.</summary>
    public const ushort DefaultParlor = 0x9314;

    /// <summary>Parlor state selected after event zero wakes Zebes.</summary>
    public const ushort AwakenedParlor = 0x932e;

    /// <summary>Blue Brinstar elevator state selected after Morph Ball and Missiles.</summary>
    public const ushort BlueBrinstarElevatorAfterItems = 0x97e0;

    /// <summary>Green Brinstar elevator room's sole state at $8F:9945.</summary>
    public const ushort GreenBrinstarElevator = 0x9945;

    /// <summary>Green Brinstar main shaft's sole state at $8F:9AE6.</summary>
    public const ushort GreenBrinstarMainShaft = 0x9ae6;
}
