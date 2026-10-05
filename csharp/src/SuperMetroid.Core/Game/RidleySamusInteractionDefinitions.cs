namespace SuperMetroid.Core.Game;

/// <summary>Movement policies selected by CheckIfSamusMorphedSpinJumpingDamageBoosting at $A6:BCF1.</summary>
/// <remarks>
/// The BIT operand uses the high byte of $A6:BD04..BD1F: bit7 controls grab eligibility,
/// bit6 controls morphed release timing. Those are the only consumed fields in the two
/// managed callers. No lower flag bits or adjacent-table reads are exposed by their contract.
/// </remarks>
internal static class RidleySamusInteractionDefinitions
{
    private enum GrabPolicy { Immune, Ordinary, Morphed }

    /// <summary>$A6:BD04..BD1F dispatches by the named Samus movement handler; unknown byte values retain the managed no-grab/default-release boundary.</summary>
    private static GrabPolicy Policy(SamusMovementType movement) => movement switch
    {
        SamusMovementType.MorphBallGround or SamusMovementType.UnusedGlitchBall or
        SamusMovementType.MorphBallFalling or SamusMovementType.UnusedGlitchBallAlternate or
        SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
        SamusMovementType.SpringBallFalling => GrabPolicy.Morphed,
        SamusMovementType.Standing or SamusMovementType.Running or SamusMovementType.NormalJumping or
        SamusMovementType.Crouching or SamusMovementType.Falling or SamusMovementType.Knockback or
        SamusMovementType.Unused0D or SamusMovementType.TurningOnGround or SamusMovementType.PostureTransition or
        SamusMovementType.Moonwalking or SamusMovementType.WallJumping or SamusMovementType.RanIntoWall or
        SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling or
        SamusMovementType.Special => GrabPolicy.Ordinary,
        _ => GrabPolicy.Immune,
    };

    /// <summary>$A6:BCFC transfers movement bit7 to carry, permitting Ridley's grab when set.</summary>
    internal static bool CanGrab(SamusMovementType movement) => Policy(movement) != GrabPolicy.Immune;

    /// <summary>$A6:BC98 loads6 frames for morphed overflow; $A6:BC9D otherwise loads10.</summary>
    internal static ushort ReleaseIntangibilityFrames(SamusMovementType movement) =>
        Policy(movement) == GrabPolicy.Morphed ? (ushort)6 : (ushort)10;
}