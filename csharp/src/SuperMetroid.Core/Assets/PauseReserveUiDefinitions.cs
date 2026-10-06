namespace SuperMetroid.Core.Assets;

/// <summary>Editable reserve-label, digit and arrow presentation sources and dimensions.</summary>
public static class PauseReserveUiDefinitions
{
    public const int Version = 1;
    public const string FileName = "pause-reserve-ui.json";
    public const int TilemapColumns = 32, TilemapRows = 32;
    public const int LabelWords = 7, ModeWords = 4, DigitCount = 10, ArrowFrames = 32;
    public const int SupplyDigitPlaces = 3;
    /// <summary>$82:AB47 preserves priority, palette and flip fields while replacing MANUAL/AUTO characters.</summary>
    public const ushort TileAttributeMask = 0xfc00;

    /// <summary>$82:C068, native destinations for MODE/MANUAL and RESERVE TANK labels.</summary>
    public const int LabelDestinations = 0x82c068;
    /// <summary>$82:C088, native source pointers for MODE/MANUAL and RESERVE TANK labels.</summary>
    public const int LabelSources = 0x82c088;
    /// <summary>$82:BF22, MANUAL replacement text.</summary>
    public const int ManualSource = 0x82bf22;
    /// <summary>$82:BF2A, AUTO replacement text.</summary>
    public const int AutoSource = 0x82bf2a;
    /// <summary>$82:AD5D, animated reserve-arrow palette-six color-six sequence.</summary>
    public const int ArrowColor6Source = 0x82ad5d;
    /// <summary>$82:AD9D, animated reserve-arrow palette-six color-eleven sequence.</summary>
    public const int ArrowColor11Source = 0x82ad9d;
    /// <summary>$7E:3800, native mutable equipment tilemap base.</summary>
    public const int TilemapWramBase = 0x3800;
    /// <summary>$82:AB47 destination for MANUAL/AUTO replacement text.</summary>
    public const int ModeCell = 327;
    /// <summary>$82:8FCE destination for the three reserve-supply digits.</summary>
    public const int DigitCell = 392;
    /// <summary>$82:8F70 native zero tilemap word; digits zero through nine are consecutive.</summary>
    public const ushort DigitZeroWord = 0x0804;
    /// <summary>$82:ADE4/$ADF6 solid-arrow color six.</summary>
    public const ushort SolidColor6 = 0x0156;
    /// <summary>$82:ADDD/$ADEF solid-arrow color eleven.</summary>
    public const ushort SolidColor11 = 0x039e;
    public const int EnabledPalette = 6, DisabledPalette = 7;
    public const int VerticalStartCell = 0x102 / 2, VerticalCount = 8, RowStrideCells = 0x40 / 2;
    public const int HorizontalStartCell = 0x302 / 2, HorizontalCount = 2;
    /// <summary>$82:BF06-BF0A selects MODE at interface tiles $119-11B with palette one and priority.</summary>
    private const int ModeGlyph = 0x119, ModePrefixWords = 3;
    /// <summary>$82:BF0C-BF12 and BF22-BF28 share MANUAL at tiles $146-149, palette seven and priority.</summary>
    private const int ManualGlyph = 0x146;
    /// <summary>$82:BF2A-BF30 selects AUTO at tiles $156-159, palette seven and priority.</summary>
    private const int AutoGlyph = 0x156;
    /// <summary>$82:BF14-BF20 selects RESERVE TANK at tiles $80-86, palette seven and priority.</summary>
    private const int ReserveTankGlyph = 0x80;

    internal static int StockLabelWords(string name) => name switch
    {
        "Mode" or "ReserveTank" => LabelWords,
        "Manual" or "Auto" => ModeWords,
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    };

    /// <summary>$82:C068/C06A places the seven-word labels on consecutive rows; replacement modes follow MODE.</summary>
    internal static int StockLabelOffset(string name) => sizeof(ushort) * (name switch
    {
        "Mode" => ModeCell - ModePrefixWords,
        "ReserveTank" => ModeCell - ModePrefixWords + TilemapColumns,
        "Manual" or "Auto" => ModeCell,
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    });

    /// <summary>Compose reserve text from named consecutive glyph runs. Independent glyph pixels remain artwork.</summary>
    internal static ushort StockLabelWord(string name, int index)
    {
        if ((uint)index >= StockLabelWords(name)) throw new IndexOutOfRangeException();
        if (name == "Mode")
            return index < ModePrefixWords ? Word(ModeGlyph + index, 1)
                : Word(ManualGlyph + index - ModePrefixWords, 7);
        int first = name switch
        {
            "ReserveTank" => ReserveTankGlyph,
            "Manual" => ManualGlyph,
            "Auto" => AutoGlyph,
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };
        return Word(first + index, 7);
    }

    private static ushort Word(int glyph, int palette) => (ushort)(0x2000 | palette << 10 | glyph);
}
