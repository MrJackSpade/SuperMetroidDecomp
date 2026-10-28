namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete early Tourian escape red-flash control program.</summary>
public sealed class TourianEscapeRedFlashPaletteFxColorDefinition
{
    /// <summary>Builds the definition from its owner's program values.</summary>
    internal TourianEscapeRedFlashPaletteFxColorDefinition(TourianEscapeRedFlashPaletteOwner owner, PaletteFxObject paletteFxObject, PaletteFxProgram program, int colorByteIndex, int duration, int colorsPerFrame)
    {
        Owner = owner;
        Object = paletteFxObject;
        ColorByteIndex = colorByteIndex;
        Duration = duration;
        ColorsPerFrame = colorsPerFrame;
        Rows = PaletteFxColorRows.Of(program, colorsPerFrame);
    }

    /// <summary>The mutually exclusive escape-palette owner.</summary>
    public TourianEscapeRedFlashPaletteOwner Owner { get; }

    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public int ColorByteIndex { get; }

    /// <summary>The uniform display duration for every record.</summary>
    public int Duration { get; }

    /// <summary>The number of live BGR555 colors in every record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
