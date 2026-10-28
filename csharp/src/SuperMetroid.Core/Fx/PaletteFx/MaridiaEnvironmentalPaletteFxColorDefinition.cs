namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Maridia environmental palette control program.</summary>
public sealed class MaridiaEnvironmentalPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal MaridiaEnvironmentalPaletteFxColorDefinition()
    {
    }

    /// <summary>The environmental animation represented by this program.</summary>
    public required MaridiaEnvironmentalPaletteOwner Owner { get; init; }

    /// <summary>A one-color cyclic rotation returns after the named group size.</summary>
    public required int FrameCount { get; init; }

    /// <summary>Sand pits rotate two groups; sand falls and waterfalls each rotate one group.</summary>
    public required int ColorsPerFrame { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
