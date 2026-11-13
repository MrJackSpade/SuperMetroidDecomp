namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The projectile component's compositions (#1638), import data whose parts are the original's packed sprite placements:
/// only the import composes the projectile and grapple sheets' pictures from them, and the installation holds the pictures.
/// </summary>
/// <param name="Sprites">Projectile and bomb compositions.</param>
/// <param name="Flares">Charge-flare compositions.</param>
/// <param name="Trails">Trail frame appearances.</param>
/// <param name="Grapple">The grapple endpoint and beam sprite compositions.</param>
/// <param name="HitBursts">The compositions of the burst her hit shows at an enemy it hurts (#1713).</param>
public sealed record ProjectileCompositions(InstalledProjectileSprites Sprites, ChargeFlareSprites Flares, InstalledProjectileTrails Trails,
    InstalledGrappleSprites Grapple, InstalledProjectileSprites HitBursts);
