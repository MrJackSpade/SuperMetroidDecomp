namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The installed charge-flare placement (#633) and the projectile sheet the flare draws from.</summary>
public readonly record struct ChargeFlareArtwork
{
    /// <summary>The installed offsets of the flare from Samus, by her movement and aim.</summary>
    public required SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacements Placement { get; init; }

    /// <summary>The projectile sheets the flare's frames are drawn from, one for each beam combination.</summary>
    public required ProjectileDrawArtwork Sheets { get; init; }
}
