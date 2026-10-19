namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete cinematic glow control program.</summary>
public sealed class CinematicGlowPaletteFxColorDefinition
{
    /// <summary>Binds a glow owner to its object, first record, CGRAM destination, record width, and cadence.</summary>
    internal CinematicGlowPaletteFxColorDefinition(
        CinematicGlowPaletteFxProgramOwner owner,
        PaletteFxObject paletteFxObject,
        int firstRecord,
        ushort colorByteIndex,
        int colorsPerFrame,
        ushort frameDuration)
    {
        Owner = owner;
        Object = paletteFxObject;
        Rows = new(firstRecord, CinematicGlowPaletteFxColorDefinitions.FrameCount, colorsPerFrame);
        ColorByteIndex = colorByteIndex;
        ColorsPerFrame = colorsPerFrame;
        FrameDuration = frameDuration;
    }

    /// <summary>The mutually exclusive glow owner.</summary>
    public CinematicGlowPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks>
    /// <c>$8D:E1BC</c> owns the old Mother Brain lights and <c>$8D:E1C0</c> the gunship.
    /// </remarks>
    public PaletteFxObject Object { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public ushort FrameDuration { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
