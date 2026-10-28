namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete persistent Zebes-explosion ambient palette loop.</summary>
public sealed class ZebesExplosionAmbientPaletteFxColorDefinition(
        ZebesExplosionAmbientPaletteFxProgramOwner owner,
        PaletteFxObject paletteFxObject,
        PaletteFxProgram program,
        int colorByteIndex,
        int colorsPerFrame,
        int cycleFrames)
{
    /// <summary>The mutually exclusive ambient owner.</summary>
    public ZebesExplosionAmbientPaletteFxProgramOwner Owner { get; } = owner;

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks><c>$8D:E1D4</c> is afterglow and <c>$8D:E1D8</c> is lava.</remarks>
    public PaletteFxObject Object { get; } = paletteFxObject;

    /// <summary>The first destination byte in CGRAM.</summary>
    public int ColorByteIndex { get; } = colorByteIndex;

    /// <summary>The number of timed records in the loop.</summary>
    public int FrameCount => Rows.FrameCount;

    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public int ColorsPerFrame { get; } = colorsPerFrame;

    /// <summary>The complete loop duration in frames.</summary>
    public int CycleFrames { get; } = cycleFrames;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(program, colorsPerFrame);
}
