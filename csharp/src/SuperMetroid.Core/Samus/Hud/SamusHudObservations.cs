using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>What the HUD reads of Samus's movement and posture.</summary>
internal static class SamusHudObservations
{
    /// <summary>The HUD selection mode of each of the 28 movement types.</summary>
    internal static SamusHudSelectionMode SelectionMode(SamusMovementType movement)
    {
        return movement switch
        {
            SamusMovementType.SpinJumping or SamusMovementType.Knockback or SamusMovementType.CompactAirborne or
            SamusMovementType.WallJumping or SamusMovementType.DamageBoost or SamusMovementType.Special => SamusHudSelectionMode.Jump,
            SamusMovementType.MorphBallGround or SamusMovementType.GlitchBall or SamusMovementType.MorphBallFalling or
            SamusMovementType.GlitchBallKnockback or SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or SamusMovementType.SpringBallFalling => SamusHudSelectionMode.MorphBall,
            SamusMovementType.Grappling => SamusHudSelectionMode.Grapple,
            SamusMovementType.TurningOnGround or SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling => SamusHudSelectionMode.Turning,
            SamusMovementType.PostureTransition => SamusHudSelectionMode.Transition,
            SamusMovementType.DraygonHeld => SamusHudSelectionMode.DraygonHeld,
            SamusMovementType.Standing or SamusMovementType.Running or SamusMovementType.NormalJumping or
                SamusMovementType.Crouching or SamusMovementType.Falling or SamusMovementType.Moonwalking or
                SamusMovementType.RanIntoWall => SamusHudSelectionMode.Standard,
            _ => throw new ArgumentOutOfRangeException(nameof(movement), movement, "Undefined movement type."),
        };
    }

    /// <summary>
    /// Whether the posture-transition admission admits weapons for <paramref name="pose"/>: the transition table admits
    /// crouch and stand transitions and preserves the charge through morph and unmorph art; outside the table the answer
    /// is the one <paramref name="beyond"/> decoded from what the admission reads there.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The admission reads no flag for the pose.</exception>
    internal static bool PostureAdmitsWeapons(SamusPoseId pose, PostureAdmissionBeyondTransitions<SamusPoseId> beyond)
    {
        Ensure.NotNull(beyond);
        SamusPostureTransitionAdmission admission = SamusPoseMetadata.PostureTransitionAdmission(pose);
        return admission switch
        {
            SamusPostureTransitionAdmission.TableAdmits => true,
            SamusPostureTransitionAdmission.TablePreservesCharge => false,
            SamusPostureTransitionAdmission.BeyondTable => beyond.AdmitsWeapons(pose),
            SamusPostureTransitionAdmission.AlwaysAdmits or SamusPostureTransitionAdmission.NeverAdmits =>
                throw new ArgumentOutOfRangeException(nameof(pose), pose, "The posture-transition admission reads no flag for this pose."),
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusPostureTransitionAdmission)} {admission}."),
        };
    }
}
