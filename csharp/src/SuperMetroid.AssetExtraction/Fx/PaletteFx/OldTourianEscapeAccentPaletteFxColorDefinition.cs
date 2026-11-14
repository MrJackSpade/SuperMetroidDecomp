using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One complete old-Tourian escape accent control program.</summary>
public sealed class OldTourianEscapeAccentPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal OldTourianEscapeAccentPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive escape-accent owner.</summary>
    public required OldTourianEscapeAccentPaletteOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    public required ColourEffectId Object { get; init; }


    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
