namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Maridia environmental palette control program.</summary>
public sealed class MaridiaEnvironmentalPaletteFxColorDefinition
{
    /// <summary>Builds the definition from its owner's program values.</summary>
    /// <param name="owner">The environmental animation.</param>
    /// <param name="program">The program whose records the rotation shows.</param>
    /// <param name="frameCount">The records after which the one-color cyclic rotation returns.</param>
    /// <param name="colorsPerFrame">The colours each record writes.</param>
    internal MaridiaEnvironmentalPaletteFxColorDefinition(MaridiaEnvironmentalPaletteOwner owner, PaletteFxProgram program, int frameCount, int colorsPerFrame)
    {
        Owner = owner;
        FrameCount = frameCount;
        ColorsPerFrame = colorsPerFrame;
        Rows = PaletteFxColorRows.Of(program, colorsPerFrame);
    }

    /// <summary>The environmental animation represented by this program.</summary>
    public MaridiaEnvironmentalPaletteOwner Owner { get; }

    /// <summary>A one-color cyclic rotation returns after the named group size.</summary>
    public int FrameCount { get; }

    /// <summary>Sand pits rotate two groups; sand falls and waterfalls each rotate one group.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
