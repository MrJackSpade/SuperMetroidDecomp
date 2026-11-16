using static SuperMetroid.Core.Samus.Poses.SamusAnimationDelayDefinitions;
using SamusFramesBeyondAnimation = SuperMetroid.Core.Compatibility.FramesBeyondAnimation<SuperMetroid.Core.Samus.Poses.SamusPoseId, SuperMetroid.Core.Samus.Poses.SamusAnimationFrame, SuperMetroid.Core.Samus.Poses.SamusAnimationPlaceName, SuperMetroid.Core.Samus.Poses.SamusPoseAnimationStep>;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Which animation each Samus pose animates by (#1473), and how her animation reads its frames (#2048).</summary>
internal static class SamusAnimationDelays
{
    /// <summary>The animation <paramref name="pose"/> animates by.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The pose is one of the three unauthored poses, which no gameplay path gives Samus.
    /// </exception>
    internal static SamusPoseAnimation AnimationForPose(SamusPoseId pose) => AnimationOf(pose);

    /// <summary>The step of <paramref name="pose"/>'s animation at <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException">The frame reads no step of the pose's animation.</exception>
    /// <remarks>
    /// A frame on a value a cue carries reads the step the value selects, and a frame past the animation's end reads the
    /// frame of a later animation the import decoded for it, both through <paramref name="framesBeyondAnimation"/>, which
    /// names each animation by a pose that animates by it (#1535); a cue read there names its frames in that animation
    /// (<see cref="Target"/>).
    /// </remarks>
    /// <param name="pose">The pose whose animation is read.</param>
    /// <param name="frame">Samus's animation frame.</param>
    /// <param name="framesBeyondAnimation">What a frame that is no step of its animation reads.</param>
    internal static SamusPoseAnimationStep StepFor(SamusPoseId pose, SamusAnimationFrame frame, SamusFramesBeyondAnimation framesBeyondAnimation) =>
        TryStepFor(pose, frame, framesBeyondAnimation) ?? throw new InvalidDataException(
            $"Samus animation frame {frame} is not a step of pose {pose}'s animation nor of an animation laid out after it.");

    /// <summary>
    /// The step of <paramref name="pose"/>'s animation at <paramref name="frame"/>, read as <see cref="StepFor"/> reads it, or
    /// null when the frame reads no step: past every animation, or on a cue value whose step would publish a record no
    /// gameplay path gives Samus, where the animation fails.
    /// </summary>
    /// <param name="pose">The pose whose animation is read.</param>
    /// <param name="frame">Samus's animation frame.</param>
    /// <param name="framesBeyondAnimation">What a frame that is no step of its animation reads.</param>
    /// <exception cref="InvalidDataException">A read past the animation reaches a frame that names no step.</exception>
    internal static SamusPoseAnimationStep? TryStepFor(SamusPoseId pose, SamusAnimationFrame frame, SamusFramesBeyondAnimation framesBeyondAnimation)
    {
        Ensure.NotNull(framesBeyondAnimation);
        if (OwnStep(pose, frame, framesBeyondAnimation) is { } own)
            return own;
        return framesBeyondAnimation.ReadPast(pose, frame) is { } read
            ? OwnStep(read.Animation, read.Frame, framesBeyondAnimation) ?? throw new InvalidDataException(
                $"Samus animation frame {frame} of pose {pose} reads frame {read.Frame} of pose {read.Animation}'s animation, which names no step.")
            : null;
    }

    /// <summary>The frame after <paramref name="frame"/> of <paramref name="pose"/>'s animation, past its end included.</summary>
    internal static SamusAnimationFrame Next(SamusPoseId pose, SamusAnimationFrame frame) => AnimationOf(pose).Next(frame);

    /// <summary>
    /// The frame of <paramref name="pose"/>'s animation a cue read at <paramref name="at"/> moves it to when it names
    /// <paramref name="target"/>: the named frame of the pose's own animation for a cue of its own, or, for a cue a read past
    /// the animation reaches, the frame the import decoded for the frame the cue names in the later animation (#1535).
    /// </summary>
    /// <exception cref="InvalidDataException">The frame <paramref name="at"/> reads no step, or the read names no move to the target.</exception>
    internal static SamusAnimationFrame Target(SamusPoseId pose, SamusAnimationFrame at, SamusAnimationTarget target,
        SamusFramesBeyondAnimation framesBeyondAnimation)
    {
        Ensure.NotNull(framesBeyondAnimation);
        if (OwnStep(pose, at, framesBeyondAnimation) is not null)
            return AnimationOf(pose).Resolve(target);
        SamusFramesBeyondAnimation.Read read = framesBeyondAnimation.ReadPast(pose, at) ?? throw new InvalidDataException(
            $"Samus animation frame {at} of pose {pose} reads no step whose cue could name a frame.");
        SamusAnimationFrame named = AnimationOf(read.Animation).Resolve(target);
        return read.Moves.TryGetValue(named, out SamusAnimationFrame moved)
            ? moved
            : throw new InvalidDataException(
                $"Samus animation frame {at} of pose {pose} reads pose {read.Animation}'s animation, whose frame {named} it has no installed move to.");
    }

    /// <summary>
    /// The frame of <paramref name="pose"/>'s animation <paramref name="place"/> sets: the named frame itself when the pose
    /// animates by the place's animation, otherwise the frame the import decoded for the place in the pose's (#2048).
    /// </summary>
    internal static SamusAnimationFrame Land(SamusPoseId pose, SamusAnimationPlace place, SamusFramesBeyondAnimation framesBeyondAnimation)
    {
        Ensure.NotNull(place.Animation);
        Ensure.NotNull(framesBeyondAnimation);
        return ReferenceEquals(AnimationOf(pose), place.Animation) ? place.Frame : framesBeyondAnimation.Landing(place.Name, pose);
    }

    /// <summary>
    /// Whether <paramref name="frame"/> of <paramref name="pose"/>'s animation has reached <paramref name="place"/>: lies at
    /// it or after it, as the frame <paramref name="place"/> sets on the pose would (<see cref="Land"/>).
    /// </summary>
    internal static bool HasReached(SamusPoseId pose, SamusAnimationFrame frame, SamusAnimationPlace place,
        SamusFramesBeyondAnimation framesBeyondAnimation) =>
        frame >= Land(pose, place, framesBeyondAnimation);

    /// <summary>Whether <paramref name="frame"/> of <paramref name="pose"/>'s animation lies past <paramref name="place"/>.</summary>
    internal static bool HasPassed(SamusPoseId pose, SamusAnimationFrame frame, SamusAnimationPlace place,
        SamusFramesBeyondAnimation framesBeyondAnimation) =>
        frame > Land(pose, place, framesBeyondAnimation);

    /// <summary>
    /// The step <paramref name="frame"/> of <paramref name="pose"/>'s animation names itself: one of its delays or cues, or a
    /// value one of its cues carries; null for a frame read past the animation.
    /// </summary>
    private static SamusPoseAnimationStep? OwnStep(SamusPoseId pose, SamusAnimationFrame frame, SamusFramesBeyondAnimation framesBeyondAnimation) =>
        AnimationOf(pose).TryStepAt(frame) ?? framesBeyondAnimation.CueValueStep(pose, frame);

    /// <summary>The animation of <paramref name="pose"/>.</summary>
    private static SamusPoseAnimation AnimationOf(SamusPoseId pose)
    {
        return pose switch
        {
            SamusPoseId.ForwardFacingPowerSuitPose or
            SamusPoseId.ForwardFacingSuitedPose => ForwardFacingAnimation,
            SamusPoseId.FacingRightNormalPose or
            SamusPoseId.FacingLeftNormalPose => StandingAnimation,
            SamusPoseId.StandingAimUpRightPose or
            SamusPoseId.StandingAimUpLeftPose or
            SamusPoseId.CrouchingAimUpRightPose or
            SamusPoseId.CrouchingAimUpLeftPose => AimingUpAnimation,
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
            SamusPoseId.DraygonGrabbedAimDownRightPose => HeldStillAnimation,
            SamusPoseId.MovingRightNormalPose or
            SamusPoseId.MovingLeftNormalPose or
            SamusPoseId.MovingRightGunExtendedPose or
            SamusPoseId.MovingLeftGunExtendedPose or
            SamusPoseId.RunningAimUpRightPose or
            SamusPoseId.RunningAimUpLeftPose or
            SamusPoseId.RunningAimDiagonalUpRightPose or
            SamusPoseId.RunningAimDiagonalUpLeftPose or
            SamusPoseId.RunningAimDiagonalDownRightPose or
            SamusPoseId.RunningAimDiagonalDownLeftPose => RunningAnimation,
            SamusPoseId.NormalJumpGunExtendedRightPose or
            SamusPoseId.NormalJumpGunExtendedLeftPose or
            SamusPoseId.NormalJumpAimDiagonalUpRightPose or
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
            SamusPoseId.NormalJumpAimDiagonalDownRightPose or
            SamusPoseId.NormalJumpAimDiagonalDownLeftPose => NormalJumpAimingAnimation,
            SamusPoseId.NormalJumpAimUpRightPose or
            SamusPoseId.NormalJumpAimUpLeftPose => NormalJumpAimUpAnimation,
            SamusPoseId.NormalJumpAimDownRightPose or
            SamusPoseId.NormalJumpAimDownLeftPose => NormalJumpAimDownAnimation,
            SamusPoseId.SpinJumpRightPose or
            SamusPoseId.SpinJumpLeftPose => SpinJumpAnimation,
            SamusPoseId.SpaceJumpRightPose or
            SamusPoseId.SpaceJumpLeftPose => SpaceJumpAnimation,
            SamusPoseId.MorphBallGroundRightPose or
            SamusPoseId.MorphBallMovingRightPose or
            SamusPoseId.MorphBallMovingLeftPose or
            SamusPoseId.GlitchBallRightPose or
            SamusPoseId.MorphBallFallingRightPose or
            SamusPoseId.MorphBallFallingLeftPose or
            SamusPoseId.GlitchBallKnockbackRightPose or
            SamusPoseId.GlitchBallKnockbackLeftPose or
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
            SamusPoseId.DraygonHeldBallLeftPose => MorphBallAnimation,
            SamusPoseId.TurningRightToLeftPose => TurningRightToLeftAnimation,
            SamusPoseId.TurningLeftToRightPose => TurningLeftToRightAnimation,
            SamusPoseId.CrouchingRightPose or
            SamusPoseId.CrouchingLeftPose => CrouchingAnimation,
            SamusPoseId.FallingRightPose or
            SamusPoseId.FallingLeftPose => FallingAnimation,
            SamusPoseId.FallingAimUpRightPose or
            SamusPoseId.FallingAimUpLeftPose => FallingAimUpAnimation,
            SamusPoseId.FallingAimDownRightPose or
            SamusPoseId.FallingAimDownLeftPose => FallingAimDownAnimation,
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
            SamusPoseId.MoonwalkAimDownRightPose => MoonwalkAnimation,
            SamusPoseId.NeutralJumpTransitionRightPose => NeutralJumpTransitionRightAnimation,
            SamusPoseId.NeutralJumpTransitionLeftPose => NeutralJumpTransitionLeftAnimation,
            SamusPoseId.NeutralJumpRightPose or
            SamusPoseId.NeutralJumpLeftPose or
            SamusPoseId.ShinesparkWindupRightPose or
            SamusPoseId.ShinesparkWindupLeftPose => NeutralJumpAndWindupAnimation,
            SamusPoseId.DamageBoostLeftPose or
            SamusPoseId.DamageBoostRightPose => DamageBoostAnimation,
            SamusPoseId.NormalJumpForwardRightPose or
            SamusPoseId.NormalJumpForwardLeftPose => NormalJumpForwardAnimation,
            SamusPoseId.KnockbackRightPose or
            SamusPoseId.KnockbackLeftPose => KnockbackAnimation,
            SamusPoseId.NormalJumpTransitionAimUpRightPose => NormalJumpTransitionAimUpRightAnimation,
            SamusPoseId.NormalJumpTransitionAimUpLeftPose => NormalJumpTransitionAimUpLeftAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose => NormalJumpTransitionAimDiagonalUpRightAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose => NormalJumpTransitionAimDiagonalUpLeftAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose => NormalJumpTransitionAimDiagonalDownRightAnimation,
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose => NormalJumpTransitionAimDiagonalDownLeftAnimation,
            SamusPoseId.CompactAirborneRightPose => CompactAirborneRightAnimation,
            SamusPoseId.CompactAirborneLeftPose => CompactAirborneLeftAnimation,
            SamusPoseId.CompactAirborneFallingRightPose or
            SamusPoseId.CompactAirborneFallingLeftPose => CompactAirborneFallingAnimation,
            SamusPoseId.FallingGunExtendedRightPose or
            SamusPoseId.FallingGunExtendedLeftPose => FallingGunExtendedAnimation,
            SamusPoseId.FallingAimDiagonalUpRightPose or
            SamusPoseId.FallingAimDiagonalUpLeftPose or
            SamusPoseId.FallingAimDiagonalDownRightPose or
            SamusPoseId.FallingAimDiagonalDownLeftPose => FallingAimDiagonalAnimation,
            SamusPoseId.ScrewAttackRightPose or
            SamusPoseId.ScrewAttackLeftPose => ScrewAttackAnimation,
            SamusPoseId.WallJumpRightPose or
            SamusPoseId.WallJumpLeftPose => WallJumpAnimation,
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
            SamusPoseId.GrappleStandingDownLeftPose => GrappleStandingAnimation,
            SamusPoseId.GrappleAirborneGunExtendedRightPose => GrappleAirborneGunExtendedRightAnimation,
            SamusPoseId.GrappleAirborneAimDownRightPose => GrappleAirborneAimDownRightAnimation,
            SamusPoseId.GrappleAirborneAimDiagonalDownRightPose => GrappleAirborneAimDiagonalDownRightAnimation,
            SamusPoseId.GrappleSwingRightPose or
            SamusPoseId.GrappleSwingLeftPose => GrappleSwingAnimation,
            SamusPoseId.DraygonGrabbedMovingLeftPose or
            SamusPoseId.DraygonGrabbedMovingRightPose => DraygonGrabbedMovingAnimation,
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
            SamusPoseId.ShinesparkDiagonalLeftPose => ShinesparkAnimation,
            SamusPoseId.CrystalFlashRightPose => CrystalFlashRightAnimation,
            SamusPoseId.CrystalFlashLeftPose => CrystalFlashLeftAnimation,
            SamusPoseId.XrayingStandingRightPose or
            SamusPoseId.XrayingStandingLeftPose or
            SamusPoseId.XrayingCrouchingRightPose or
            SamusPoseId.XrayingCrouchingLeftPose => XrayingAnimation,
            SamusPoseId.DeathSequenceRightPose or
            SamusPoseId.DeathSequenceLeftPose => DeathSequenceAnimation,
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
            SamusPoseId.TallStillRightPose or
                SamusPoseId.ForwardStandingPose or
                SamusPoseId.ForwardStillCrouchHeightPose => throw new ArgumentOutOfRangeException(nameof(pose)),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };
    }
}
