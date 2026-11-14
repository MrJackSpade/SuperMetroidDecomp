using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the Zebes explosion's afterglow and lava loops.</summary>
/// <remarks>
/// Issue #846 / #625: the afterglow definition
/// runs <c>SetColorIndex(450)</c>, six 20-byte
/// records of <c>16, colors[8], Wait</c>, then a goto back to its first record
/// (96 frames per loop). All 16 control words match. Its color
/// rows are exactly <c>A,B,C,D,C,B</c>: all 16 words in rows 4 and 5 repeat
/// rows 2 and 1. Rows <c>A</c> through <c>D</c> remain authored data;
/// straight component interpolation from <c>A</c> to <c>D</c> misses 10 of
/// the 16 middle words with floor rounding and 12 with nearest rounding.
/// The presentation compiler supplies all 48 live colors while this catalog
/// supplies only controls to the palette-FX runtime.
/// Issue #847 / #625: the lava definition
/// runs <c>SetColorIndex(128)</c>, ten six-byte records of
/// <c>duration, color, Wait</c>, then a goto back to its first record.
/// For record <c>f</c> (0..9), duration is <c>9 - f</c> for 0..4 and
/// <c>f</c> for 5..9 (70 frames per cycle). Its single red-only BGR555
/// color is <c>18 - 3*f</c> for 0..5 and <c>3*(f - 4)</c> for 6..9.
/// All 24 control words and ten colors match the original program. The presentation
/// compiler still supplies the live colors.
/// </remarks>
public static class ZebesExplosionAmbientPaletteFxColorDefinitions
{
    /// <summary>Fixed afterglow and lava program descriptors in definition order.</summary>
    private static readonly ZebesExplosionAmbientPaletteFxColorDefinition[] Definitions =
    [
        new()
        {
            Owner = ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow,
            Object = ColourEffectId.ZebesExplosionPlanetAfterglow,
            ColorsPerFrame = 8,
            CycleFrames = 96,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.ZebesExplosionPlanetAfterglow, 8),
        },
        new()
        {
            Owner = ZebesExplosionAmbientPaletteFxProgramOwner.Lava,
            Object = ColourEffectId.ExplodingZebesLava,
            ColorsPerFrame = 1,
            CycleFrames = 70,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.ExplodingZebesLava, 1),
        },
    ];

    /// <summary>The afterglow and lava programs in definition order.</summary>
    public static IReadOnlyList<ZebesExplosionAmbientPaletteFxColorDefinition> All { get; } = Array.AsReadOnly(Definitions);
}
