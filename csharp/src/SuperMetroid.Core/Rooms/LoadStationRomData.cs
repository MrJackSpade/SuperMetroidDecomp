namespace SuperMetroid.Core.Rooms;

/// <summary>Native bank-$80 addresses retained only for load-station parity diagnostics.</summary>
public static class LoadStationRomData
{

    /// <summary>Number of bytes in one native room/door/camera/Samus placement record.</summary>
    public const int EntryByteCount = 14;

    /// <summary>$80:C4C5, first byte of the Crateria list.</summary>
    public const int DataStart = 0x80c4c5;
}
