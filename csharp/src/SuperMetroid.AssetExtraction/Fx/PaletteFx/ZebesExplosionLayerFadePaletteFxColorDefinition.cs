namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One exploding-Zebes layer fade's records and the colours each writes.</summary>
public sealed class ZebesExplosionLayerFadePaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal ZebesExplosionLayerFadePaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive faded layer.</summary>
    public required ZebesExplosionLayerFadePaletteFxProgramOwner Owner { get; init; }


    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
