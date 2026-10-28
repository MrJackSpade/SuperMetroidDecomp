namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete old-Tourian escape accent control program.</summary>
public sealed class OldTourianEscapeAccentPaletteFxColorDefinition
{
    /// <summary>Builds the definition from its owner's program values.</summary>
    internal OldTourianEscapeAccentPaletteFxColorDefinition(OldTourianEscapeAccentPaletteOwner owner, PaletteFxObject paletteFxObject, PaletteFxProgram program, int colorByteIndex) =>
        (Owner, Object, ColorByteIndex, Rows) = (owner, paletteFxObject, colorByteIndex, PaletteFxColorRows.Of(program, OldTourianEscapeAccentPaletteFxColorDefinitions.ColorsPerFrame));

    /// <summary>The mutually exclusive escape-accent owner.</summary>
    public OldTourianEscapeAccentPaletteOwner Owner { get; }

    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public int ColorByteIndex { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
