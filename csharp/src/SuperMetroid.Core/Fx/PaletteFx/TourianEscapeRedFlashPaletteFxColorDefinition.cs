namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete early Tourian escape red-flash control program.</summary>
public sealed class TourianEscapeRedFlashPaletteFxColorDefinition(
        TourianEscapeRedFlashPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        PaletteFxProgram program,
        int colorByteIndex,
        int duration,
        int colorsPerFrame)
{
    /// <summary>The mutually exclusive escape-palette owner.</summary>
    public TourianEscapeRedFlashPaletteOwner Owner { get; } = owner;

    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; } = paletteFxObject;

    /// <summary>The first destination byte in CGRAM.</summary>
    public int ColorByteIndex { get; } = colorByteIndex;

    /// <summary>The uniform display duration for every record.</summary>
    public int Duration { get; } = duration;

    /// <summary>The number of live BGR555 colors in every record.</summary>
    public int ColorsPerFrame { get; } = colorsPerFrame;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(program, colorsPerFrame);
}
