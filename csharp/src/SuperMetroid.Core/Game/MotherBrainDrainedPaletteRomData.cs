namespace SuperMetroid.Core.Game;

/// <summary>Native $AD:EF0D/$EF4A drained-body and revival palette copies.</summary>
public static class MotherBrainDrainedPaletteRomData
{
    /// <summary>$AD:EF87, eight drained transition pointers followed by zero.</summary>
    public const int ToGreyTable = 0xadef87;
    /// <summary>$AD:ED9C, reversed grey transition pointers followed by zero.</summary>
    public const int FromGreyTable = 0xaded9c;
    /// <summary>$AD:EF64 copies fifteen colors when draining.</summary>
    public const int DrainedColors = 15;
    /// <summary>$AD:EF22 copies thirteen colors when reviving.</summary>
    public const int RevivalColors = 13;
    /// <summary>$AD:EF34/EF71 destination byte offset $168: back-leg colors start at CGRAM index 180.</summary>
    public const int BackLegColor = 180;
    /// <summary>$AD:EF37/EF74 copies five back-leg colors after body/brain colors.</summary>
    public const int BackLegCount = 5;
    /// <summary>$AD:EF44/EF81 writes the trailing source word directly to low WRAM $017C, not CGRAM.</summary>
    public const int TrailingWordWram = 0x7e017c;
}
