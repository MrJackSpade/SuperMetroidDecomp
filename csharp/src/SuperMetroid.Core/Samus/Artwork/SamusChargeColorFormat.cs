using SuperMetroid.Core.Samus.Palettes;


namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Installed filename and bounded suit, phase, frame, and palette geometry for selected Samus charge-related body colors.</summary>
public static class SamusChargeColorFormat
{
    /// <summary>Returns the first phase using a native charge palette.</summary>
    /// <remarks>Phase0..5: charge mirrors around3; pseudo-Screw holds each
    /// input for three phases. Original pointers91D7DB..D827 establish exact
    /// row identity for all three suits. No extrapolation or invalid masking.</remarks>
    internal static int CanonicalPhase(bool pseudo, int phase)
    {
        if ((uint)phase >= PhasesPerSuit) throw new ArgumentOutOfRangeException(nameof(phase));
        return pseudo ? phase / 3 * 3 : Math.Min(phase, 6 - phase);
    }
    /// <summary>Calculates the bright pseudo-Screw shade from the corresponding normal ink.</summary>
    /// <remarks>Native91D805/D811/D81D select active-shinespark final rows
    /// 9B9C80/9E80/A080. Opaque gold inks reuse the owning full-body gold
    /// operation at shade3; other inks use the shared warm tint (RG+26,B+10),
    /// saturating each channel at31. These also cover common secondary-suit
    /// inks represented by Power aliases in the full-body view. Gravity's
    /// different base1/12 RG inputs both saturate here,so its normal-row RGB
    /// produces the identical final color. Power/Varia ink2 blue differs and
    /// remains independently supplied; all matching channels are calculated.</remarks>
    internal static Bgr555 PseudoScrewBrightColor(int suit, int color, Bgr555 basis)
    {
        if ((uint)suit >= SuitCount) throw new ArgumentOutOfRangeException(nameof(suit));
        if (color is < 1 or >= ColorsPerPalette) throw new ArgumentOutOfRangeException(nameof(color));
        return SamusFullBodyCycleColorFormat.TryActiveGoldRamp(suit * 16 + 11, color, basis, out Bgr555 gold)
            ? gold : SamusFullBodyCycleColorFormat.ActiveShineTint(basis, 3);
    }
    /// <summary>Installed editable JSON filename for charged-beam, pseudo-Screw, and Hyper-shot palette rows.</summary>
    public const string FileName = "samus-charge-colors.json";
    /// <summary>Supported color schema revision, requiring the complete suit/phase and Hyper-shot row sets.</summary>
    public const int Version = 1;
    /// <summary>Three ordered suit variants: Power, Varia, and Gravity.</summary>
    public const int SuitCount = 3;
    /// <summary>Six playback phases per suit, matching native charge lists $91:D7DB/$D7E7/$D7F3 and pseudo-Screw lists $D805/$D811/$D81D.</summary>
    public const int PhasesPerSuit = 6;
    /// <summary>Ten Hyper-shot playback frames in the native $91:D82B–$D83D reverse palette sequence.</summary>
    public const int HyperFrameCount = 10;
    /// <summary>Sixteen RGB5 words per Samus OBJ palette row, including the retained transparent-slot word and fifteen visible inks.</summary>
    public const int ColorsPerPalette = SamusPaletteDefinitions.Common.ColorsPerObjPalette;
}
