namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Calculated setup, two repeated flash groups and neutral intervals of the Landing Site's lightning.</summary>
public sealed class CrateriaLightningPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal CrateriaLightningPaletteFxColorDefinition()
    {
    }

    /// <summary>Live colours per timed record: the eight the lightning flashes.</summary>
    public required int ColorsPerFrame { get; init; }

    /// <summary>The 240-update neutral records the program shows before its last flash group.</summary>
    internal int NeutralCount { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
