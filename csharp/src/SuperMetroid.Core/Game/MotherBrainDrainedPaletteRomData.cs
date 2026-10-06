namespace SuperMetroid.Core.Game;

/// <summary>Native $AD:EF0D/$EF4A drained-body and revival palette copies.</summary>
public static class MotherBrainDrainedPaletteRomData
{
    /// <summary>$AD:EDA0 selects revival frame2, whose EE80 word repeats EEA6; the reviewed paint animation holds this one tissue shade for the preceding palette step.</summary>
    private const int RevivalTissueHoldFrame = 2;
    /// <summary>$AD:EE80 is body color B (zero-based10); this reviewed tissue ink supplies lower-face and neck shade pixels.</summary>
    private const int RevivalHeldTissueColor = 10;
    /// <summary>Exact EE80/EEA6 prior-step tissue-shade hold; narrowly reviewed paint content, with no historical-typo claim.</summary>
    internal static int RevivalInterpolationFrame(int frame, int color) =>
        frame == RevivalTissueHoldFrame && color == RevivalHeldTissueColor ? frame - 1 : frame;
    /// <summary>$AD:EF87, eight drained transition pointers followed by zero.</summary>
    public const int ToGreyTable = 0xadef87;
    /// <summary>$AD:ED9C, reversed grey transition pointers followed by zero.</summary>
    public const int FromGreyTable = 0xaded9c;
    /// <summary>$AD:EF5F selects fifteen colors for the EF62/EF6D drain copies.</summary>
    public const int DrainedColors = 15;
    /// <summary>$AD:EF22 copies thirteen colors when reviving.</summary>
    public const int RevivalColors = 13;
    /// <summary>$AD:EF34/EF71 destination byte offset $168: back-leg colors start at CGRAM index 180.</summary>
    public const int BackLegColor = 180;
    /// <summary>$AD:EF37/EF74 copies five back-leg colors after body/brain colors.</summary>
    public const int BackLegCount = 5;
    /// <summary>$AD:EF44/EF81 writes the trailing source word directly to low WRAM $017C, not CGRAM.</summary>
    public const int TrailingWordWram = 0x7e017c;
    /// <summary>$AD:EF71 selects rear CGRAM color B4, three inks after $A9:BD39's B1 source.</summary>
    public const int RearSourceColor = BackLegColor - MotherBrainRainbowPaletteRomData.SecondaryColor;
    /// <summary>$AD:EF99's final word EFC1 repeats rainbow rear ink E, zero-based nontransparent index13; EF7E-EF81 writes it to WRAM.</summary>
    public const int TrailingRearSourceColor = 13;
}
