using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One complete early Tourian escape red-flash control program.</summary>
public sealed class TourianEscapeRedFlashPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal TourianEscapeRedFlashPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive escape-palette owner.</summary>
    public required TourianEscapeRedFlashPaletteOwner Owner { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
