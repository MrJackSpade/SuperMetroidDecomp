using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive Maridia environmental palette program.</summary>
public enum MaridiaEnvironmentalPaletteOwner
{
    /// <summary>The sand pits: rotates two four-color bands of the <see cref="ColourEffectColours.MaridiaSandPits"/>, with four ten-update phases.</summary>
    SandPits,
    /// <summary>The sand falls: rotates the four <see cref="ColourEffectColours.MaridiaSandFalls"/>, with four ten-update phases.</summary>
    SandFalls,
    /// <summary>The background waterfalls: rotates the eight <see cref="ColourEffectColours.MaridiaBackgroundWaterfalls"/>, with eight two-update phases; the operand, not the effect's traditional description, determines the destination.</summary>
    BackgroundWaterfalls,
}
