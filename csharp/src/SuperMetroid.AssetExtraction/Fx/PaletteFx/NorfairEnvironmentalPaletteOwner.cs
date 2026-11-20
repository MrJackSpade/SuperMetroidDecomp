using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive Norfair environmental palette program.</summary>
public enum NorfairEnvironmentalPaletteOwner
{
    /// <summary>The heated foreground: updates the <see cref="ColourEffectColours.NorfairHeatForeground"/> and publishes each phase for Samus's separate heat palette.</summary>
    ForegroundAndHeatPhase,
    /// <summary>The green foreground: updates the <see cref="ColourEffectColours.NorfairGreenForeground"/> without publishing a heat phase.</summary>
    GreenForeground,
    /// <summary>The purple foreground: updates the <see cref="ColourEffectColours.NorfairPurpleForeground"/>.</summary>
    PurpleForeground,
    /// <summary>The dark foreground: updates the <see cref="ColourEffectColours.NorfairDarkForeground"/> using its own five-color artwork rows.</summary>
    DarkForeground,
}
