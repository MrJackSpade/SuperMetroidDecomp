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
    /// <summary>An enemy, or one component of an extended enemy; the instance is its enemy slot.</summary>
    Enemy,
    /// <summary>An enemy projectile; the instance is its enemy-projectile slot.</summary>
    EnemyProjectile,
    /// <summary>A room sprite object (explosion, dust, splash); the instance is its sprite-object slot.</summary>
    RoomSpriteObject,
    /// <summary>A Ceres elevator arrival actor; the instance is 0 for the moving pad and 1 for the platform.</summary>
    CeresElevatorArrival,
    /// <summary>Dead Torizo's post-enemy frame, drawn by its graphics hook.</summary>
    DeadTorizoHook,
    /// <summary>A Ridley tail segment; the instance is the segment, 0 at the base through 6 at the tip.</summary>
    RidleyTail,
    /// <summary>Ridley's wings.</summary>
    RidleyWings,
    /// <summary>The Baby Metroid Ceres Ridley's scene draws.</summary>
    CeresBaby,
    /// <summary>The Ridley-room door overlay Ceres Ridley's scene draws.</summary>
    CeresDoorOverlay,
    /// <summary>Mother Brain's brain.</summary>
    MotherBrainBrain,
    /// <summary>A Mother Brain neck joint; the instance is the joint, 0 nearest the brain through 4 at the body.</summary>
    MotherBrainNeck,
}
