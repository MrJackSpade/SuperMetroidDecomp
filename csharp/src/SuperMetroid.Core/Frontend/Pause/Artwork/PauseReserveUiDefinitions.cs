using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Editable reserve-label, digit and arrow presentation sources and dimensions.</summary>
public static class PauseReserveUiDefinitions
{
    /// <summary>Supported reserve UI document schema revision.</summary>
    public const int Version = 1;
    /// <summary>JSON filename containing reserve labels, digits, arrow locations, and colors.</summary>
    public const string FileName = "pause-reserve-ui.json";
    /// <summary>Equipment-page tilemap dimensions in cells.</summary>
    public const int TilemapColumns = 32, TilemapRows = 32;
    /// <summary>Fixed label lengths, digit glyph count, and arrow pulse-frame count.</summary>
    public const int LabelWords = 7, ModeWords = 4, DigitCount = 10, ArrowFrames = 32;
    /// <summary>The background row holding the reserve arrow's two animated colours.</summary>
    public const int ArrowBackgroundRow = 6;
    /// <summary>The row's first animated arrow colour.</summary>
    public const int ArrowFirstColour = 6;
    /// <summary>The row's second animated arrow colour.</summary>
    public const int ArrowSecondColour = 11;
    /// <summary>Number of decimal places in the reserve-supply display.</summary>
    public const int SupplyDigitPlaces = 3;
    /// <summary>$82:AB47 destination for MANUAL/AUTO replacement text.</summary>
    public const int ModeCell = 327;
    /// <summary>$82:8FCE destination for the three reserve-supply digits.</summary>
    public const int DigitCell = 392;
    /// <summary>$82:8F70: the zero digit's tile, glyph 4 in palette two; digits zero through nine are consecutive.</summary>
    public static BackgroundTile DigitZeroTile { get; } = new(0x04, 2, highPriority: false);
    /// <summary>$82:ADE4/$ADF6 solid-arrow color six.</summary>
    public const ushort SolidColor6 = 0x0156;
    /// <summary>$82:ADDD/$ADEF solid-arrow color eleven.</summary>
    public const ushort SolidColor11 = 0x039e;
    /// <summary>BG palette selectors used by enabled and disabled reserve arrows.</summary>
    public const int EnabledPalette = 6, DisabledPalette = 7;
    /// <summary>First vertical arrow cell, number of vertical arrows, and row stride in cells.</summary>
    public const int VerticalStartCell = 0x102 / 2, VerticalCount = 8, RowStrideCells = 0x40 / 2;
    /// <summary>First horizontal arrow cell and number of adjacent horizontal arrows.</summary>
    public const int HorizontalStartCell = 0x302 / 2, HorizontalCount = 2;
    /// <summary>$82:BF06-BF0A selects MODE at interface tiles $119-11B with palette one and priority.</summary>
    internal const int ModeGlyph = 0x119, ModePrefixWords = 3;
    /// <summary>$82:BF0C-BF12 and BF22-BF28 share MANUAL at tiles $146-149, palette seven and priority.</summary>
    internal const int ManualGlyph = 0x146;
    /// <summary>$82:BF2A-BF30 selects AUTO at tiles $156-159, palette seven and priority.</summary>
    internal const int AutoGlyph = 0x156;
    /// <summary>$82:BF14-BF20 selects RESERVE TANK at tiles $80-86, palette seven and priority.</summary>
    internal const int ReserveTankGlyph = 0x80;

    /// <summary>Every reserve label, in document order.</summary>
    public static IReadOnlyList<PauseReserveLabel> Labels { get; } = Enum.GetValues<PauseReserveLabel>();
}
