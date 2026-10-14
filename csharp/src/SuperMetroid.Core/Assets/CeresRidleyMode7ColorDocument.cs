namespace SuperMetroid.Core.Assets;

/// <summary>Editable Ceres Ridley getaway zoom shades corresponding to $A6:B107 onward, excluding each native row's unused sixteenth word.</summary>
public sealed record CeresRidleyMode7ColorDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresRidleyMode7ColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Nine zoom-high-byte rows, zero through eight, each with fifteen RGB5 colors.</summary>
    public required PaletteRgb5[][] ZoomRows { get; init; }
}
