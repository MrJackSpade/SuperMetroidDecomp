namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Norfair environmental palette control program.</summary>
public sealed class NorfairEnvironmentalPaletteFxColorDefinition
{

    /// <summary>Captures the object, first record and destination for one environmental loop.</summary>
    internal NorfairEnvironmentalPaletteFxColorDefinition(
        NorfairEnvironmentalPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        PaletteFxProgram program,
        int colorByteIndex,
        bool publishesHeatPhase)
    {
        Owner = owner;
        Object = paletteFxObject;
        Rows = PaletteFxColorRows.Of(program, NorfairEnvironmentalPaletteFxColorDefinitions.ColorsPerFrame);
        ColorByteIndex = colorByteIndex;
        PublishesHeatPhase = publishesHeatPhase;
    }

    /// <summary>The mutually exclusive environmental palette owner.</summary>
    public NorfairEnvironmentalPaletteOwner Owner { get; }
    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }
    /// <summary>The first destination byte in CGRAM.</summary>
    public int ColorByteIndex { get; }
    /// <summary>Whether each record begins with a byte-sized heat-phase publication.</summary>
    public bool PublishesHeatPhase { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
