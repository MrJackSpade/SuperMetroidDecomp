namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive Maridia environmental palette program.</summary>
public enum MaridiaEnvironmentalPaletteOwner
{
    /// <summary>$8D:F795, PaletteFXObjects_Maridia1_SandPits: $F4E9 rotates two four-color bands of the <see cref="PaletteFxColours.MaridiaSandPits"/>, with four ten-update phases.</summary>
    SandPits,
    /// <summary>$8D:F799, PaletteFXObjects_Maridia2_SandFalls: $F541 rotates the four <see cref="PaletteFxColours.MaridiaSandFalls"/>, with four ten-update phases.</summary>
    SandFalls,
    /// <summary>$8D:F79D, PaletteFXObjects_Maridia4_BackgroundWaterfalls: $F579 rotates the eight <see cref="PaletteFxColours.MaridiaBackgroundWaterfalls"/>, with eight two-update phases; the operand, not the incorrect native annotation, determines the destination.</summary>
    BackgroundWaterfalls,
}
