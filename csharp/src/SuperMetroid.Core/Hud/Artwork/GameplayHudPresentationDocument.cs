namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Serializable versioned document describing the editable gameplay HUD.</summary>
public sealed record GameplayHudPresentationDocument
{
    /// <summary>Gets the gameplay-HUD document schema version.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the immutable 32-cell top row transferred at the HUD DMA boundary.</summary>
    public required GameplayHudCell[] TopRow { get; init; }
    /// <summary>Gets the complete 32-by-3 mutable HUD tilemap template.</summary>
    public required GameplayHudCell[] Template { get; init; }
    /// <summary>Gets the cell used to clear dynamic HUD regions.</summary>
    public required GameplayHudCell Blank { get; init; }
    /// <summary>Gets the palette index used for a selected item.</summary>
    public required int SelectedPalette { get; init; }
    /// <summary>Gets the palette index used for a deselected item.</summary>
    public required int DeselectedPalette { get; init; }
    /// <summary>Gets the upper-left coordinate of the minimap output region.</summary>
    public required Position16<TilemapCells> MinimapAnchor { get; init; }
    /// <summary>Gets the energy-tank visuals and anchors.</summary>
    public required GameplayHudEnergyTankDocument EnergyTanks { get; init; }
    /// <summary>Gets the health and ammunition digit visuals and anchors.</summary>
    public required GameplayHudDigitDocument Digits { get; init; }
    /// <summary>Gets the AUTO reserve-indicator visuals and anchors.</summary>
    public required GameplayHudAutoReserveDocument AutoReserve { get; init; }
    /// <summary>Gets item-icon documents keyed by their canonical names.</summary>
    public required Dictionary<string, GameplayHudIconDocument> Icons { get; init; }
}
