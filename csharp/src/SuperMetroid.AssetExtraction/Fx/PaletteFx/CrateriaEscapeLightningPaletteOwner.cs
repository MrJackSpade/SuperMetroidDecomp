using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive late Crateria escape palette program.</summary>
public enum CrateriaEscapeLightningPaletteOwner
{
    /// <summary>The escape's yellow lightning, running the eleven-color yellow-lightning loop over the <see cref="ColourEffectColours.CrateriaEscapeLightning"/>.</summary>
    YellowLightning,
    /// <summary>The Parlor's escape sky, running the lightning's five-color dark tail over the <see cref="ColourEffectColours.ParlorEscapeSky"/> with the lightning cadence.</summary>
    ParlorSky,
}
