namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Samus-loading palette control program.</summary>
public sealed class SamusLoadingSuitPaletteFxColorDefinition
{

    /// <summary>Pairs one suit's palette-FX object with its first record.</summary>
    internal SamusLoadingSuitPaletteFxColorDefinition(
        SamusLoadingSuitPaletteFxProgramOwner owner,
        PaletteFxObject paletteFxObject,
        int firstRecord)
    {
        Owner = owner;
        Object = paletteFxObject;
        Rows = new(firstRecord, SamusLoadingSuitPaletteFxColorDefinitions.FrameCount, SamusLoadingSuitPaletteFxColorDefinitions.ColorsPerFrame);
    }

    /// <summary>The mutually exclusive suit palette.</summary>
    public SamusLoadingSuitPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks><c>$8D:E1F4</c>, <c>$8D:E1F8</c>, or <c>$8D:E1FC</c>.</remarks>
    public PaletteFxObject Object { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
