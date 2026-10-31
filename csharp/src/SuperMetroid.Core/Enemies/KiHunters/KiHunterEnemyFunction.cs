namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The routine a Ki-Hunter's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum KiHunterEnemyFunction
{
    /// <summary>Patrols within the spawn-height band, reflects movement on collision, and starts a swoop toward nearby Samus below.</summary>
    FlyingPatrol = 1,
    /// <summary>Follows an accelerating elliptic dive, changing art at the swipe angle and backing off on collision.</summary>
    Swooping,
    /// <summary>Moves upward away from a blocked dive until collision or passage above the spawn Y restores patrol.</summary>
    RecoveringFromSwoop,
    /// <summary>Applies gravity after clipping wings or a ground-bound spawn, then prepares a hop on landing.</summary>
    FallingAfterWingLoss,
    /// <summary>Aims a two-pixel-per-frame hop toward Samus with an RNG-selected initial upward speed of seven or eight pixels per frame.</summary>
    StartGroundJump,
    /// <summary>Leaves movement to the jump, landing, or spit animation until its instruction stream selects the next function.</summary>
    NoOp,
    /// <summary>Advances the collision-bearing ballistic hop and starts the landing animation after downward collision.</summary>
    GroundJump,
    /// <summary>Counts down the post-landing or post-spit wait, then spits within 96 horizontal pixels or prepares another hop.</summary>
    GroundWait,
    /// <summary>Installs the acid-spit list facing Samus and suspends movement while bytecode owns the attack.</summary>
    SelectAcidSpit,
    /// <summary>Copies X and Y from the physically preceding body slot, native enemy minus $40.</summary>
    FollowBody,
    /// <summary>Dispatches the detached wing's secondary arc function stored in its extended variable zero.</summary>
    DetachedWing,
}
