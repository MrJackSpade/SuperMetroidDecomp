namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The installed charge-flare placement and sprite compositions (#633), and the OBJ page it draws over.</summary>
public readonly record struct ChargeFlareArtwork
{
    /// <summary>Placement.</summary>
    public required SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacementCatalog? Placement { get; init; }

    /// <summary>Compositions.</summary>
    public required SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteCatalog? Compositions { get; init; }

    /// <summary>The projectile OBJ page the flare's characters index.</summary>
    public required ProjectileDrawArtwork Objects { get; init; }
}
