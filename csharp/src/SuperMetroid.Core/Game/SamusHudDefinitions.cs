using static SuperMetroid.Core.Game.SamusHudRomData;

namespace SuperMetroid.Core.Game;

/// <summary>Compiled weapon-admission policy; HUD artwork and labels are separate assets.</summary>
internal static class SamusHudDefinitions
{
    /// <summary>$90:DD75-$DDA9: bounded bytes of turning and posture-transition handler instructions, preceding the real flags.</summary>
    /// <remarks>These 53 accidental observations encode unrelated native instructions. Generating them would require reconstructing 65816 encoding, not managed posture policy; retained only for the existing bounded API.</remarks>
    private static ReadOnlySpan<byte> PrecedingInstructionObservations =>
    [
        0x5e, 0x0b, 0xf0, 0x04, 0x20, 0x3d, 0xdd, 0x60,
        0xad, 0x32, 0x0d, 0xc9, 0xf0, 0xc4, 0xf0, 0x06,
        0xa9, 0x56, 0xc8, 0x8d, 0x32, 0x0d, 0x60, 0xad,
        0x1c, 0x0a, 0xc9, 0xf1, 0x00, 0x10, 0x12, 0xc9,
        0xdb, 0x00, 0x10, 0x10, 0x38, 0xe9, 0x35, 0x00,
        0xaa, 0xbd, 0xaa, 0xdd, 0x29, 0xff, 0x00, 0xd0,
        0x10, 0x20, 0x3d, 0xdd, 0x60,
    ];

    /// <summary>$90:DDB6-$DE4F: bounded bytes of spin/held/X-ray HUD handlers and SamusIsHit_Interruption, following the real flags.</summary>
    /// <remarks>These 154 accidental observations encode unrelated native instructions. The approved nonsense exception ends at DE4F and excludes all twelve actual transition flags.</remarks>
    private static ReadOnlySpan<byte> FollowingInstructionObservations =>
    [
        0xad, 0x32, 0x0d, 0xc9, 0xf0, 0xc4, 0xf0, 0x09,
        0xa9, 0x56, 0xc8, 0x8d, 0x32, 0x0d, 0x20, 0x3d,
        0xdd, 0x60, 0xa5, 0x8b, 0x2c, 0xb6, 0x09, 0xd0,
        0x04, 0x20, 0x0d, 0xb8, 0x60, 0x22, 0xd6, 0xca,
        0x91, 0x60, 0xad, 0x1c, 0x0a, 0xc9, 0xdf, 0x00,
        0xf0, 0x05, 0x20, 0x3d, 0xdd, 0x80, 0x03, 0x20,
        0x9d, 0xbf, 0x60, 0x08, 0xc2, 0x30, 0xad, 0xaa,
        0x18, 0xf0, 0x2f, 0xad, 0xe0, 0x0d, 0xc9, 0x07,
        0x00, 0x30, 0x08, 0x9c, 0xa8, 0x18, 0x9c, 0xaa,
        0x18, 0x80, 0x1d, 0xad, 0x78, 0x0a, 0xd0, 0x18,
        0xad, 0x52, 0x0a, 0xd0, 0x13, 0xad, 0x1f, 0x0a,
        0x29, 0xff, 0x00, 0x0a, 0xaa, 0xfc, 0x82, 0xde,
        0x90, 0x06, 0xa9, 0x01, 0x00, 0x8d, 0x30, 0x0a,
        0x28, 0x60, 0xad, 0x52, 0x0a, 0xf0, 0x53, 0xad,
        0x1f, 0x0a, 0x29, 0xff, 0x00, 0xc9, 0x0a, 0x00,
        0xf0, 0x18, 0xad, 0x32, 0x0a, 0xc9, 0x03, 0x00,
        0xd0, 0x08, 0xa9, 0x08, 0x00, 0x8d, 0x32, 0x0a,
        0x28, 0x60, 0xad, 0x1c, 0x0a, 0x8d, 0x2c, 0x0a,
        0x80, 0x28, 0xad, 0xd0, 0x0c, 0xc9, 0x10, 0x00,
        0x30, 0x07,
    ];

    /// <summary>$90:DD05: movement-state HUD admission dispatch, preserving all 28 native handler identities.</summary>
    internal static ushort MovementHandler(SamusMovementType movement)
    {
        if ((byte)movement > (byte)SamusMovementType.Special) throw new IndexOutOfRangeException();
        return movement switch
        {
            SamusMovementType.SpinJumping or SamusMovementType.Knockback or SamusMovementType.Unused0D or
            SamusMovementType.WallJumping or SamusMovementType.DamageBoost or SamusMovementType.Special => JumpHandler,
            SamusMovementType.MorphBallGround or SamusMovementType.UnusedGlitchBall or SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or SamusMovementType.SpringBallFalling => MorphBallHandler,
            SamusMovementType.Unused0B or SamusMovementType.Unused0C or SamusMovementType.Grappling => GrappleHandler,
            SamusMovementType.TurningOnGround or SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling => TurningHandler,
            SamusMovementType.PostureTransition => TransitionHandler,
            SamusMovementType.DraygonHeld => DraygonHeldHandler,
            SamusMovementType.Standing or SamusMovementType.Running or SamusMovementType.NormalJumping or
                SamusMovementType.Crouching or SamusMovementType.Falling or SamusMovementType.Moonwalking or
                SamusMovementType.RanIntoWall => StandardHandler,
            _ => throw new InvalidOperationException($"Undefined SamusMovementType {movement}."),
        };
    }

    /// <summary>$90:DDAA-$DDB5 actual transition flags: crouch/stand admit weapons; morph/unmorph preserve charge unless cancelling Grapple. Other admitted indices preserve only their bounded native instruction observations.</summary>
    internal static byte PostureObservation(byte pose)
    {
        if (pose >= NonFiringTransitionStart)
            throw new ArgumentOutOfRangeException(nameof(pose));
        if (pose < FirstTransitionPose)
            return PrecedingInstructionObservations[pose];
        if (pose > (byte)SamusPoseId.UnusedPose40)
            return FollowingInstructionObservations[pose - ((byte)SamusPoseId.UnusedPose40 + 1)];
        return (SamusPoseId)pose switch
        {
            SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or
            SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose => 0,
            SamusPoseId.MorphingTransitionRightPose or SamusPoseId.MorphingTransitionLeftPose or
            SamusPoseId.UnmorphingTransitionRightPose or SamusPoseId.UnmorphingTransitionLeftPose or
            SamusPoseId.UnusedPose39 or SamusPoseId.UnusedPose3A or
            SamusPoseId.UnusedPose3F or SamusPoseId.UnusedPose40 => 1,
            _ => throw new InvalidOperationException($"Pose ${pose:X2} is outside the $35-$40 transition block."),
        };
    }
}
