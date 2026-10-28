namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The installed charge-flare placement (#633) and the projectile sheet the flare draws from.</summary>
public readonly record struct ChargeFlareArtwork
{
    /// <summary>Placement.</summary>
    public required SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacements? Placement { get; init; }


    /// <summary>The projectile sheet the flare's frames are drawn from.</summary>
    public required ProjectileDrawArtwork Objects { get; init; }
}
