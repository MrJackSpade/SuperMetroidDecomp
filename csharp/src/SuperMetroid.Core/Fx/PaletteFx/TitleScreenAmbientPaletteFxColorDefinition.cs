namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete looping title-screen ambient palette program.</summary>
public sealed class TitleScreenAmbientPaletteFxColorDefinition
{
    /// <summary>Builds the definition from its owner's program values.</summary>
    /// <param name="owner">The semantic title palette owner.</param>
    /// <param name="program">The program whose records the loop shows.</param>
    /// <param name="firstColor">The first CGRAM color slot the program controls.</param>
    /// <param name="lastColor">The last CGRAM color slot the program controls.</param>
    /// <param name="frameCount">The timed palette-image records per loop.</param>
    internal TitleScreenAmbientPaletteFxColorDefinition(TitleScreenAmbientPaletteFxProgramOwner owner, PaletteFxProgram program, int firstColor, int lastColor, int frameCount) =>
        (Owner, ColorByteIndex, FrameCount, ColorsPerFrame, Rows) = (owner, firstColor * sizeof(ushort), frameCount, lastColor - firstColor + 1, PaletteFxColorRows.Of(program, lastColor - firstColor + 1));

    /// <summary>Semantic title-palette owner selecting native definition $8D:E1A0 (tube) or $8D:E1A4 (displays); not a runtime palette-object slot index.</summary>
    public TitleScreenAmbientPaletteFxProgramOwner Owner { get; }
    /// <summary>Native color destinations: byte $54 (tube) / $5C (displays).</summary>
    public int ColorByteIndex { get; }
    /// <summary>Timed palette-image records per loop: eight tube shades or two display states, distinct from the number of palette updates spent in the loop.</summary>
    public int FrameCount { get; }
    /// <summary>Editable packed RGB5 words per timed record: four for tube CGRAM slots 42..45 or two for display slots 46..47, excluding duration and wait control words.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
