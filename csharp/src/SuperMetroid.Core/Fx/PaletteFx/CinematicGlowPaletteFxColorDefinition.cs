namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete cinematic glow control program.</summary>
public sealed class CinematicGlowPaletteFxColorDefinition(
        CinematicGlowPaletteFxProgramOwner owner,
        PaletteFxObject paletteFxObject,
        PaletteFxProgram program,
        int colorByteIndex,
        int colorsPerFrame,
        int frameDuration)
{
    /// <summary>The mutually exclusive glow owner.</summary>
    public CinematicGlowPaletteFxProgramOwner Owner { get; } = owner;

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks>
    /// <c>$8D:E1BC</c> owns the old Mother Brain lights and <c>$8D:E1C0</c> the gunship.
    /// </remarks>
    public PaletteFxObject Object { get; } = paletteFxObject;

    /// <summary>The first destination byte in CGRAM.</summary>
    public int ColorByteIndex { get; } = colorByteIndex;

    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public int ColorsPerFrame { get; } = colorsPerFrame;

    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public int FrameDuration { get; } = frameDuration;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(program, colorsPerFrame);
}
