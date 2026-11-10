using SuperMetroid.AssetExtraction.Samus.Artwork;
namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Installed filename and bounded suit, phase, frame, and palette geometry for selected Samus charge-related body colors.</summary>
public static class SamusChargeColorFormat
{
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
    public const int ColorsPerPalette = SamusColourSets.ColoursPerSet;
}
