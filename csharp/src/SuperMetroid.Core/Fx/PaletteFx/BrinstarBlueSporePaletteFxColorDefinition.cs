namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One room-specific entry into the fourteen-frame blue-spore palette loop.</summary>
public sealed class BrinstarBlueSporePaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal BrinstarBlueSporePaletteFxColorDefinition()
    {
    }

    /// <summary>The room family represented by this program.</summary>
    public required BrinstarBlueSporePaletteOwner Owner { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorRows Rows { get; init; }
}
