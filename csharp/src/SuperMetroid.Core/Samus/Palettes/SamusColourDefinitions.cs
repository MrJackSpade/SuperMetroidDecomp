using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// The sizes of Samus's colourings and the sprites that show them (#1405, #1512). Which inks of her sheets each colouring
/// recolours is the import's.
/// </summary>
internal static class SamusColourDefinitions
{
    /// <summary>The colours one colouring of her suit or her beams holds.</summary>
    public const int SuitColours = 16;

    /// <summary>The beam colours the Hyper Beam's own cycle shows.</summary>
    public const int HyperBeamColours = 8;

    /// <summary>The sprites that show Samus's colours: her body, her equipment's and her weapons' draws.</summary>
    public static ImmutableHashSet<SpriteSourceKind> Sources { get; } =
    [
        SpriteSourceKind.Samus, SpriteSourceKind.SamusArmCannon, SpriteSourceKind.SamusAtmosphere, SpriteSourceKind.SamusBomb,
        SpriteSourceKind.SamusChargeFlare, SpriteSourceKind.SamusChest, SpriteSourceKind.SamusDeathExplosion,
        SpriteSourceKind.SamusEcho, SpriteSourceKind.SamusGrappleEndpoint, SpriteSourceKind.SamusGrappleFlare,
        SpriteSourceKind.SamusGrappleRope, SpriteSourceKind.SamusProjectile, SpriteSourceKind.SamusProjectileTrail,
    ];
}
