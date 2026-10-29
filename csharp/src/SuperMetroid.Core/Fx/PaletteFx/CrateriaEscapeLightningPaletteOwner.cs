namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive late Crateria escape palette program.</summary>
public enum CrateriaEscapeLightningPaletteOwner
{
    /// <summary>PalFxDef_Crateria4 ($8D:FFE9), running the eleven-color yellow-lightning loop from $FE01 over the <see cref="PaletteFxColours.CrateriaEscapeLightning"/>.</summary>
    YellowLightning,
    /// <summary>PalFxDef_Crateria40 ($8D:FFED), running the five-color CRE-block pixel tail from $FF27 over the <see cref="PaletteFxColours.CreBlockPixel"/> with the lightning cadence.</summary>
    CreBlockPixel,
}
