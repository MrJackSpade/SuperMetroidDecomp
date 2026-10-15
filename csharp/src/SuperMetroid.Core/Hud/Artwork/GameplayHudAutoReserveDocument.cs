namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Serializable AUTO reserve-indicator cells and their HUD anchors.</summary>
public sealed record GameplayHudAutoReserveDocument
{
    /// <summary>Gets the ordered AUTO indicator cell coordinates.</summary>
    public required Position16<TilemapCells>[] Anchors { get; init; }
    /// <summary>Gets the six cells drawn when reserve energy is available.</summary>
    public required GameplayHudCell[] ContainsEnergy { get; init; }
    /// <summary>Gets the six cells drawn when the reserve is empty.</summary>
    public required GameplayHudCell[] Empty { get; init; }
}
