namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="RoomHeaderRomData"/> that only verification reads.</summary>
internal static class RoomHeaderRomDataConstants
{
    /// <summary>SNES bank containing room headers, inline selectors, and room-state records.</summary>
    public const int BankAddress = 0x8f0000;

    /// <summary>Provides the native room header bank address used to resolve room records.</summary>
    extension(RoomHeaderRomData)
    {
        /// <inheritdoc cref="RoomHeaderRomDataConstants.BankAddress"/>
        internal static int BankAddress => RoomHeaderRomDataConstants.BankAddress;
    }
}
