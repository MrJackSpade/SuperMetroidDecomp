using static SuperMetroid.Core.Samus.Hud.SamusHudRomData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>Compiled weapon-admission policy; HUD artwork and labels are separate assets.</summary>
internal static class SamusHudDefinitions
{
    /// <summary>$90:DD05: movement-state HUD admission dispatch, preserving all 28 native handler identities.</summary>
    internal static SamusHudHandler MovementHandler(SamusMovementType movement)
    {
        if ((byte)movement > (byte)SamusMovementType.Special) throw new IndexOutOfRangeException();
        return movement switch
        {
            SamusMovementType.SpinJumping or SamusMovementType.Knockback or SamusMovementType.Unused0D or
            SamusMovementType.WallJumping or SamusMovementType.DamageBoost or SamusMovementType.Special => SamusHudHandler.Jump,
            SamusMovementType.MorphBallGround or SamusMovementType.UnusedGlitchBall or SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or SamusMovementType.SpringBallFalling => SamusHudHandler.MorphBall,
            SamusMovementType.Unused0B or SamusMovementType.Unused0C or SamusMovementType.Grappling => SamusHudHandler.Grapple,
            SamusMovementType.TurningOnGround or SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling => SamusHudHandler.Turning,
            SamusMovementType.PostureTransition => SamusHudHandler.Transition,
            SamusMovementType.DraygonHeld => SamusHudHandler.DraygonHeld,
            SamusMovementType.Standing or SamusMovementType.Running or SamusMovementType.NormalJumping or
                SamusMovementType.Crouching or SamusMovementType.Falling or SamusMovementType.Moonwalking or
                SamusMovementType.RanIntoWall => SamusHudHandler.Standard,
            _ => throw new InvalidOperationException($"Undefined SamusMovementType {movement}."),
        };
    }

    /// <summary>$90:DDAA-$DDB5 actual transition flags: crouch/stand admit weapons; morph/unmorph preserve charge unless cancelling Grapple. Other admitted indices preserve only their bounded native instruction observations.</summary>
    /// <remarks>
    /// The admission reads a byte per pose. Outside the transition block that byte is the turning, posture, spin, held
    /// and X-ray handlers' code around the real flags, which <paramref name="overreads"/> holds.
    /// </remarks>
    internal static byte PostureObservation(SamusPoseId pose, SamusOverreadCatalog? overreads)
    {
        if (pose >= NonFiringTransitionStart)
            throw new ArgumentOutOfRangeException(nameof(pose));
        if (pose < FirstTransitionPose)
            return RequireOverreads(overreads).HudPrecedingPostureByte((int)pose);
        if (pose > SamusPoseId.UnusedPose40)
            return RequireOverreads(overreads).HudFollowingPostureByte((int)pose - ((int)SamusPoseId.UnusedPose40 + 1));
        return (SamusPoseId)pose switch
        {
            SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or
            SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose => 0,
            SamusPoseId.MorphingTransitionRightPose or SamusPoseId.MorphingTransitionLeftPose or
            SamusPoseId.UnmorphingTransitionRightPose or SamusPoseId.UnmorphingTransitionLeftPose or
            SamusPoseId.UnusedPose39 or SamusPoseId.UnusedPose3A or
            SamusPoseId.UnusedPose3F or SamusPoseId.UnusedPose40 => 1,
            _ => throw new InvalidOperationException($"Pose ${(int)pose:X2} is outside the $35-$40 transition block."),
        };
    }

    /// <summary>Requires the installed overread catalog for a pose outside the transition block.</summary>
    private static SamusOverreadCatalog RequireOverreads(SamusOverreadCatalog? overreads) =>
        overreads ?? throw new InvalidOperationException(
            "A posture-transition admission outside the transition block needs the installed Samus overread catalog.");
}
