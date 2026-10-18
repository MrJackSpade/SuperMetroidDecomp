namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Maridia environmental palette control program.</summary>
public sealed class MaridiaEnvironmentalPaletteFxColorDefinition
{
    /// <summary>Selects one of the three mutually exclusive Maridia palette programs.</summary>
    internal MaridiaEnvironmentalPaletteFxColorDefinition(MaridiaEnvironmentalPaletteOwner owner) => Owner = owner;

    /// <summary>The environmental animation represented by this program.</summary>
    public MaridiaEnvironmentalPaletteOwner Owner { get; }

    /// <summary>A one-color cyclic rotation returns after the named group size.</summary>
    public int FrameCount => Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
        ? MaridiaEnvironmentalPaletteFxColorDefinitions.WaterfallRotationColors
        : MaridiaEnvironmentalPaletteFxColorDefinitions.SandRotationColors;

    /// <summary>Sand pits rotate two groups; sand falls and waterfalls each rotate one group.</summary>
    public int ColorsPerFrame => FrameCount * (Owner == MaridiaEnvironmentalPaletteOwner.SandPits ? MaridiaEnvironmentalPaletteFxColorDefinitions.SandPitBandCount : 1);

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows => Owner switch
    {
        MaridiaEnvironmentalPaletteOwner.SandPits => new(498, FrameCount, ColorsPerFrame),
        MaridiaEnvironmentalPaletteOwner.SandFalls => new(502, FrameCount, ColorsPerFrame),
        MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls => new(506, FrameCount, ColorsPerFrame),
        _ => throw new InvalidOperationException("Unsupported Maridia palette owner."),
    };

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
