namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Maridia environmental palette program.</summary>
public enum MaridiaEnvironmentalPaletteOwner
{
    /// <summary>$8D:F795, PaletteFXObjects_Maridia1_SandPits: $F4E9 rotates two four-color bands in BG palette 2 colors 4..11, with four ten-update phases.</summary>
    SandPits,
    /// <summary>$8D:F799, PaletteFXObjects_Maridia2_SandFalls: $F541 rotates four colors in BG palette 2 colors 8..11, with four ten-update phases.</summary>
    SandFalls,
    /// <summary>$8D:F79D, PaletteFXObjects_Maridia4_BackgroundWaterfalls: $F579 rotates eight colors in BG palette 3 colors 4..11, with eight two-update phases; the operand, not the incorrect native annotation, determines the destination.</summary>
    BackgroundWaterfalls,
}
