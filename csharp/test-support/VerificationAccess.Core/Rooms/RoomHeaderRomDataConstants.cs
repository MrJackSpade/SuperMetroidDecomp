namespace SuperMetroid.Core.Rooms;

/// <summary>Where the cartridge keeps room headers; only verification reads it.</summary>
internal static class RoomHeaderRomDataConstants
{
    /// <summary>SNES bank containing room headers, inline selectors, and room-state records.</summary>
    public const int BankAddress = 0x8f0000;
}
