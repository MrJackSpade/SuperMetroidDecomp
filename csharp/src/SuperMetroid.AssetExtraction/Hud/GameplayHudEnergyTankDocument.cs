using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Serializable energy-tank cell definitions and HUD anchors.</summary>
public sealed record GameplayHudEnergyTankDocument
{
    /// <summary>Gets the ordered energy-tank cell coordinates.</summary>
    public required Position16<TilemapCells>[] Anchors { get; init; }
    /// <summary>Gets the cell drawn for a filled energy tank.</summary>
    public required GameplayHudCell Filled { get; init; }
    /// <summary>Gets the cell drawn for an empty energy tank.</summary>
    public required GameplayHudCell Empty { get; init; }
}
