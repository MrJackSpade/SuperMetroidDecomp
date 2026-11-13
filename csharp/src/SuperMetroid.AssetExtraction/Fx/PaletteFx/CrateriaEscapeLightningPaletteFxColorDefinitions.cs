using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for Crateria's paired late-escape palette loops.</summary>
/// <remarks>
/// The two definitions share an eleven-record duration schedule.
/// Their 176 BGR555 words remain live presentation data; this catalog owns palette
/// placement, timing, waits, and loop control.
/// </remarks>
public static class CrateriaEscapeLightningPaletteFxColorDefinitions
{

    /// <summary>The yellow-lightning program targets eleven colors from colour byte $A2.</summary>
    private static readonly CrateriaEscapeLightningPaletteFxColorDefinition YellowLightning = new()
    {
        Owner = CrateriaEscapeLightningPaletteOwner.YellowLightning,
        Object = PaletteFxObject.CrateriaEscapeYellowLightning,
        ColorsPerFrame = 11,
        ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.CrateriaEscapeYellowLightningCycle, 11),
    };
    /// <summary>The Parlor's sky program targets the lightning's five-color dark tail from colour byte $AE.</summary>
    private static readonly CrateriaEscapeLightningPaletteFxColorDefinition ParlorSky = new()
    {
        Owner = CrateriaEscapeLightningPaletteOwner.ParlorSky,
        Object = PaletteFxObject.ParlorEscapeSkyFlicker,
        ColorsPerFrame = 5,
        ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.ParlorEscapeSkyFlickerCycle, 5),
    };
    /// <summary>The yellow-lightning and Parlor sky programs in definition order.</summary>
    public static IReadOnlyList<CrateriaEscapeLightningPaletteFxColorDefinition> All { get; } = [YellowLightning, ParlorSky];

}
