namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive Norfair environmental palette program.</summary>
public enum NorfairEnvironmentalPaletteOwner
{
    /// <summary>$8D:F785, Norfair 2: InstList_PaletteFXObject_Norfair2_0 at $F08E updates the <see cref="PaletteFxColours.NorfairHeatForeground"/> and publishes each phase through $F1C6 for Samus's separate heat palette.</summary>
    ForegroundAndHeatPhase,
    /// <summary>$8D:F789, Norfair 4: InstList_PaletteFXObject_Norfair4_0 at $F1D1 updates the <see cref="PaletteFxColours.NorfairForegroundPalette4"/> without publishing a heat phase.</summary>
    ForegroundPalette4,
    /// <summary>$8D:F78D, Norfair 8: InstList_PaletteFXObject_Norfair8_0 at $F2D9 updates the <see cref="PaletteFxColours.NorfairForegroundPalette5"/>.</summary>
    ForegroundPalette5,
    /// <summary>$8D:F791, Norfair 10h: InstList_PaletteFXObject_Norfair10_0 at $F3E1 updates the <see cref="PaletteFxColours.NorfairForegroundPalette6"/> using its own five-color artwork rows.</summary>
    ForegroundPalette6,
}
