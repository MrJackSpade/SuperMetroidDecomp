namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Samus-loading palette control program.</summary>
public sealed class SamusLoadingSuitPaletteFxColorDefinition(
        SamusLoadingSuitPaletteFxProgramOwner owner,
        PaletteFxObject paletteFxObject,
        PaletteFxProgram program)
{

    /// <summary>The mutually exclusive suit palette.</summary>
    public SamusLoadingSuitPaletteFxProgramOwner Owner { get; } = owner;

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks><c>$8D:E1F4</c>, <c>$8D:E1F8</c>, or <c>$8D:E1FC</c>.</remarks>
    public PaletteFxObject Object { get; } = paletteFxObject;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; } = PaletteFxColorRows.Of(program, SamusLoadingSuitPaletteFxColorDefinitions.ColorsPerFrame);
}
