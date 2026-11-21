using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One escape lightning effect's records and the colours each writes.</summary>
public sealed class CrateriaEscapeLightningPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions: the records <paramref name="effect"/> shows, each writing <paramref name="colorsPerFrame"/> colours.</summary>
    /// <param name="effect">The colour effect whose records the program holds.</param>
    /// <param name="colorsPerFrame">The colours each record writes.</param>
    internal CrateriaEscapeLightningPaletteFxColorDefinition(ColourEffectId effect, int colorsPerFrame)
    {
        Effect = effect;
        ColorFrames = PaletteFxColorFrames.Of(effect, colorsPerFrame);
    }

    /// <summary>The colour effect whose records the program holds, which identifies the definition.</summary>
    public ColourEffectId Effect { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorFrames ColorFrames { get; }
}
