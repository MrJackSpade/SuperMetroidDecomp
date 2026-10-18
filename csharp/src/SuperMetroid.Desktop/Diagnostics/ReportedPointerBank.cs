namespace SuperMetroid.Desktop.Diagnostics;

/// <summary>The cartridge bank the room-state pointer an error report quotes is bank-relative to.</summary>
internal enum ReportedPointerBank : byte
{
    /// <summary>Bank $8F: room headers and their room states.</summary>
    RoomHeaders = 0x8F,
}
