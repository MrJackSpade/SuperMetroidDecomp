using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One complete Samus-loading palette control program.</summary>
public sealed class SamusLoadingSuitPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal SamusLoadingSuitPaletteFxColorDefinition()
    {
    }


    /// <summary>The mutually exclusive suit palette.</summary>
    public required SamusLoadingSuitPaletteFxProgramOwner Owner { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
