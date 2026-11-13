using static SuperMetroid.Core.Samus.Poses.SamusAnimationDelayDefinitions;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Which animation program each Samus pose animates by (#1473).</summary>
internal static class SamusAnimationDelays
{
    /// <summary>The animation program <paramref name="pose"/> animates by.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The pose is one of the three numbers past the pose records, which no gameplay path gives Samus.
    /// </exception>
    internal static SamusPoseAnimation AnimationForPose(SamusPoseId pose) => AnimationOf(pose);

    /// <summary>The step of <paramref name="pose"/>'s program at frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException">No step of the pose's program is numbered <paramref name="frame"/>.</exception>
    /// <remarks>
    /// A frame on a cue's operand reads the step the operand selects, and a frame past the program's end reads the
    /// program laid out after it, both through <paramref name="continuation"/>, which names each program by a pose that
    /// animates by it; frame targets that program names are shifted back to this pose's frames (#1535).
    /// </remarks>
    /// <param name="pose">The pose whose program is read.</param>
    /// <param name="frame">Samus's animation frame.</param>
    /// <param name="continuation">What a frame past a program's end reads.</param>
    internal static SamusPoseAnimationStep StepFor(SamusPoseId pose, int frame, LaidOutAnimationContinuation<SamusPoseId, SamusPoseAnimationStep> continuation) =>
        TryStepFor(pose, frame, continuation) ?? throw new InvalidDataException(
            $"Samus animation frame {frame} is not a step of pose {pose}'s program nor of a program laid out after it.");

    /// <summary>
    /// The step of <paramref name="pose"/>'s program at frame <paramref name="frame"/>, read as <see cref="StepFor"/> reads
    /// it, or null when the frame reads past every program, or lands on an operand whose step would publish a record no
    /// gameplay path gives Samus, where the animation fails.
    /// </summary>
    /// <param name="pose">The pose whose program is read.</param>
    /// <param name="frame">Samus's animation frame.</param>
    /// <param name="continuation">What a frame past a program's end reads.</param>
    internal static SamusPoseAnimationStep? TryStepFor(SamusPoseId pose, int frame, LaidOutAnimationContinuation<SamusPoseId, SamusPoseAnimationStep> continuation)
    {
        Ensure.NotNull(continuation);
        SamusPoseId read = pose;
        SamusPoseAnimation program = AnimationOf(read);
        int readFrame = frame;
        int shift = 0;
        SamusPoseAnimationStep? step = program.TryStepAt(readFrame) ?? continuation.OperandStep(read, readFrame);
        while (step is null)
        {
            if (continuation.ReadPast(read, readFrame) is not { } past)
                return null;
            read = past.Animation;
            program = AnimationOf(read);
            readFrame = past.Frame;
            shift += past.Shift;
            step = program.TryStepAt(readFrame) ?? continuation.OperandStep(read, readFrame);
        }
        return shift == 0 ? step : step switch
        {
            SamusAnimationRepeat repeat => new SamusAnimationRepeat(repeat.Frame + shift),
            SamusAnimationWallJumpSelect select => new SamusAnimationWallJumpSelect(
                select.OrdinaryFrame + shift, select.SpaceJumpFrame + shift, select.ScrewAttackFrame + shift),
            _ => step,
        };
    }

    /// <summary>The animation program of <paramref name="pose"/>.</summary>
    private static SamusPoseAnimation AnimationOf(SamusPoseId pose)
    {
        return pose switch
        {
            SamusPoseId.ForwardFacingPowerSuitPose or
            SamusPoseId.ForwardFacingSuitedPose => ForwardFacingPowerSuitAnimation,
            SamusPoseId.FacingRightNormalPose or
            SamusPoseId.FacingLeftNormalPose => FacingRightNormalAnimation,
            SamusPoseId.StandingAimUpRightPose or
            SamusPoseId.StandingAimUpLeftPose or
            SamusPoseId.CrouchingAimUpRightPose or
            SamusPoseId.CrouchingAimUpLeftPose => StandingAimUpRightAnimation,
            SamusPoseId.StandingAimDiagonalUpRightPose or
            SamusPoseId.StandingAimDiagonalUpLeftPose or
            SamusPoseId.StandingAimDiagonalDownRightPose or
            SamusPoseId.StandingAimDiagonalDownLeftPose or
            SamusPoseId.CrouchingAimDiagonalUpRightPose or
            SamusPoseId.CrouchingAimDiagonalUpLeftPose or
            SamusPoseId.CrouchingAimDiagonalDownRightPose or
            SamusPoseId.CrouchingAimDiagonalDownLeftPose or
            SamusPoseId.RanIntoWallRightPose or
            SamusPoseId.RanIntoWallLeftPose or
            SamusPoseId.GrappleCrouchingRightPose or
            SamusPoseId.GrappleCrouchingLeftPose or
            SamusPoseId.GrappleCrouchingDownRightPose or
            SamusPoseId.GrappleCrouchingDownLeftPose or
            SamusPoseId.GrappleWallContactLeftPose or
            SamusPoseId.GrappleWallContactRightPose or
            SamusPoseId.DraygonGrabbedNeutralLeftPose or
            SamusPoseId.DraygonGrabbedAimUpLeftPose or
            SamusPoseId.DraygonGrabbedFiringLeftPose or
            SamusPoseId.DraygonGrabbedAimDownLeftPose or
            SamusPoseId.RanIntoWallAimUpRightPose or
            SamusPoseId.RanIntoWallAimUpLeftPose or
            SamusPoseId.RanIntoWallAimDownRightPose or
            SamusPoseId.RanIntoWallAimDownLeftPose or
            SamusPoseId.DraygonGrabbedNeutralRightPose or
            SamusPoseId.DraygonGrabbedAimUpRightPose or
            SamusPoseId.DraygonGrabbedFiringRightPose or
            SamusPoseId.DraygonGrabbedAimDownRightPose => StandingAimDiagonalUpRightAnimation,
            SamusPoseId.MovingRightNormalPose or
            SamusPoseId.MovingLeftNormalPose or
            SamusPoseId.MovingRightGunExtendedPose or
            SamusPoseId.MovingLeftGunExtendedPose or
            SamusPoseId.RunningAimUpRightPose or
            SamusPoseId.RunningAimUpLeftPose or
            SamusPoseId.RunningAimDiagonalUpRightPose or
            SamusPoseId.RunningAimDiagonalUpLeftPose or
            SamusPoseId.RunningAimDiagonalDownRightPose or
            SamusPoseId.RunningAimDiagonalDownLeftPose => MovingRightNormalAnimation,
            SamusPoseId.NormalJumpGunExtendedRightPose or
            SamusPoseId.NormalJumpGunExtendedLeftPose or
            SamusPoseId.NormalJumpAimDiagonalUpRightPose or
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
            SamusPoseId.NormalJumpAimDiagonalDownRightPose or
            SamusPoseId.NormalJumpAimDiagonalDownLeftPose => NormalJumpGunExtendedRightAnimation,
            SamusPoseId.NormalJumpAimUpRightPose or
            SamusPoseId.NormalJumpAimUpLeftPose => NormalJumpAimUpRightAnimation,
            SamusPoseId.NormalJumpAimDownRightPose or
            SamusPoseId.NormalJumpAimDownLeftPose => NormalJumpAimDownRightAnimation,
            SamusPoseId.SpinJumpRightPose or
            SamusPoseId.SpinJumpLeftPose => SpinJumpRightAnimation,
            SamusPoseId.SpaceJumpRightPose or
            SamusPoseId.SpaceJumpLeftPose => SpaceJumpRightAnimation,
            SamusPoseId.MorphBallGroundRightPose or
            SamusPoseId.MorphBallMovingRightPose or
            SamusPoseId.MorphBallMovingLeftPose or
            SamusPoseId.GlitchBallRightPose or
            SamusPoseId.MorphBallFallingRightPose or
            SamusPoseId.MorphBallFallingLeftPose or
            SamusPoseId.UnusedKnockbackRightPose or
            SamusPoseId.UnusedKnockbackLeftPose or
            SamusPoseId.MorphBallGroundLeftPose or
            SamusPoseId.GlitchBallLeftPose or
            SamusPoseId.SpringBallGroundRightPose or
            SamusPoseId.SpringBallGroundLeftPose or
            SamusPoseId.SpringBallMovingRightPose or
            SamusPoseId.SpringBallMovingLeftPose or
            SamusPoseId.SpringBallFallingRightPose or
            SamusPoseId.SpringBallFallingLeftPose or
            SamusPoseId.SpringBallJumpRightPose or
            SamusPoseId.SpringBallJumpLeftPose or
            SamusPoseId.DraygonHeldBallLeftPose => MorphBallGroundRightAnimation,
            SamusPoseId.TurningRightToLeftPose => TurningRightToLeftAnimation,
            SamusPoseId.TurningLeftToRightPose => TurningLeftToRightAnimation,
            SamusPoseId.CrouchingRightPose or
            SamusPoseId.CrouchingLeftPose => CrouchingRightAnimation,
            SamusPoseId.FallingRightPose or
            SamusPoseId.FallingLeftPose => FallingRightAnimation,
            SamusPoseId.FallingAimUpRightPose or
            SamusPoseId.FallingAimUpLeftPose => FallingAimUpRightAnimation,
            SamusPoseId.FallingAimDownRightPose or
            SamusPoseId.FallingAimDownLeftPose => FallingAimDownRightAnimation,
            SamusPoseId.TurningRightToLeftJumpPose => TurningRightToLeftJumpAnimation,
            SamusPoseId.TurningLeftToRightJumpPose => TurningLeftToRightJumpAnimation,
            SamusPoseId.CrouchingTransitionRightPose => CrouchingTransitionRightAnimation,
            SamusPoseId.CrouchingTransitionLeftPose => CrouchingTransitionLeftAnimation,
            SamusPoseId.MorphingTransitionRightPose => MorphingTransitionRightAnimation,
            SamusPoseId.MorphingTransitionLeftPose => MorphingTransitionLeftAnimation,
            SamusPoseId.GlitchBallMorphingRightPose => GlitchBallMorphingRightAnimation,
            SamusPoseId.GlitchBallMorphingLeftPose => GlitchBallMorphingLeftAnimation,
            SamusPoseId.StandingTransitionRightPose => StandingTransitionRightAnimation,
            SamusPoseId.StandingTransitionLeftPose => StandingTransitionLeftAnimation,
            SamusPoseId.UnmorphingTransitionRightPose => UnmorphingTransitionRightAnimation,
            SamusPoseId.UnmorphingTransitionLeftPose => UnmorphingTransitionLeftAnimation,
            SamusPoseId.InstantMorphingRightPose => InstantMorphingRightAnimation,
            SamusPoseId.InstantMorphingLeftPose => InstantMorphingLeftAnimation,
            SamusPoseId.TurningRightToLeftCrouchingPose => TurningRightToLeftCrouchingAnimation,
            SamusPoseId.TurningLeftToRightCrouchingPose => TurningLeftToRightCrouchingAnimation,
            SamusPoseId.MoonwalkFacingLeftPose or
            SamusPoseId.MoonwalkFacingRightPose or
            SamusPoseId.MoonwalkAimUpLeftPose or
            SamusPoseId.MoonwalkAimUpRightPose or
            SamusPoseId.MoonwalkAimDownLeftPose or
            SamusPoseId.MoonwalkAimDownRightPose => MoonwalkFacingLeftAnimation,
            SamusPoseId.NeutralJumpTransitionRightPose => NeutralJumpTransitionRightAnimation,
            SamusPoseId.NeutralJumpTransitionLeftPose => NeutralJumpTransitionLeftAnimation,
            SamusPoseId.NeutralJumpRightPose or
            SamusPoseId.NeutralJumpLeftPose or
            SamusPoseId.ShinesparkWindupRightPose or
            SamusPoseId.ShinesparkWindupLeftPose => NeutralJumpRightAnimation,
            SamusPoseId.DamageBoostLeftPose or
            SamusPoseId.DamageBoostRightPose => DamageBoostLeftAnimation,
            SamusPoseId.NormalJumpForwardRightPose or
            SamusPoseId.NormalJumpForwardLeftPose => NormalJumpForwardRightAnimation,
            SamusPoseId.KnockbackRightPose or
            SamusPoseId.KnockbackLeftPose => KnockbackRightAnimation,
            SamusPoseId.NormalJumpTransitionAimUpRightPose => NormalJumpTransitionAimUpRightAnimation,
            SamusPoseId.NormalJumpTransitionAimUpLeftPose => NormalJumpTransitionAimUpLeftAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose => NormalJumpTransitionAimDiagonalUpRightAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose => NormalJumpTransitionAimDiagonalUpLeftAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose => NormalJumpTransitionAimDiagonalDownRightAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose => NormalJumpTransitionAimDiagonalDownLeftAnimation,
            SamusPoseId.CompactAirborneRightPose => CompactAirborneRightAnimation,
            SamusPoseId.CompactAirborneLeftPose => CompactAirborneLeftAnimation,
            SamusPoseId.CompactAirborneFallingRightPose or
            SamusPoseId.CompactAirborneFallingLeftPose => CompactAirborneFallingRightAnimation,
            SamusPoseId.FallingGunExtendedRightPose or
            SamusPoseId.FallingGunExtendedLeftPose => FallingGunExtendedRightAnimation,
            SamusPoseId.FallingAimDiagonalUpRightPose or
            SamusPoseId.FallingAimDiagonalUpLeftPose or
            SamusPoseId.FallingAimDiagonalDownRightPose or
            SamusPoseId.FallingAimDiagonalDownLeftPose => FallingAimDiagonalUpRightAnimation,
            SamusPoseId.ScrewAttackRightPose or
            SamusPoseId.ScrewAttackLeftPose => ScrewAttackRightAnimation,
            SamusPoseId.WallJumpRightPose or
            SamusPoseId.WallJumpLeftPose => WallJumpRightAnimation,
            SamusPoseId.TurningRightToLeftFallingPose => TurningRightToLeftFallingAnimation,
            SamusPoseId.TurningLeftToRightFallingPose => TurningLeftToRightFallingAnimation,
            SamusPoseId.TurningRightToLeftAimUpPose => TurningRightToLeftAimUpAnimation,
            SamusPoseId.TurningLeftToRightAimUpPose => TurningLeftToRightAimUpAnimation,
            SamusPoseId.TurningRightToLeftAimDiagonalDownPose => TurningRightToLeftAimDiagonalDownAnimation,
            SamusPoseId.TurningLeftToRightAimDiagonalDownPose => TurningLeftToRightAimDiagonalDownAnimation,
            SamusPoseId.TurningRightToLeftJumpAimUpPose => TurningRightToLeftJumpAimUpAnimation,
            SamusPoseId.TurningLeftToRightJumpAimUpPose => TurningLeftToRightJumpAimUpAnimation,
            SamusPoseId.TurningRightToLeftJumpAimDownPose => TurningRightToLeftJumpAimDownAnimation,
            SamusPoseId.TurningLeftToRightJumpAimDownPose => TurningLeftToRightJumpAimDownAnimation,
            SamusPoseId.TurningRightToLeftFallingAimUpPose => TurningRightToLeftFallingAimUpAnimation,
            SamusPoseId.TurningLeftToRightFallingAimUpPose => TurningLeftToRightFallingAimUpAnimation,
            SamusPoseId.TurningRightToLeftFallingAimDownPose => TurningRightToLeftFallingAimDownAnimation,
            SamusPoseId.TurningLeftToRightFallingAimDownPose => TurningLeftToRightFallingAimDownAnimation,
            SamusPoseId.TurningRightToLeftCrouchingAimUpPose => TurningRightToLeftCrouchingAimUpAnimation,
            SamusPoseId.TurningLeftToRightCrouchingAimUpPose => TurningLeftToRightCrouchingAimUpAnimation,
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose => TurningRightToLeftCrouchingAimDiagonalDownAnimation,
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose => TurningLeftToRightCrouchingAimDiagonalDownAnimation,
            SamusPoseId.TurningRightToLeftAimDiagonalUpPose => TurningRightToLeftAimDiagonalUpAnimation,
            SamusPoseId.TurningLeftToRightAimDiagonalUpPose => TurningLeftToRightAimDiagonalUpAnimation,
            SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose => TurningRightToLeftJumpAimDiagonalUpAnimation,
            SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose => TurningLeftToRightJumpAimDiagonalUpAnimation,
            SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose => TurningRightToLeftFallingAimDiagonalUpAnimation,
            SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose => TurningLeftToRightFallingAimDiagonalUpAnimation,
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose => TurningRightToLeftCrouchingAimDiagonalUpAnimation,
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose => TurningLeftToRightCrouchingAimDiagonalUpAnimation,
            SamusPoseId.NormalLandingRightPose or
            SamusPoseId.FiringLandingRightPose => NormalLandingRightAnimation,
            SamusPoseId.NormalLandingLeftPose or
            SamusPoseId.FiringLandingLeftPose => NormalLandingLeftAnimation,
            SamusPoseId.SpinLandingRightPose => SpinLandingRightAnimation,
            SamusPoseId.SpinLandingLeftPose => SpinLandingLeftAnimation,
            SamusPoseId.GrappleStandingRightPose or
            SamusPoseId.GrappleStandingLeftPose or
            SamusPoseId.GrappleStandingDownRightPose or
            SamusPoseId.GrappleStandingDownLeftPose => GrappleStandingRightAnimation,
            SamusPoseId.GrappleAirborneGunExtendedRightPose => GrappleAirborneGunExtendedRightAnimation,
            SamusPoseId.GrappleAirborneAimDownRightPose => GrappleAirborneAimDownRightAnimation,
            SamusPoseId.GrappleAirborneAimDiagonalDownRightPose => GrappleAirborneAimDiagonalDownRightAnimation,
            SamusPoseId.GrappleSwingRightPose or
            SamusPoseId.GrappleSwingLeftPose => GrappleSwingRightAnimation,
            SamusPoseId.DraygonGrabbedMovingLeftPose or
            SamusPoseId.DraygonGrabbedMovingRightPose => DraygonGrabbedMovingLeftAnimation,
            SamusPoseId.MoonwalkTurnJumpLeftPose => MoonwalkTurnJumpLeftAnimation,
            SamusPoseId.MoonwalkTurnJumpRightPose => MoonwalkTurnJumpRightAnimation,
            SamusPoseId.MoonwalkTurnJumpAimUpLeftPose => MoonwalkTurnJumpAimUpLeftAnimation,
            SamusPoseId.MoonwalkTurnJumpAimUpRightPose => MoonwalkTurnJumpAimUpRightAnimation,
            SamusPoseId.MoonwalkTurnJumpAimDownLeftPose => MoonwalkTurnJumpAimDownLeftAnimation,
            SamusPoseId.MoonwalkTurnJumpAimDownRightPose => MoonwalkTurnJumpAimDownRightAnimation,
            SamusPoseId.DraygonGrabbingLeftPose => DraygonGrabbingLeftAnimation,
            SamusPoseId.ShinesparkHorizontalRightPose or
            SamusPoseId.ShinesparkHorizontalLeftPose or
            SamusPoseId.ShinesparkVerticalRightPose or
            SamusPoseId.ShinesparkVerticalLeftPose or
            SamusPoseId.ShinesparkDiagonalRightPose or
            SamusPoseId.ShinesparkDiagonalLeftPose => ShinesparkHorizontalRightAnimation,
            SamusPoseId.CrystalFlashRightPose => CrystalFlashRightAnimation,
            SamusPoseId.CrystalFlashLeftPose => CrystalFlashLeftAnimation,
            SamusPoseId.XrayingStandingRightPose or
            SamusPoseId.XrayingStandingLeftPose or
            SamusPoseId.XrayingCrouchingRightPose or
            SamusPoseId.XrayingCrouchingLeftPose => XrayingStandingRightAnimation,
            SamusPoseId.DeathSequenceRightPose or
            SamusPoseId.DeathSequenceLeftPose => DeathSequenceRightAnimation,
            SamusPoseId.CompactMorphingRightPose => CompactMorphingRightAnimation,
            SamusPoseId.DraygonHeldMorphingLeftPose => DraygonHeldMorphingLeftAnimation,
            SamusPoseId.CompactStandingUpRightPose => CompactStandingUpRightAnimation,
            SamusPoseId.DraygonHeldStandingUpLeftPose => DraygonHeldStandingUpLeftAnimation,
            SamusPoseId.LandingAimUpRightPose => LandingAimUpRightAnimation,
            SamusPoseId.LandingAimUpLeftPose => LandingAimUpLeftAnimation,
            SamusPoseId.LandingAimDiagonalUpRightPose => LandingAimDiagonalUpRightAnimation,
            SamusPoseId.LandingAimDiagonalUpLeftPose => LandingAimDiagonalUpLeftAnimation,
            SamusPoseId.LandingAimDiagonalDownRightPose => LandingAimDiagonalDownRightAnimation,
            SamusPoseId.LandingAimDiagonalDownLeftPose => LandingAimDiagonalDownLeftAnimation,
            SamusPoseId.DrainedCrouchingRightPose => DrainedCrouchingRightAnimation,
            SamusPoseId.DrainedCrouchingLeftPose => DrainedCrouchingLeftAnimation,
            SamusPoseId.DrainedStandingRightPose => DrainedStandingRightAnimation,
            SamusPoseId.DrainedStandingLeftPose => DrainedStandingLeftAnimation,
            SamusPoseId.CrouchingTransitionAimUpRightPose => CrouchingTransitionAimUpRightAnimation,
            SamusPoseId.CrouchingTransitionAimUpLeftPose => CrouchingTransitionAimUpLeftAnimation,
            SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose => CrouchingTransitionAimDiagonalUpRightAnimation,
            SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose => CrouchingTransitionAimDiagonalUpLeftAnimation,
            SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose => CrouchingTransitionAimDiagonalDownRightAnimation,
            SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose => CrouchingTransitionAimDiagonalDownLeftAnimation,
            SamusPoseId.StandingTransitionAimUpRightPose => StandingTransitionAimUpRightAnimation,
            SamusPoseId.StandingTransitionAimUpLeftPose => StandingTransitionAimUpLeftAnimation,
            SamusPoseId.StandingTransitionAimDiagonalUpRightPose => StandingTransitionAimDiagonalUpRightAnimation,
            SamusPoseId.StandingTransitionAimDiagonalUpLeftPose => StandingTransitionAimDiagonalUpLeftAnimation,
            SamusPoseId.StandingTransitionAimDiagonalDownRightPose => StandingTransitionAimDiagonalDownRightAnimation,
            SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => StandingTransitionAimDiagonalDownLeftAnimation,
            SamusPoseId.FirstUnauthoredPose or
                SamusPoseId.SecondUnauthoredPose or
                SamusPoseId.LastUnauthoredPose => throw new ArgumentOutOfRangeException(nameof(pose)),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };
    }
}
