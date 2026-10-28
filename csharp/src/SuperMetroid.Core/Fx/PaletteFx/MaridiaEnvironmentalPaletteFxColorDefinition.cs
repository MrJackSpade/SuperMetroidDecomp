namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Maridia environmental palette control program.</summary>
/// <param name="owner">The environmental animation.</param>
/// <param name="program">The program whose records the rotation shows.</param>
/// <param name="frameCount">The records after which the one-color cyclic rotation returns.</param>
/// <param name="colorsPerFrame">The colours each record writes.</param>
public sealed class MaridiaEnvironmentalPaletteFxColorDefinition(
    MaridiaEnvironmentalPaletteOwner owner, PaletteFxProgram program, int frameCount, int colorsPerFrame)
{
    /// <summary>The environmental animation represented by this program.</summary>
    public MaridiaEnvironmentalPaletteOwner Owner { get; } = owner;

    /// <summary>A one-color cyclic rotation returns after the named group size.</summary>
    public int FrameCount { get; } = frameCount;

    /// <summary>Sand pits rotate two groups; sand falls and waterfalls each rotate one group.</summary>
    public int ColorsPerFrame { get; } = colorsPerFrame;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(program, colorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
