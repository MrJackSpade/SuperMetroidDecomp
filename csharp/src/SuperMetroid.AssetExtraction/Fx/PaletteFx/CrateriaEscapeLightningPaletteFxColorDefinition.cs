namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One complete late-Crateria escape palette control program.</summary>
public sealed class CrateriaEscapeLightningPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal CrateriaEscapeLightningPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive palette owner.</summary>
    public required CrateriaEscapeLightningPaletteOwner Owner { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
