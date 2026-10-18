namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete early Tourian escape red-flash control program.</summary>
public sealed class TourianEscapeRedFlashPaletteFxColorDefinition
{
    /// <summary>Binds one escape palette owner to its object, first record and uniform timed-record geometry.</summary>
    internal TourianEscapeRedFlashPaletteFxColorDefinition(
        TourianEscapeRedFlashPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        int firstRecord,
        ushort colorByteIndex,
        ushort duration,
        int colorsPerFrame)
    {
        Owner = owner;
        Object = paletteFxObject;
        Rows = new(firstRecord, TourianEscapeRedFlashPaletteFxColorDefinitions.FrameCount, colorsPerFrame);
        ColorByteIndex = colorByteIndex;
        Duration = duration;
        ColorsPerFrame = colorsPerFrame;
    }

    /// <summary>The mutually exclusive escape-palette owner.</summary>
    public TourianEscapeRedFlashPaletteOwner Owner { get; }

    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The uniform display duration for every record.</summary>
    public ushort Duration { get; }

    /// <summary>The number of live BGR555 colors in every record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => sizeof(ushort) * (ColorsPerFrame + 2);

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
