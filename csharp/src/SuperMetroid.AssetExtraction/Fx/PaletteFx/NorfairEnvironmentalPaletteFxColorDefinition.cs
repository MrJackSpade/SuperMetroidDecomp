namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One Norfair environmental effect's records and the colours each writes.</summary>
public sealed class NorfairEnvironmentalPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal NorfairEnvironmentalPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive environmental palette owner.</summary>
    public required NorfairEnvironmentalPaletteOwner Owner { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
