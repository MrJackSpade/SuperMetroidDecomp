using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive late Crateria escape palette program.</summary>
public enum CrateriaEscapeLightningPaletteOwner
{
    /// <summary>Crateria palette effect 4, running the eleven-color yellow-lightning loop over the <see cref="ColourEffectColours.CrateriaEscapeLightning"/>.</summary>
    YellowLightning,
    /// <summary>Crateria palette effect 40, running the lightning's five-color dark tail over the <see cref="ColourEffectColours.ParlorEscapeSky"/> with the lightning cadence.</summary>
    ParlorSky,
}
