namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete exploding-Zebes layer-fade control program.</summary>
public sealed class ZebesExplosionLayerFadePaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal ZebesExplosionLayerFadePaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive faded layer.</summary>
    public required ZebesExplosionLayerFadePaletteFxProgramOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks><c>$8D:E1DC</c> is crust and <c>$8D:E1E0</c> is grey clouds.</remarks>
    public required PaletteFxObject Object { get; init; }


    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public required int FrameDuration { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }
}
