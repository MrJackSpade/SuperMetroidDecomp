namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete early Tourian escape red-flash control program.</summary>
public sealed class TourianEscapeRedFlashPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal TourianEscapeRedFlashPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive escape-palette owner.</summary>
    public required TourianEscapeRedFlashPaletteOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    public required PaletteFxObject Object { get; init; }


    /// <summary>The uniform display duration for every record.</summary>
    public required int Duration { get; init; }

    /// <summary>The number of live BGR555 colors in every record.</summary>
    public required int ColorsPerFrame { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }
}
