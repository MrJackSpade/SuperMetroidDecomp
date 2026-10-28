namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One Torizo-specific entry and six-frame belly palette loop.</summary>
public sealed class TorizoBellyPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal TorizoBellyPaletteFxColorDefinition()
    {
    }

    /// <summary>The Torizo variant represented by this program.</summary>
    public required TorizoBellyPaletteOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    public required PaletteFxObject Object { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }
}
