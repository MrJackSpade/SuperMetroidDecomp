namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One cinematic glow's records and the colours each writes.</summary>
public sealed class CinematicGlowPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal CinematicGlowPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive glow owner.</summary>
    public required CinematicGlowPaletteFxProgramOwner Owner { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
