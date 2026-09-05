namespace SuperMetroid.Core.Frontend;

/// <summary>VRAM layout and palette fields used by the bank-$82 pause-screen routines.</summary>
internal static class PauseMenuLayout
{
    /// <summary>BG1 tilemap base word used by the map and equipment pages.</summary>
    public const ushort Bg1TilemapWord = 0x3000;
    /// <summary>BG2 tilemap base word used by the pause-screen frame.</summary>
    public const ushort Bg2TilemapWord = 0x3800;
    /// <summary>First BG3 word cleared below the retained four-row HUD.</summary>
    public const ushort Bg3FxClearDestinationWord = 0x5880;
    /// <summary>Pause character/palette word used to blank the retained FX plane.</summary>
    public const ushort Bg3FxClearTile = 0x184e;
    /// <summary>Number of BG3 words below the four-row HUD.</summary>
    public const int Bg3FxClearWordCount = 0x0780;
    /// <summary>BG2 destination for the two mutable pause-button rows.</summary>
    public const ushort ButtonRowsDestinationWord = 0x3b20;
    /// <summary>Byte offset of those rows in the bank-$B6 button tilemap.</summary>
    public const int ButtonRowsSourceOffset = 0x0240;
    /// <summary>Combined byte length of the two mutable button rows.</summary>
    public const int ButtonRowsByteCount = 0x0080;
    /// <summary>Three-bit BG palette index applied to unavailable equipment labels.</summary>
    public const int DisabledEquipmentPaletteIndex = 3;
    /// <summary>OBSEL value installed by the pause-screen PPU setup.</summary>
    public const byte ObjectSelection = 0x01;
    /// <summary>
    /// Byte offset of the reserve-supply hundreds digit in the mutable equipment tilemap,
    /// matching <c>EquipmentScreenBG1Tilemap+$310</c> at $82:8FCE.
    /// </summary>
    public const int ReserveSupplyDigitsByteOffset = 0x0310;
    /// <summary>Number of decimal digits written by $82:8F70.</summary>
    public const int ReserveSupplyDigitCount = 3;
    /// <summary>
    /// Tilemap word for decimal zero used by $82:8F70; decimal digit values are added to
    /// this word without altering its palette or priority fields.
    /// </summary>
    public const ushort ReserveSupplyDigitZeroTile = 0x0804;
}

/// <summary>Cartridge-authored four-frame animation for Samus's pause-map marker.</summary>
internal static class PauseMapIndicatorAnimation
{
    public static readonly ushort[] SpritemapIds = [0x5f, 0x60, 0x61, 0x60];
    public static readonly int[] FrameDelays = [8, 4, 8, 4];
}

/// <summary>One bank-$82 equipment-category table record used by the pause screen.</summary>
internal readonly record struct PauseEquipmentCategoryDefinition(
    int OffsetTableAddress,
    int TilemapPointerTableAddress,
    int BitmaskTableAddress,
    int ItemCount,
    int LabelWordCount);

/// <summary>Native reserve, beam, suit/misc, and boot category definitions.</summary>
internal static class PauseEquipmentCategories
{
    public static readonly PauseEquipmentCategoryDefinition[] Definitions =
    [
        // Category zero is reserve tanks. Its special controls are unavailable until Samus
        // owns reserve capacity, so the native equipment tables are intentionally null.
        new(0, 0, 0, 0, 0),
        new(0x82c06c, 0x82c08c, 0x82c04c, 5, 5),
        new(0x82c076, 0x82c096, 0x82c056, 6, 9),
        new(0x82c082, 0x82c0a2, 0x82c062, 3, 9),
    ];
}
