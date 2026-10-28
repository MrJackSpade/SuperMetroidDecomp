namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Samus-loading palette control program.</summary>
public sealed class SamusLoadingSuitPaletteFxColorDefinition
{
    /// <summary>Builds the definition from its owner's program values.</summary>
    internal SamusLoadingSuitPaletteFxColorDefinition(SamusLoadingSuitPaletteFxProgramOwner owner, PaletteFxObject paletteFxObject, PaletteFxProgram program) =>
        (Owner, Object, Rows) = (owner, paletteFxObject, PaletteFxColorRows.Of(program, SamusLoadingSuitPaletteFxColorDefinitions.ColorsPerFrame));


    /// <summary>The mutually exclusive suit palette.</summary>
    public SamusLoadingSuitPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks><c>$8D:E1F4</c>, <c>$8D:E1F8</c>, or <c>$8D:E1FC</c>.</remarks>
    public PaletteFxObject Object { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows { get; }
}
