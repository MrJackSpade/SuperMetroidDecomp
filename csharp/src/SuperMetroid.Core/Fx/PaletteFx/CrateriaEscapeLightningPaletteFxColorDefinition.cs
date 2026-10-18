namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete late-Crateria escape palette control program.</summary>
public sealed class CrateriaEscapeLightningPaletteFxColorDefinition
{
    /// <summary>Stores a program's object, first record, destination, and color-row width.</summary>
    internal CrateriaEscapeLightningPaletteFxColorDefinition(
        CrateriaEscapeLightningPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        int firstRecord,
        ushort colorByteIndex,
        int colorsPerFrame)
    {
        Owner = owner;
        Object = paletteFxObject;
        Rows = new(firstRecord, CrateriaEscapeLightningPaletteFxColorDefinitions.FrameCount, colorsPerFrame);
        ColorByteIndex = colorByteIndex;
        ColorsPerFrame = colorsPerFrame;
    }

    /// <summary>The mutually exclusive palette owner.</summary>
    public CrateriaEscapeLightningPaletteOwner Owner { get; }
    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }
    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }
    /// <summary>The number of live BGR555 colors in every record.</summary>
    public int ColorsPerFrame { get; }
    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => sizeof(ushort) * (ColorsPerFrame + 2);

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
