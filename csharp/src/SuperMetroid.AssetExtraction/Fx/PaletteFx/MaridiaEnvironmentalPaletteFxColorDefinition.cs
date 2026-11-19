
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One complete Maridia environmental palette control program.</summary>
public sealed class MaridiaEnvironmentalPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal MaridiaEnvironmentalPaletteFxColorDefinition()
    {
    }

    /// <summary>The environmental animation represented by this program.</summary>
    public required MaridiaEnvironmentalPaletteOwner Owner { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
