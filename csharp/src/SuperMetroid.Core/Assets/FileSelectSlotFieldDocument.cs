namespace SuperMetroid.Core.Assets;

/// <summary>Dynamic display-field anchors for one save slot; all coordinates are tiles inside a 32-by-32 page.</summary>
public sealed record FileSelectSlotFieldDocument
{
    /// <summary>Origin of the ENERGY label patch; the engine also derives energy-tank placement from this anchor.</summary>
    public required Position16<TilemapCells> EnergyAnchor { get; init; }
    /// <summary>Origin of the two decimal health digits, written at horizontal offsets zero and one.</summary>
    public required Position16<TilemapCells> HealthAnchor { get; init; }
    /// <summary>Origin of the empty-slot NO DATA patch, used when the slot contains no valid save.</summary>
    public required Position16<TilemapCells> NoDataAnchor { get; init; }
    /// <summary>Origin of the five-tile HH:MM field: hour digits at offsets 0-1, colon at 2, and minute digits at 3-4.</summary>
    public required Position16<TilemapCells> TimeValueAnchor { get; init; }
}
