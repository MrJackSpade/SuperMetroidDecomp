namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete old-Tourian escape accent control program.</summary>
public sealed class OldTourianEscapeAccentPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal OldTourianEscapeAccentPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive escape-accent owner.</summary>
    public required OldTourianEscapeAccentPaletteOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    public required PaletteFxObject Object { get; init; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public required int ColorByteIndex { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }
}
