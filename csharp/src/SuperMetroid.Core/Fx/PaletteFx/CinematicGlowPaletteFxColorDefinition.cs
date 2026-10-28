namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete cinematic glow control program.</summary>
public sealed class CinematicGlowPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal CinematicGlowPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive glow owner.</summary>
    public required CinematicGlowPaletteFxProgramOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks>
    /// <c>$8D:E1BC</c> owns the old Mother Brain lights and <c>$8D:E1C0</c> the gunship.
    /// </remarks>
    public required PaletteFxObject Object { get; init; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public required int ColorByteIndex { get; init; }

    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public required int ColorsPerFrame { get; init; }

    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public required int FrameDuration { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }
}
