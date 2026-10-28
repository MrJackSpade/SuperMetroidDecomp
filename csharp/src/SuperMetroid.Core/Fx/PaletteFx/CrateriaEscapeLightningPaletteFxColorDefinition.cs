namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete late-Crateria escape palette control program.</summary>
public sealed class CrateriaEscapeLightningPaletteFxColorDefinition
{
    /// <summary>Builds the definition from its owner's program values.</summary>
    internal CrateriaEscapeLightningPaletteFxColorDefinition(CrateriaEscapeLightningPaletteOwner owner, PaletteFxObject paletteFxObject, PaletteFxProgram program, int colorByteIndex, int colorsPerFrame) =>
        (Owner, Object, ColorByteIndex, ColorsPerFrame, Rows) = (owner, paletteFxObject, colorByteIndex, colorsPerFrame, PaletteFxColorRows.Of(program, colorsPerFrame));

    /// <summary>The mutually exclusive palette owner.</summary>
    public CrateriaEscapeLightningPaletteOwner Owner { get; }
    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }
    /// <summary>The first destination byte in CGRAM.</summary>
    public int ColorByteIndex { get; }
    /// <summary>The number of live BGR555 colors in every record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
