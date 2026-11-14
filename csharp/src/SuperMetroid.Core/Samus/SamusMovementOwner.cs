namespace SuperMetroid.Core.Samus;

/// <summary>
/// Who moves Samus in her movement phase: her pose's own movement, or the ability, hold or ride that has taken her
/// movement from it. It sits beside the <see cref="SamusControlMode"/> her frame runs under: the movement phase resolves
/// it in priority order (<see cref="SamusHandlerFrame.MovementOwner"/>), and an owner takes her movement by naming itself
/// (<see cref="SamusMovementHandOver"/>). Its values are arbitrary and nothing reads them.
/// </summary>
internal enum SamusMovementOwner
{
    /// <summary>
    /// Nothing moves her: the death and suit-pickup movement modes, a script's stationary lock, or her locked control
    /// outside an elevator ride.
    /// </summary>
    None,

    /// <summary>Her pose's own movement: standing, running, jumping, falling, rolling and the rest of her ordinary moves.</summary>
    Pose,

    /// <summary>The grapple beam: a swing, or her flight after letting go of one.</summary>
    Grapple,

    /// <summary>Draygon's hold, which leaves her wherever Draygon carries her.</summary>
    DraygonHold,

    /// <summary>The X-ray scope, which turns her and aims its beam.</summary>
    Xray,

    /// <summary>Ceres Ridley's shove, which throws her toward a wall as he flies off.</summary>
    CeresRidleyEjection,

    /// <summary>The knockback from a hit.</summary>
    Knockback,

    /// <summary>A bomb jump's rise and fall.</summary>
    BombJump,

    /// <summary>Crystal Flash: its rise, its bubble and her lowering at its end.</summary>
    CrystalFlash,

    /// <summary>Mother Brain's drain: the locked knockback, the drained crouch and stand, and the drained fall.</summary>
    Drained,

    /// <summary>The shinespark: its windup, its flight and its crash.</summary>
    Shinespark,

    /// <summary>An elevator ride, which moves her by her forward-facing pose's movement while her control is locked.</summary>
    Elevator,
}
