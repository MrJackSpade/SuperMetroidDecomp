namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Norfair environmental palette control program.</summary>
public sealed class NorfairEnvironmentalPaletteFxColorDefinition
{

    /// <summary>Captures the object, first record and destination for one environmental loop.</summary>
    internal NorfairEnvironmentalPaletteFxColorDefinition(
        NorfairEnvironmentalPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        int firstRecord,
        ushort colorByteIndex,
        bool publishesHeatPhase)
    {
        Owner = owner;
        Object = paletteFxObject;
        Rows = new(firstRecord, NorfairEnvironmentalPaletteFxColorDefinitions.FrameCount, NorfairEnvironmentalPaletteFxColorDefinitions.ColorsPerFrame);
        ColorByteIndex = colorByteIndex;
        PublishesHeatPhase = publishesHeatPhase;
    }

    /// <summary>The mutually exclusive environmental palette owner.</summary>
    public NorfairEnvironmentalPaletteOwner Owner { get; }
    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }
    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }
    /// <summary>Whether each record begins with a byte-sized heat-phase publication.</summary>
    public bool PublishesHeatPhase { get; }
    /// <summary>Bytes from one record's first mechanic through its wait command.</summary>
    public int FrameByteCount => PublishesHeatPhase ? 19 : 16;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
