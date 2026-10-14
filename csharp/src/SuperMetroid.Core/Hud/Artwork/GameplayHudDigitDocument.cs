namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Serializable numeric glyphs and their gameplay-HUD anchors.</summary>
public sealed record GameplayHudDigitDocument
{
    /// <summary>Gets the ten health-digit cells in numeral order.</summary>
    public required GameplayHudCell[] Health { get; init; }
    /// <summary>Gets the ten ammunition-digit cells in numeral order.</summary>
    public required GameplayHudCell[] Ammo { get; init; }
    /// <summary>Gets the anchor of the health digits.</summary>
    public required Position16<TilemapCells> HealthAnchor { get; init; }
    /// <summary>Gets the anchor of the missile digits.</summary>
    public required Position16<TilemapCells> MissileAnchor { get; init; }
    /// <summary>Gets the anchor of the Super Missile digits.</summary>
    public required Position16<TilemapCells> SuperMissileAnchor { get; init; }
    /// <summary>Gets the anchor of the Power Bomb digits.</summary>
    public required Position16<TilemapCells> PowerBombAnchor { get; init; }
}
