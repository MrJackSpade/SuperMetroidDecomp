namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Cartridge values of the pause screen's layout that only verification reads.</summary>
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
    /// <summary>The tiles in the frame's two button-label rows, which each highlight change uploads.</summary>
    public const int ButtonRowsTileCount = 0x0040;
    /// <summary>BG1 tilemap base word used by the map and equipment pages.</summary>
    public const ushort Bg1TilemapWord = 0x3000;
    /// <summary>BG2 tilemap base word used by the pause-screen frame.</summary>
    public const ushort Bg2TilemapWord = 0x3800;
}
