namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive Norfair environmental palette program.</summary>
public enum NorfairEnvironmentalPaletteOwner
{
    /// <summary>$8D:F785, Norfair 2: InstList_PaletteFXObject_Norfair2_0 at $F08E updates BG palette 3 and publishes each phase through $F1C6 for Samus's separate heat palette.</summary>
    ForegroundAndHeatPhase,
    /// <summary>$8D:F789, Norfair 4: InstList_PaletteFXObject_Norfair4_0 at $F1D1 updates BG palette 4 from color 1 without publishing a heat phase.</summary>
    ForegroundPalette4,
    /// <summary>$8D:F78D, Norfair 8: InstList_PaletteFXObject_Norfair8_0 at $F2D9 updates BG palette 5 from color 1; the native 8 names the definition, not the palette number.</summary>
    ForegroundPalette5,
    /// <summary>$8D:F791, Norfair 10h: InstList_PaletteFXObject_Norfair10_0 at $F3E1 updates BG palette 6 from color 1 using its own five-color artwork rows.</summary>
    ForegroundPalette6,
}
