using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>Immutable mechanics for the looping title-screen ambient palettes.</summary>
/// <remarks>
/// Issue #853 / #625: the tube-light definition
/// runs <c>SetColorIndex(84)</c>, eight 12-byte
/// records of <c>10, colors[4], Wait</c>, then a goto back to its first record
/// (80 frames per loop). All 20 control words match. The
/// first frame reuses the initial title palette. Tube channels interpolate to their
/// clamped endpoint using the minimum phase count at the selected three-unit
/// scale and nearest/even integer quantization; no green exception is needed.
/// Issue #854 / #625: the displays' definition
/// sets color index 92, and alternates two
/// eight-byte records of <c>1, colors[2], Wait</c>;
/// a goto repeats the two-frame cycle. All
/// eight control words match. Even records write <c>[13FF,0BB1]</c> and
/// odd records write <c>[00AC,0145]</c>. These frames reuse the initial
/// title palette's material paint through named bindings and calculated shading.
/// The initial palette's independent paint values remain separately required
/// under TitlePalettePresentation.colors; each supplied initial or animated
/// edit preserves every other installed value. Missing inputs report an error
/// rather than falling back to reading the original game data.
/// </remarks>
public static class TitleScreenAmbientPaletteFxColorDefinitions
{
    /// <summary>Four dimming steps and their reflected return form the tube pulse. This selected pulse depth is authored title choreography.</summary>
    internal const int TubeDimmingSteps = 4;
    /// <summary>Bright and dim display states alternate on consecutive palette updates.</summary>
    internal const int DisplayStates = 2;
    /// <summary>The tube light's four material colours.</summary>
    internal const int TubeColourCount = 4;
    /// <summary>The displays' two material colours, just after the tube's.</summary>
    internal const int DisplayColourCount = 2;
    /// <summary>The eight-frame baby-Metroid tube light.</summary>
    public static TitleScreenAmbientPaletteFxColorDefinition TubeLight { get; } =
        new()
        {
            Owner = TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight,
            Colours = ColourEffectColours.BabyMetroidTubeLight,
            FrameCount = 2 * TubeDimmingSteps,
            ColorsPerFrame = TubeColourCount,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.TitleScreenBabyMetroidTubeLight, TubeColourCount),
        };
    /// <summary>The two-frame flickering displays.</summary>
    public static TitleScreenAmbientPaletteFxColorDefinition Displays { get; } =
        new()
        {
            Owner = TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays,
            Colours = ColourEffectColours.TitleScreenDisplays,
            FrameCount = DisplayStates,
            ColorsPerFrame = DisplayColourCount,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.TitleScreenFlickeringDisplays, DisplayColourCount),
        };
}
