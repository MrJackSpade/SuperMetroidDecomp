namespace SuperMetroid.Core.Rooms;

/// <summary>Native bank-$80 addresses retained only for load-station parity diagnostics.</summary>
public static class LoadStationRomData
{
    /// <summary>$80:C4B5, seven area pointers followed by the end pointer for the Ceres list.</summary>
    public const int PointerTable = 0x80c4b5;

    /// <summary>Number of bytes in one native room/door/camera/Samus placement record.</summary>
    public const int EntryByteCount = 14;

    /// <summary>$80:C4C5, first byte of the Crateria list.</summary>
    public const int DataStart = 0x80c4c5;

    /// <summary>$80:CC19, exclusive end of the Ceres list.</summary>
    public const int DataEnd = 0x80cc19;
}
