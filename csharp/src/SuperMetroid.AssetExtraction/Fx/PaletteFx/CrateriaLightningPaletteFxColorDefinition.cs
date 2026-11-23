namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The Landing Site lightning's records and the colours each writes.</summary>
public sealed partial class CrateriaLightningPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal CrateriaLightningPaletteFxColorDefinition()
    {
    }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
