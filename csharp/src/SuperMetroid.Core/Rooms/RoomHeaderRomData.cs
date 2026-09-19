namespace SuperMetroid.Core.Rooms;

/// <summary>Native bank-$8F room-header locations retained for staged parity diagnostics.</summary>
public static class RoomHeaderRomData
{
    /// <summary>SNES bank containing room headers, inline selectors, and room-state records.</summary>
    public const int BankAddress = 0x8f0000;

    /// <summary>Fixed bytes preceding a room's inline state-selection program.</summary>
    public const int FixedHeaderByteCount = 11;

    /// <summary>Bytes in one selected room-state record consumed by $82:DEF2.</summary>
    public const int StateByteCount = 26;
}
