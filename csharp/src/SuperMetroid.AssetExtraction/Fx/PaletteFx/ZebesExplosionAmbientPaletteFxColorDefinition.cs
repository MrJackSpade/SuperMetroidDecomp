namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One complete persistent Zebes-explosion ambient palette loop.</summary>
public sealed class ZebesExplosionAmbientPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal ZebesExplosionAmbientPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive ambient owner.</summary>
    public required ZebesExplosionAmbientPaletteFxProgramOwner Owner { get; init; }


    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
