namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete persistent Zebes-explosion ambient palette loop.</summary>
public sealed class ZebesExplosionAmbientPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal ZebesExplosionAmbientPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive ambient owner.</summary>
    public required ZebesExplosionAmbientPaletteFxProgramOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks><c>$8D:E1D4</c> is afterglow and <c>$8D:E1D8</c> is lava.</remarks>
    public required PaletteFxObject Object { get; init; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public required int ColorByteIndex { get; init; }

    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public required int ColorsPerFrame { get; init; }

    /// <summary>The complete loop duration in frames.</summary>
    public required int CycleFrames { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }
}
