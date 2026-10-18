namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete old-Tourian escape accent control program.</summary>
public sealed class OldTourianEscapeAccentPaletteFxColorDefinition
{
    /// <summary>Records one accent destination, its object and first record.</summary>
    internal OldTourianEscapeAccentPaletteFxColorDefinition(
        OldTourianEscapeAccentPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        int firstRecord,
        ushort colorByteIndex)
    {
        Owner = owner;
        Object = paletteFxObject;
        Rows = new(firstRecord, OldTourianEscapeAccentPaletteFxColorDefinitions.FrameCount, OldTourianEscapeAccentPaletteFxColorDefinitions.ColorsPerFrame);
        ColorByteIndex = colorByteIndex;
    }

    /// <summary>The mutually exclusive escape-accent owner.</summary>
    public OldTourianEscapeAccentPaletteOwner Owner { get; }

    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
