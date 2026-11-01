using SuperMetroid.Core.Fx.PaletteFx;
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive late Crateria escape palette program.</summary>
public enum CrateriaEscapeLightningPaletteOwner
{
    /// <summary>Crateria palette effect 4, running the eleven-color yellow-lightning loop over the <see cref="PaletteFxColours.CrateriaEscapeLightning"/>.</summary>
    YellowLightning,
    /// <summary>Crateria palette effect 40, running the five-color CRE-block pixel tail over the <see cref="PaletteFxColours.CreBlockPixel"/> with the lightning cadence.</summary>
    CreBlockPixel,
}
