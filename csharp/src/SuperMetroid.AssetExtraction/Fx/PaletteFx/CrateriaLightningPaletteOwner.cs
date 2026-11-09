using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive Crateria lightning palette program.</summary>
public enum CrateriaLightningPaletteOwner
{
    /// <summary>Crateria palette effect 1 lightning: flashes the <see cref="PaletteFxColours.LandingSiteLightning"/> at the Landing Site before power bombs are acquired.</summary>
    SurfaceLightning,
    /// <summary>The unused dark-lightning counterpart: darkens the seven <see cref="PaletteFxColours.UnusedDarkLightning"/>.</summary>
    UnusedDarkLightning,
}
