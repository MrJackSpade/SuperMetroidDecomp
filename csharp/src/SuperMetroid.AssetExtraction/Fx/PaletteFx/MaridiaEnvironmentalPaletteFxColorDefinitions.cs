using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The records of Maridia's sand-pit, sand-fall and background-waterfall effects and the colours each writes, for the
/// import.
/// </summary>
public static partial class MaridiaEnvironmentalPaletteFxColorDefinitions
{
    /// <summary>Four-color sand rotation group.</summary>
    internal const int SandRotationColors = 4;
    /// <summary>Pair of separately rotating sand-pit color bands.</summary>
    internal const int SandPitBandCount = 2;
    /// <summary>Eight-color waterfall rotation group.</summary>
    internal const int WaterfallRotationColors = 8;

    /// <summary>The sand pits' two separately rotating bands.</summary>
    internal static MaridiaEnvironmentalPaletteFxColorDefinition MaridiaSandPits { get; } = new(ColourEffectId.MaridiaSandPits, SandRotationColors * SandPitBandCount);

    /// <summary>The sand falls' rotation.</summary>
    internal static MaridiaEnvironmentalPaletteFxColorDefinition MaridiaSandFalls { get; } = new(ColourEffectId.MaridiaSandFalls, SandRotationColors);

    /// <summary>The background waterfalls' rotation.</summary>
    internal static MaridiaEnvironmentalPaletteFxColorDefinition MaridiaBackgroundWaterfalls { get; } = new(ColourEffectId.MaridiaBackgroundWaterfalls, WaterfallRotationColors);

}
