namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Norfair environmental palette control program.</summary>
public sealed class NorfairEnvironmentalPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal NorfairEnvironmentalPaletteFxColorDefinition()
    {
    }


    /// <summary>The mutually exclusive environmental palette owner.</summary>
    public required NorfairEnvironmentalPaletteOwner Owner { get; init; }
    /// <summary>The palette-FX object that installs this program.</summary>
    public required PaletteFxObject Object { get; init; }
    /// <summary>Whether each record begins with a byte-sized heat-phase publication.</summary>
    public required bool PublishesHeatPhase { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }
}
