namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Calculated setup, two repeated flash groups and neutral intervals of the Landing Site's lightning.</summary>
public sealed class CrateriaLightningPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal CrateriaLightningPaletteFxColorDefinition()
    {
    }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
