using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive Norfair environmental palette program.</summary>
public enum NorfairEnvironmentalPaletteOwner
{
    /// <summary>Norfair palette effect 2: updates the <see cref="ColourEffectColours.NorfairHeatForeground"/> and publishes each phase for Samus's separate heat palette.</summary>
    ForegroundAndHeatPhase,
    /// <summary>Norfair palette effect 4: updates the <see cref="ColourEffectColours.NorfairGreenForeground"/> without publishing a heat phase.</summary>
    GreenForeground,
    /// <summary>Norfair palette effect 8: updates the <see cref="ColourEffectColours.NorfairPurpleForeground"/>.</summary>
    PurpleForeground,
    /// <summary>Norfair palette effect 10h: updates the <see cref="ColourEffectColours.NorfairDarkForeground"/> using its own five-color artwork rows.</summary>
    DarkForeground,
}
