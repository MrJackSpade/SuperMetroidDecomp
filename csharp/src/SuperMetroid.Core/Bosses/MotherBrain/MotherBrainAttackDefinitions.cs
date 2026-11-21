using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What Mother Brain's attack chooser reads of Samus (<see cref="MotherBrainAttacks"/>).</summary>
internal static partial class MotherBrainAttackDefinitions
{
    /// <summary>
    /// The movement types the attack chooser counts as airborne: jumping, spinning, falling, the airborne balls, the glitch
    /// ball's knockback, the compact airborne body and the wall jump.
    /// </summary>
    internal static readonly SamusMovementType[] AirborneMovements =
    [
        SamusMovementType.NormalJumping,
        SamusMovementType.SpinJumping,
        SamusMovementType.Falling,
        SamusMovementType.MorphBallFalling,
        SamusMovementType.GlitchBallKnockback,
        SamusMovementType.CompactAirborne,
        SamusMovementType.SpringBallInAir,
        SamusMovementType.SpringBallFalling,
        SamusMovementType.WallJumping,
    ];
}
