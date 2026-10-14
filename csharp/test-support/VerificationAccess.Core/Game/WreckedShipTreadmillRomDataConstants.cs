using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="WreckedShipTreadmillRomData"/> that only verification reads.</summary>
internal static class WreckedShipTreadmillRomDataConstants
{
    /// <summary>Second 32-byte graphics frame at $87:8E84.</summary>
    public const int Frame1Source = WreckedShipTreadmillRomData.Frame0Source + WreckedShipTreadmillRomData.TransferByteCount;
    /// <summary>Third 32-byte graphics frame at $87:8EA4.</summary>
    public const int Frame2Source = WreckedShipTreadmillRomData.Frame0Source + 2 * WreckedShipTreadmillRomData.TransferByteCount;
    /// <summary>Fourth 32-byte graphics frame at $87:8EC4.</summary>
    public const int Frame3Source = WreckedShipTreadmillRomData.Frame0Source + 3 * WreckedShipTreadmillRomData.TransferByteCount;

    /// <summary>Provides the three native Wrecked Ship treadmill frame sources for tile animation checks.</summary>
    extension(WreckedShipTreadmillRomData)
    {
        /// <inheritdoc cref="WreckedShipTreadmillRomDataConstants.Frame1Source"/>
        internal static int Frame1Source => WreckedShipTreadmillRomDataConstants.Frame1Source;
        /// <inheritdoc cref="WreckedShipTreadmillRomDataConstants.Frame2Source"/>
        internal static int Frame2Source => WreckedShipTreadmillRomDataConstants.Frame2Source;
        /// <inheritdoc cref="WreckedShipTreadmillRomDataConstants.Frame3Source"/>
        internal static int Frame3Source => WreckedShipTreadmillRomDataConstants.Frame3Source;
    }
}
