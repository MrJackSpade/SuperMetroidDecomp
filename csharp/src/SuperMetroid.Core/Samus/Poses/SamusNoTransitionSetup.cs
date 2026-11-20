namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The setup the pose update runs when Samus's input chooses no new pose (#2506), from her fallback pose: a pose names its
/// own, and the ground turn and the normal jump name theirs on their movement (<see cref="SamusNoTransitionSetups"/>).
/// </summary>
internal enum SamusNoTransitionSetup
{
    /// <summary>Nothing more is set up: her pose and momentum stay as her movement left them.</summary>
    KeepsMomentum,

    /// <summary>A knockback kept as its fallback stops her momentum.</summary>
    StopKnockback,

    /// <summary>Releasing every damage-boost chord leaves the boost for the neutral jump.</summary>
    ReleaseDamageBoost,

    /// <summary>Draygon's hold returns her aimed, firing or struggling pose to the hold's neutral one.</summary>
    ReturnToNeutralDraygonHold,

    /// <summary>A wall jump gives way to the ordinary spin, losing its speed unless the spin's setup promotes it.</summary>
    SpinFromWallJump,

    /// <summary>A falling Morph Ball kept as its fallback slows down.</summary>
    DecelerateFallingBall,

    /// <summary>A still or airborne ball kept as its fallback loses her horizontal speed.</summary>
    StopBall,

    /// <summary>A rolling ball stops, losing her horizontal speed, and takes its still pose.</summary>
    StopRollingBall,

    /// <summary>Plain running slows down while she keeps speed, and otherwise stops in the standing pose.</summary>
    StopRunning,

    /// <summary>Aimed or firing running slows down while she keeps speed, and otherwise lowers her aim to standing.</summary>
    StopAimedRunning,

    /// <summary>A moonwalk ends in its standing or aimed fallback.</summary>
    StopMoonwalk,

    /// <summary>An aimed standing, crouching or wall-stop pose lowers her aim to its plain pose.</summary>
    LowerGroundedAim,

    /// <summary>An aimed crouching turn still in its own pose is abandoned for its crouch.</summary>
    AbandonAimedCrouchingTurn,

    /// <summary>A turn still in its own pose stops her momentum without restarting the turn.</summary>
    StopTurn,

    /// <summary>A normal jump takes its fallback's aim and slows down.</summary>
    ContinueNormalJump,

    /// <summary>An aimed fall takes its fallback's aim.</summary>
    LowerAerialAim,
}
