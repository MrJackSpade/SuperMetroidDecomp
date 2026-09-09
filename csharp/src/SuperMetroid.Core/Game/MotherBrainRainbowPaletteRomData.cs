namespace SuperMetroid.Core.Game;

/// <summary>Native rainbow palette sources and CGRAM destinations for Mother Brain.</summary>
public static class MotherBrainRainbowPaletteRomData
{
    /// <summary>$AD:E434, zero-terminated pointer list consumed by $A9:BCFD.</summary>
    public const int PointerTable = 0xade434;
    /// <summary>$A9:BD1D reads each two-palette payload from bank $AD.</summary>
    public const int SourceBank = 0xad0000;
    /// <summary>$A9:BD1D copies fifteen nontransparent colors per destination.</summary>
    public const int ColorCount = 15;
    /// <summary>$A9:BD1D byte destination $82, body background colors.</summary>
    public const int BodyColor = 0x41;
    /// <summary>$A9:BD1D byte destination $122, brain and neck sprite colors.</summary>
    public const int BrainColor = 0x91;
    /// <summary>$A9:BD1D byte destination $162, rear-leg sprite palette.</summary>
    public const int SecondaryColor = 0xb1;
    /// <summary>$A9:BCCE restores brain colors from $A9:9474, skipping transparent color zero.</summary>
    public const int NormalBrainSource = 0xa99474;
    /// <summary>$A9:BCCE restores secondary colors from $A9:9494.</summary>
    public const int NormalSecondarySource = 0xa99494;
    /// <summary>$A9:BCF6 selects entry six when the Baby exhausts the rainbow drain.</summary>
    public const int DrainedPointerOffset = 12;
}
