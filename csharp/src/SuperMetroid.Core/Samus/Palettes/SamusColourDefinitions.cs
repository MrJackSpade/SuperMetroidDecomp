using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// The sprites that show Samus's colourings (#1405, #1512). The colourings' sizes, and which inks of her sheets each
/// recolours, are the import's.
/// </summary>
internal static class SamusColourDefinitions
{
    /// <summary>The sprites that show Samus's colours: her body, her equipment's and her weapons' draws.</summary>
    public static ImmutableHashSet<SpriteSourceKind> Sources { get; } =
    [
        SpriteSourceKind.Samus, SpriteSourceKind.SamusArmCannon, SpriteSourceKind.SamusAtmosphere, SpriteSourceKind.SamusBomb,
        SpriteSourceKind.SamusChargeFlare, SpriteSourceKind.SamusDeathExplosion,
        SpriteSourceKind.SamusEcho, SpriteSourceKind.SamusGrappleEndpoint, SpriteSourceKind.SamusGrappleFlare,
        SpriteSourceKind.SamusGrappleRope, SpriteSourceKind.SamusHitBurst, SpriteSourceKind.SamusProjectile,
        SpriteSourceKind.SamusProjectileTrail,
    ];
}
