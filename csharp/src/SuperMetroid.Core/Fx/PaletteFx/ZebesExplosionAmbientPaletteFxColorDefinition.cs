namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete persistent Zebes-explosion ambient palette loop.</summary>
public sealed class ZebesExplosionAmbientPaletteFxColorDefinition
{
    /// <summary>Captures one palette loop's owner, object, first record, color width, and cycle length.</summary>
    internal ZebesExplosionAmbientPaletteFxColorDefinition(
        ZebesExplosionAmbientPaletteFxProgramOwner owner,
        PaletteFxObject paletteFxObject,
        int firstRecord,
        ushort colorByteIndex,
        int frameCount,
        int colorsPerFrame,
        int cycleFrames)
    {
        Owner = owner;
        Object = paletteFxObject;
        Rows = new(firstRecord, frameCount, colorsPerFrame);
        ColorByteIndex = colorByteIndex;
        FrameCount = frameCount;
        ColorsPerFrame = colorsPerFrame;
        CycleFrames = cycleFrames;
    }

    /// <summary>The mutually exclusive ambient owner.</summary>
    public ZebesExplosionAmbientPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks><c>$8D:E1D4</c> is afterglow and <c>$8D:E1D8</c> is lava.</remarks>
    public PaletteFxObject Object { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The number of timed records in the loop.</summary>
    public int FrameCount { get; }

    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The complete loop duration in frames.</summary>
    public int CycleFrames { get; }

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => (ColorsPerFrame + 2) * sizeof(ushort);

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
