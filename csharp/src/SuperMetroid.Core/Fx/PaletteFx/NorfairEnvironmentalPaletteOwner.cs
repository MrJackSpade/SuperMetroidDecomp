namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive Norfair environmental palette program.</summary>
public enum NorfairEnvironmentalPaletteOwner
{
    /// <summary>Norfair palette effect 2: updates the <see cref="PaletteFxColours.NorfairHeatForeground"/> and publishes each phase for Samus's separate heat palette.</summary>
    ForegroundAndHeatPhase,
    /// <summary>Norfair palette effect 4: updates the <see cref="PaletteFxColours.NorfairForegroundPalette4"/> without publishing a heat phase.</summary>
    ForegroundPalette4,
    /// <summary>Norfair palette effect 8: updates the <see cref="PaletteFxColours.NorfairForegroundPalette5"/>.</summary>
    ForegroundPalette5,
    /// <summary>Norfair palette effect 10h: updates the <see cref="PaletteFxColours.NorfairForegroundPalette6"/> using its own five-color artwork rows.</summary>
    ForegroundPalette6,
}
