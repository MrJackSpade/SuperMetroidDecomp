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
    /// <summary>$7E:3400 button source expressed relative to the $7E:3000 native word indexes.</summary>
    public const int ButtonSourceWordOrigin = 0x0200;
    /// <summary>MAP, EXIT and SAMUS labels recolored by $82:A628-$A84C.</summary>
    public static IEnumerable<(int Word, int Count)> ButtonLabelSpans
    {
        get
        {
            for (int label = 0; label < 3; label++)
            for (int row = 0; row < 2; row++)
                yield return ButtonLabelSpan((PauseButtonLabel)label, row);
        }
    }

    /// <summary>Projects a named two-row button label into the 32-column BG2 tilemap.</summary>
    /// <remarks>$82:A633/A66F/A6AB select MAP/EXIT/SAMUS at columns5/12/22.
    /// Their lower halves are one32-word row below, as at A651/A68D/A6C9.
    /// The center EXIT label occupies four columns; the outside labels occupy five.</remarks>
    public static (int Word, int Count) ButtonLabelSpan(PauseButtonLabel label, int row)
    {
        int column = label switch
        {
            PauseButtonLabel.Map => 5,
            PauseButtonLabel.Exit => 12,
            PauseButtonLabel.Samus => 22,
            _ => throw new ArgumentOutOfRangeException(nameof(label)),
        };
        if ((uint)row >= 2) throw new ArgumentOutOfRangeException(nameof(row));
        return ((25 + row) * 32 + column, label == PauseButtonLabel.Exit ? 4 : 5);
    }
    /// <summary>Three-bit BG palette index applied to unavailable equipment labels.</summary>
    public const int DisabledEquipmentPaletteIndex = 3;
    /// <summary>OBSEL value installed by the pause-screen PPU setup.</summary>
    public const byte ObjectSelection = 0x01;
    /// <summary>$82:B9C8 selects OBJ palette 7 for the Samus position indicator, independently overridable by authored sprite parts.</summary>
    public const ushort MapMarkerPaletteBits = 0x0e00;
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

/// <summary>Shared four-phase pulse for pause and file-select map markers at $82:B9FC.</summary>
internal static class PauseMapIndicatorAnimation
{
    /// <summary>$82:BA06 wraps at byte offset8: four word-indexed phases.</summary>
    public const int FrameCount = 4;

    /// <summary>Calculates the triangular sprite pulse at $82:BA2D, phase0..3.</summary>
    /// <remarks>Independently reviewed for #1165: start at5F, rise twice, then fall.
    /// The bounded triangle is2-abs(2-phase). Invalid indices preserve the former
    /// array's IndexOutOfRangeException; timer advancement remains caller-owned.</remarks>
    public static ushort SpritemapId(int frame)
    {
        if ((uint)frame >= FrameCount) throw new IndexOutOfRangeException();
        return (ushort)(0x5f + 2 - Math.Abs(2 - frame));
    }

    /// <summary>Calculates the alternating endpoint/midpoint dwell at $82:BA25.</summary>
    /// <remarks>Independently reviewed for #1165: phases0/2 hold an endpoint for8
    /// ticks; phases1/3 traverse the middle image for4. Native consumers advance
    /// before loading this delay and decrement on that same tick.</remarks>
    public static int FrameDelay(int frame)
    {
        if ((uint)frame >= FrameCount) throw new IndexOutOfRangeException();
        return 8 - 4 * (frame & 1);
    }
}

/// <summary>One bank-$82 equipment-category table record used by the pause screen.</summary>
internal readonly record struct PauseEquipmentCategoryDefinition(
    int Category,
    int OffsetTableAddress,
    int TilemapPointerTableAddress,
    int ItemCount,
    int LabelWordCount);

/// <summary>Native reserve, beam, suit/misc, and boot category definitions.</summary>
internal static class PauseEquipmentCategories
{
    /// <summary>$82:AC58 category dispatcher: reserve tanks, low selector byte zero.</summary>
    public const int Reserves = 0;
    /// <summary>$82:AFBE Weapons category, low selector byte one.</summary>
    public const int Beams = 1;
    /// <summary>$82:B0C2 Suit category, low selector byte two.</summary>
    public const int Suits = 2;
    /// <summary>$82:B150 Boots category, low selector byte three.</summary>
    public const int Boots = 3;
    /// <summary>$82:C04C Weapons mask table index of Spazer.</summary>
    public const int SpazerItem = 3;
    /// <summary>$82:C04C Weapons mask table index of Plasma; bottom of beam list.</summary>
    public const int PlasmaItem = 4;
    /// <summary>$82:AFDB ordinary Right entry starts at suit/misc table byte offset four.</summary>
    public const int BeamRightSuitItem = 2;
    /// <summary>$82:B568 target offsets are relative to EquipmentScreenBG1Tilemap at $7E:3800.</summary>
    public const int TilemapWramBase = 0x3800;
    /// <summary>Selects the native data contract for a named equipment category.</summary>
    /// <remarks>The category dispatcher at $82:AC58 selects distinct controls, not
    /// a numeric sequence. Pointer fields correspond to $82:C02C/C034/C044;
    /// item counts follow $82:ABCC/ABEB/AC05 and copy lengths $82:AFCE/B0C8/B156.
    /// Reserves use separate controls and retain the managed zero-data contract.
    /// Invalid categories preserve the former array's IndexOutOfRangeException.</remarks>
    public static PauseEquipmentCategoryDefinition Get(int category) => category switch
    {
        Reserves => new(Reserves, 0, 0, 0, 0),
        Beams => new(Beams, 0x82c06c, 0x82c08c, 5, 5),
        Suits => new(Suits, 0x82c076, 0x82c096, 6, 9),
        Boots => new(Boots, 0x82c082, 0x82c0a2, 3, 9),
        _ => throw new IndexOutOfRangeException(),
    };
}

/// <summary>Distinct pause-button label identities in the native recoloring routines.</summary>
internal enum PauseButtonLabel
{
    /// <summary>$82:A633 MAP label at the left of the button row.</summary>
    Map,
    /// <summary>$82:A66F EXIT label at the center of the button row.</summary>
    Exit,
    /// <summary>$82:A6AB SAMUS label at the right of the button row.</summary>
    Samus,
}
