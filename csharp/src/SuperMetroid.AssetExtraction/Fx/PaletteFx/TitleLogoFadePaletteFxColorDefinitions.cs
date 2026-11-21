using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the Super Metroid title logo's fade-in and the colours each writes, for the import.</summary>
/// <remarks>
/// The records brighten from black to the logo's final colours in equal steps on each channel; this describes the
/// authored colours, not arithmetic the game performs.
/// </remarks>
public static class TitleLogoFadePaletteFxColorDefinitions
{

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    internal const int ColorsPerFrame = 15;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.FadeInSuperMetroidTitleLogo, ColorsPerFrame);
}
