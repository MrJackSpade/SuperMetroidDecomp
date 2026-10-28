namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete old-Tourian escape accent control program.</summary>
public sealed class OldTourianEscapeAccentPaletteFxColorDefinition(
        OldTourianEscapeAccentPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        PaletteFxProgram program,
        int colorByteIndex)
{
    /// <summary>The mutually exclusive escape-accent owner.</summary>
    public OldTourianEscapeAccentPaletteOwner Owner { get; } = owner;

    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; } = paletteFxObject;

    /// <summary>The first destination byte in CGRAM.</summary>
    public int ColorByteIndex { get; } = colorByteIndex;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(program, OldTourianEscapeAccentPaletteFxColorDefinitions.ColorsPerFrame);
}
