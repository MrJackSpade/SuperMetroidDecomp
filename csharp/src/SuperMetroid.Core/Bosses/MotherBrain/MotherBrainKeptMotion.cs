namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The motion a Mother Brain projectile keeping what it reads moves by from its spawn (#1843, #2554): only the behaviour
/// keeping that state runs it (<see cref="MotherBrainProjectiles.KeepingBehavior{TState}"/>).
/// </summary>
internal enum MotherBrainKeptMotion
{
    /// <summary>Turns a Mother Brain room turret, fires it, or removes it once the room asks.</summary>
    TurretAiming,

    /// <summary>Raises dust once, then drops a ceiling tube, accelerating.</summary>
    TubeFalling,

    /// <summary>Keeps drool hanging from Mother Brain's mouth for its first five poses.</summary>
    DroolHanging,

    /// <summary>Holds an onion ring at the mouth for its delay, then flies it with its own collision and arena cull.</summary>
    OnionRingFlying,

    /// <summary>Drops a Mother Brain bomb under gravity, bouncing in stages and testing Samus's bombs.</summary>
    BombBouncing,

    /// <summary>Contracts the rainbow beam's charge around the moving brain.</summary>
    RainbowBeamCharging,

    /// <summary>Keeps a rainbow-beam impact at its offset from Samus as she moves.</summary>
    RainbowImpactTracking,

    /// <summary>Keeps a death explosion at its offset from Mother Brain's body each pass.</summary>
    DeathExplosionTracking,

    /// <summary>Drags and drops a door fragment under gravity for its fixed lifetime.</summary>
    DoorFragmentFalling,
}
