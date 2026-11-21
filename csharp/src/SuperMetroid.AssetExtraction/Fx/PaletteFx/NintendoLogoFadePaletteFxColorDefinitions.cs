using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the Nintendo copyright's fade and the colours each writes, for the import.</summary>
/// <remarks>
/// The fade shares its records with the cartridge's boot logo, an effect no screen starts (#1686). Each record
/// brightens the copyright's grey and its blue by the same step; this describes the authored colours, not arithmetic
/// the game performs.
/// </remarks>
public static partial class NintendoLogoFadePaletteFxColorDefinitions
{

    /// <summary>Each record writes two live BGR555 colors.</summary>
    internal const int ColorsPerFrame = 2;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.FadeInNintendoCopyright, ColorsPerFrame);
}
