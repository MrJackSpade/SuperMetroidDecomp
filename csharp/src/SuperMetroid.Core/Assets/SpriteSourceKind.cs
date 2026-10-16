namespace SuperMetroid.Core.Assets;

/// <summary>The kinds of actor or display element that publish whole-frame sprite draws.</summary>
/// <remarks>Append-only: numeric values appear in serialized display packets and debugger state.</remarks>
public enum SpriteSourceKind
{
    /// <summary>The Ceres or Zebes escape countdown.</summary>
    EscapeTimer,
    /// <summary>Samus's body: instance 0 is the top spritemap, 1 the bottom one.</summary>
    Samus,
    /// <summary>The power-suit chest cover drawn over forward-facing Samus.</summary>
    SamusChest,
    /// <summary>The arm-cannon cover.</summary>
    SamusArmCannon,
    /// <summary>A splash, bubble or dust effect; the instance is its atmospheric slot.</summary>
    SamusAtmosphere,
    /// <summary>The grapple beam's muzzle flare.</summary>
    SamusGrappleFlare,
    /// <summary>A grapple rope segment; the instance is its position along the rope.</summary>
    SamusGrappleRope,
    /// <summary>The grapple beam's endpoint.</summary>
    SamusGrappleEndpoint,
    /// <summary>A speed-booster or shinespark-crash echo of Samus; the instance is its echo slot times two plus top (0) or bottom (1).</summary>
    SamusEcho,
    /// <summary>Samus's death explosion.</summary>
    SamusDeathExplosion,
    /// <summary>A Samus projectile or its explosion; the instance is its projectile slot.</summary>
    SamusProjectile,
    /// <summary>A bomb or power bomb; the instance is its bomb slot.</summary>
    SamusBomb,
    /// <summary>A charge-flare component; the instance is the component (0 flare, 1 and 2 sparks).</summary>
    SamusChargeFlare,
    /// <summary>One side of a projectile trail; the instance is its trail slot times two plus left (0) or right (1).</summary>
    SamusProjectileTrail,
}
