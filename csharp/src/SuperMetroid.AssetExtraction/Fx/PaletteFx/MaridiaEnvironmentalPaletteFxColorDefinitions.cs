using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// Immutable control words for Maridia's sand and background-waterfall palette loops.
/// </summary>
/// <remarks>
/// The sand-pit, sand-fall and background-waterfall definitions own 112 colour
/// words. Those colors remain presentation data; this catalog owns only color-index
/// setup, durations, waits, and loop control.
/// Sand pits start at colour byte 72 and run four ten-frame records,
/// then loop: a 40-frame loop.
/// Sand falls start at colour byte 80 and run four ten-frame records,
/// then loop: another 40-frame loop.
/// Background waterfalls start at colour byte 104 and run eight two-frame records,
/// then loop: a 16-frame loop.
/// Each record ends in a wait; each goto targets its first record.
/// The next index in each program reaches control, not presentation data.
/// All 44 mechanics words match the original programs.
/// </remarks>
public static class MaridiaEnvironmentalPaletteFxColorDefinitions
{
    /// <summary>Four-color sand rotation group.</summary>
    internal const int SandRotationColors = 4;
    /// <summary>Pair of separately rotating sand-pit color bands.</summary>
    internal const int SandPitBandCount = 2;
    /// <summary>Eight-color waterfall rotation group.</summary>
    internal const int WaterfallRotationColors = 8;

    /// <summary>The sand-pit, sand-fall, and waterfall programs in definition order, calculated without cached records.</summary>
    public static IReadOnlyList<MaridiaEnvironmentalPaletteFxColorDefinition> All { get; } =
    [
        new()
        {
            Owner = MaridiaEnvironmentalPaletteOwner.SandPits,
            FrameCount = SandRotationColors,
            ColorsPerFrame = SandRotationColors * SandPitBandCount,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.MaridiaSandPitsCycle, SandRotationColors * SandPitBandCount),
        },
        new()
        {
            Owner = MaridiaEnvironmentalPaletteOwner.SandFalls,
            FrameCount = SandRotationColors,
            ColorsPerFrame = SandRotationColors,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.MaridiaSandFallsCycle, SandRotationColors),
        },
        new()
        {
            Owner = MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls,
            FrameCount = WaterfallRotationColors,
            ColorsPerFrame = WaterfallRotationColors,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.MaridiaBackgroundWaterfallsCycle, WaterfallRotationColors),
        },
    ];

}
