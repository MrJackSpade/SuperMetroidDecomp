namespace SuperMetroid.Core.Assets;

/// <summary>The kinds of actor or display element that publish whole-frame sprite draws.</summary>
public enum SpriteSourceKind
{
    /// <summary>The Ceres or Zebes escape countdown.</summary>
    EscapeTimer,
    /// <summary>Samus's whole body.</summary>
    Samus,
    /// <summary>The arm-cannon cover.</summary>
    SamusArmCannon,
    /// <summary>A splash, bubble or dust effect around Samus.</summary>
    SamusAtmosphere,
    /// <summary>The grapple beam's muzzle flare.</summary>
    SamusGrappleFlare,
    /// <summary>A grapple rope segment, drawn from the muzzle toward the anchor.</summary>
    SamusGrappleRope,
    /// <summary>The grapple beam's endpoint.</summary>
    SamusGrappleEndpoint,
    /// <summary>A speed-booster or shinespark-crash echo of Samus.</summary>
    SamusEcho,
    /// <summary>Samus's death explosion.</summary>
    SamusDeathExplosion,
    /// <summary>A Samus projectile or its explosion.</summary>
    SamusProjectile,
    /// <summary>A bomb or power bomb.</summary>
    SamusBomb,
    /// <summary>A part of Samus's charge flare: the flare or one of its two rings of sparks.</summary>
    SamusChargeFlare,
    /// <summary>One side of a projectile trail.</summary>
    SamusProjectileTrail,
    /// <summary>The burst a hit of Samus's weapon shows at an enemy it hurts (#1713).</summary>
    SamusHitBurst,
    /// <summary>An enemy.</summary>
    Enemy,
    /// <summary>An enemy projectile.</summary>
    EnemyProjectile,
    /// <summary>A room sprite object: an explosion, dust or a splash.</summary>
    RoomSpriteObject,
    /// <summary>The Ceres elevator arrival's moving pad or its platform.</summary>
    CeresElevatorArrival,
    /// <summary>Dead Torizo's sand heap, drawn after the enemy queue.</summary>
    DeadTorizoSandHeap,
    /// <summary>A segment of Ridley's tail, drawn from the tip to the base.</summary>
    RidleyTail,
    /// <summary>Ridley's wings.</summary>
    RidleyWings,
    /// <summary>The Baby Metroid Ceres Ridley's scene draws.</summary>
    CeresBaby,
    /// <summary>The Ridley-room door overlay Ceres Ridley's scene draws.</summary>
    CeresDoorOverlay,
    /// <summary>Mother Brain's brain.</summary>
    MotherBrainBrain,
    /// <summary>A joint of Mother Brain's neck, drawn from the brain to the body.</summary>
    MotherBrainNeck,
    /// <summary>A picture a frontend screen places: the title, a menu, a map or the ending.</summary>
    FrontendScreen,
}
