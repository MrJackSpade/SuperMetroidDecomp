namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Cartridge values of <see cref="PauseMenuLayout"/> that only verification reads.</summary>
internal static class PauseMenuLayoutConstants
{
    /// <summary>Number of decimal digits written by $82:8F70.</summary>
    public const int ReserveSupplyDigitCount = 3;
    /// <summary>
    /// Tilemap word for decimal zero used by $82:8F70; decimal digit values are added to
    /// this word without altering its palette or priority fields.
    /// </summary>
    public const ushort ReserveSupplyDigitZeroTile = 0x0804;
    /// <summary>
    /// Byte offset of the reserve-supply hundreds digit in the mutable equipment tilemap,
    /// matching <c>EquipmentScreenBG1Tilemap+$310</c> at $82:8FCE.
    /// </summary>
    public const int ReserveSupplyDigitsByteOffset = 0x0310;

    /// <summary>Provides reserve supply digit count, zero tile, and byte offset for pause menu layout checks.</summary>
    extension(PauseMenuLayout)
    {
        /// <inheritdoc cref="PauseMenuLayoutConstants.ReserveSupplyDigitCount"/>
        internal static int ReserveSupplyDigitCount => PauseMenuLayoutConstants.ReserveSupplyDigitCount;
        /// <inheritdoc cref="PauseMenuLayoutConstants.ReserveSupplyDigitZeroTile"/>
        internal static ushort ReserveSupplyDigitZeroTile => PauseMenuLayoutConstants.ReserveSupplyDigitZeroTile;
        /// <inheritdoc cref="PauseMenuLayoutConstants.ReserveSupplyDigitsByteOffset"/>
        internal static int ReserveSupplyDigitsByteOffset => PauseMenuLayoutConstants.ReserveSupplyDigitsByteOffset;
    }
}
