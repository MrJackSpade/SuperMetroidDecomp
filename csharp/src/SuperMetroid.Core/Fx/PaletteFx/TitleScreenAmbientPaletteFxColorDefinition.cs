namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete looping title-screen ambient palette program.</summary>
public sealed class TitleScreenAmbientPaletteFxColorDefinition
{
    /// <summary>Binds one semantic title palette owner.</summary>
    internal TitleScreenAmbientPaletteFxColorDefinition(TitleScreenAmbientPaletteFxProgramOwner owner) => Owner = owner;
    /// <summary>Semantic title-palette owner selecting native definition $8D:E1A0 (tube) or $8D:E1A4 (displays); not a runtime palette-object slot index.</summary>
    public TitleScreenAmbientPaletteFxProgramOwner Owner { get; }
    /// <summary>Distinguishes the eight-shade tube loop from the two-state displays.</summary>
    internal bool IsTubeLight => Owner == TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight;
    /// <summary>Native color destinations: byte $54 (tube) / $5C (displays).</summary>
    public ushort ColorByteIndex => (ushort)(FirstColor * sizeof(ushort));
    /// <summary>First CGRAM color slot controlled by the selected ambient program.</summary>
    private int FirstColor => IsTubeLight ? TitleScreenAmbientPaletteFxColorDefinitions.TubeFirstColor
        : TitleScreenAmbientPaletteFxColorDefinitions.TubeLastColor + 1;
    /// <summary>Timed palette-image records per loop: eight tube shades or two display states, distinct from the number of palette updates spent in the loop.</summary>
    public int FrameCount => IsTubeLight ? 2 * TitleScreenAmbientPaletteFxColorDefinitions.TubeDimmingSteps
        : TitleScreenAmbientPaletteFxColorDefinitions.DisplayStates;
    /// <summary>Editable packed RGB5 words per timed record: four for tube CGRAM slots 42..45 or two for display slots 46..47, excluding duration and wait control words.</summary>
    public int ColorsPerFrame => (IsTubeLight ? TitleScreenAmbientPaletteFxColorDefinitions.TubeLastColor
        : TitleScreenAmbientPaletteFxColorDefinitions.DisplayLastColor) - FirstColor + 1;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows => PaletteFxColorRows.Of(IsTubeLight ? PaletteFxProgram.TitleScreenBabyMetroidTubeLight1 : PaletteFxProgram.TitleScreenFlickeringDisplays1, ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
