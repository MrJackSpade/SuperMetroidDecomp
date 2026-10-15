namespace SuperMetroid.Desktop.Diagnostics;

/// <summary>The cartridge banks the room and door pointers an error report quotes are bank-relative to.</summary>
internal enum ReportedPointerBank : byte
{
    /// <summary>Bank $83: door headers.</summary>
    DoorHeaders = 0x83,
    /// <summary>Bank $8F: room headers and their room states.</summary>
    RoomHeaders = 0x8F,
}
