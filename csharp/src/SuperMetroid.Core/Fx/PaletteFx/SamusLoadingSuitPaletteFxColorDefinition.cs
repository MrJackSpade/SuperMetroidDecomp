namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Samus-loading palette control program.</summary>
public sealed class SamusLoadingSuitPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal SamusLoadingSuitPaletteFxColorDefinition()
    {
    }


    /// <summary>The mutually exclusive suit palette.</summary>
    public required SamusLoadingSuitPaletteFxProgramOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks><c>$8D:E1F4</c>, <c>$8D:E1F8</c>, or <c>$8D:E1FC</c>.</remarks>
    public required PaletteFxObject Object { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }
}
