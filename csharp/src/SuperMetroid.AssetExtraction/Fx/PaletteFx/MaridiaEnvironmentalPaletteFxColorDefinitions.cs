using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The records of Maridia's sand-pit, sand-fall and background-waterfall effects and the colours each writes, for the
/// import.
/// </summary>
public static class MaridiaEnvironmentalPaletteFxColorDefinitions
{
    /// <summary>Four-color sand rotation group.</summary>
    private const int SandRotationColors = 4;
    /// <summary>Pair of separately rotating sand-pit color bands.</summary>
    private const int SandPitBandCount = 2;
    /// <summary>Eight-color waterfall rotation group.</summary>
    private const int WaterfallRotationColors = 8;

    /// <summary>The sand-pit, sand-fall, and waterfall programs in definition order, calculated without cached records.</summary>
    public static IReadOnlyList<MaridiaEnvironmentalPaletteFxColorDefinition> All { get; } =
    [
        new()
        {
            Owner = MaridiaEnvironmentalPaletteOwner.SandPits,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.MaridiaSandPits, SandRotationColors * SandPitBandCount),
        },
        new()
        {
            Owner = MaridiaEnvironmentalPaletteOwner.SandFalls,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.MaridiaSandFalls, SandRotationColors),
        },
        new()
        {
            Owner = MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.MaridiaBackgroundWaterfalls, WaterfallRotationColors),
        },
    ];

}
