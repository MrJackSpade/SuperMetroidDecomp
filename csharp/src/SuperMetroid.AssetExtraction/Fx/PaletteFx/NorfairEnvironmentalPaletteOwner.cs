using SuperMetroid.Core.Fx.PaletteFx;
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive Norfair environmental palette program.</summary>
public enum NorfairEnvironmentalPaletteOwner
{
    /// <summary>Norfair palette effect 2: updates the <see cref="PaletteFxColours.NorfairHeatForeground"/> and publishes each phase for Samus's separate heat palette.</summary>
    ForegroundAndHeatPhase,
    /// <summary>Norfair palette effect 4: updates the <see cref="PaletteFxColours.NorfairGreenForeground"/> without publishing a heat phase.</summary>
    GreenForeground,
    /// <summary>Norfair palette effect 8: updates the <see cref="PaletteFxColours.NorfairPurpleForeground"/>.</summary>
    PurpleForeground,
    /// <summary>Norfair palette effect 10h: updates the <see cref="PaletteFxColours.NorfairDarkForeground"/> using its own five-color artwork rows.</summary>
    DarkForeground,
}
