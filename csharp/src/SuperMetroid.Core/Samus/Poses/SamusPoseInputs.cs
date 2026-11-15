using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputDefinitions;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputList;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Which pose each pose's input list selects for the held and newly pressed buttons.</summary>
internal static class SamusPoseInputs
{
    /// <summary>Maps a supported pose to its transition list: poses that share a list's transitions share the list.</summary>
    /// <returns>False only for an unauthored pose, which has no input list.</returns>
    internal static bool TryGetInputList(SamusPoseId pose, out SamusPoseInputList inputList)
    {
        // The unauthored poses have no input list.
        if (pose is SamusPoseId.TallStillRightPose or SamusPoseId.ForwardStandingPose or SamusPoseId.ForwardStillCrouchHeightPose)
        {
            inputList = default;
            return false;
        }
        inputList = pose switch
        {
            SamusPoseId.TurningRightToLeftJumpPose or SamusPoseId.TurningLeftToRightJumpPose or SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or SamusPoseId.MorphingTransitionRightPose or SamusPoseId.MorphingTransitionLeftPose or SamusPoseId.GlitchBallMorphingRightPose or SamusPoseId.GlitchBallMorphingLeftPose or SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose or SamusPoseId.InstantMorphingRightPose or SamusPoseId.InstantMorphingLeftPose or SamusPoseId.TurningRightToLeftCrouchingPose or SamusPoseId.TurningLeftToRightCrouchingPose or SamusPoseId.TurningRightToLeftFallingPose or SamusPoseId.TurningLeftToRightFallingPose or SamusPoseId.TurningRightToLeftJumpAimUpPose or SamusPoseId.TurningLeftToRightJumpAimUpPose or SamusPoseId.TurningRightToLeftJumpAimDownPose or SamusPoseId.TurningLeftToRightJumpAimDownPose or SamusPoseId.TurningRightToLeftFallingAimUpPose or SamusPoseId.TurningLeftToRightFallingAimUpPose or SamusPoseId.TurningRightToLeftFallingAimDownPose or SamusPoseId.TurningLeftToRightFallingAimDownPose or SamusPoseId.TurningRightToLeftCrouchingAimUpPose or SamusPoseId.TurningLeftToRightCrouchingAimUpPose or SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or SamusPoseId.TurningRightToLeftAimDiagonalUpPose or SamusPoseId.TurningLeftToRightAimDiagonalUpPose or SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or SamusPoseId.GrappleStandingRightPose or SamusPoseId.GrappleStandingLeftPose or SamusPoseId.GrappleStandingDownRightPose or SamusPoseId.GrappleStandingDownLeftPose or SamusPoseId.GrappleAirborneGunExtendedRightPose or SamusPoseId.GrappleAirborneAimDownRightPose or SamusPoseId.GrappleAirborneAimDiagonalDownRightPose or SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose or SamusPoseId.GrappleCrouchingRightPose or SamusPoseId.GrappleCrouchingLeftPose or SamusPoseId.GrappleCrouchingDownRightPose or SamusPoseId.GrappleCrouchingDownLeftPose or SamusPoseId.GrappleWallContactLeftPose or SamusPoseId.GrappleWallContactRightPose or SamusPoseId.DraygonGrabbingLeftPose or SamusPoseId.ShinesparkHorizontalRightPose or SamusPoseId.ShinesparkHorizontalLeftPose or SamusPoseId.ShinesparkVerticalRightPose or SamusPoseId.ShinesparkVerticalLeftPose or SamusPoseId.ShinesparkDiagonalRightPose or SamusPoseId.ShinesparkDiagonalLeftPose or SamusPoseId.CrystalFlashRightPose or SamusPoseId.CrystalFlashLeftPose or SamusPoseId.XrayingStandingRightPose or SamusPoseId.XrayingStandingLeftPose or SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose or SamusPoseId.XrayingCrouchingRightPose or SamusPoseId.XrayingCrouchingLeftPose or SamusPoseId.CompactMorphingRightPose or SamusPoseId.DraygonHeldMorphingLeftPose or SamusPoseId.CompactStandingUpRightPose or SamusPoseId.DraygonHeldStandingUpLeftPose or SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose or SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose or SamusPoseId.CrouchingTransitionAimUpRightPose or SamusPoseId.CrouchingTransitionAimUpLeftPose or SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or SamusPoseId.StandingTransitionAimUpRightPose or SamusPoseId.StandingTransitionAimUpLeftPose or SamusPoseId.StandingTransitionAimDiagonalUpRightPose or SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or SamusPoseId.StandingTransitionAimDiagonalDownRightPose or SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => NoInputTransitions,
            SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose => ForwardFacing,
            SamusPoseId.FacingRightNormalPose or SamusPoseId.StandingAimUpRightPose or SamusPoseId.StandingAimDiagonalUpRightPose or SamusPoseId.StandingAimDiagonalDownRightPose or SamusPoseId.NormalLandingRightPose or SamusPoseId.SpinLandingRightPose or SamusPoseId.LandingAimUpRightPose or SamusPoseId.LandingAimDiagonalUpRightPose or SamusPoseId.LandingAimDiagonalDownRightPose or SamusPoseId.FiringLandingRightPose => StandingAndLandingRight,
            SamusPoseId.FacingLeftNormalPose or SamusPoseId.StandingAimUpLeftPose or SamusPoseId.StandingAimDiagonalUpLeftPose or SamusPoseId.StandingAimDiagonalDownLeftPose or SamusPoseId.NormalLandingLeftPose or SamusPoseId.SpinLandingLeftPose or SamusPoseId.LandingAimUpLeftPose or SamusPoseId.LandingAimDiagonalUpLeftPose or SamusPoseId.LandingAimDiagonalDownLeftPose or SamusPoseId.FiringLandingLeftPose => StandingAndLandingLeft,
            SamusPoseId.MovingRightNormalPose or SamusPoseId.RunningAimUpRightPose or SamusPoseId.RunningAimDiagonalUpRightPose or SamusPoseId.RunningAimDiagonalDownRightPose => RunningRight,
            SamusPoseId.MovingLeftNormalPose or SamusPoseId.RunningAimUpLeftPose or SamusPoseId.RunningAimDiagonalUpLeftPose or SamusPoseId.RunningAimDiagonalDownLeftPose => RunningLeft,
            SamusPoseId.NeutralJumpTransitionRightPose or SamusPoseId.NormalJumpTransitionAimUpRightPose or SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose => JumpTransitionRight,
            SamusPoseId.NeutralJumpTransitionLeftPose or SamusPoseId.NormalJumpTransitionAimUpLeftPose or SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose => JumpTransitionLeft,
            SamusPoseId.NormalJumpAimUpRightPose or SamusPoseId.NeutralJumpRightPose or SamusPoseId.NormalJumpForwardRightPose or SamusPoseId.NormalJumpAimDiagonalUpRightPose or SamusPoseId.NormalJumpAimDiagonalDownRightPose => NormalJumpRight,
            SamusPoseId.NormalJumpAimUpLeftPose or SamusPoseId.NeutralJumpLeftPose or SamusPoseId.NormalJumpForwardLeftPose or SamusPoseId.NormalJumpAimDiagonalUpLeftPose or SamusPoseId.NormalJumpAimDiagonalDownLeftPose => NormalJumpLeft,
            SamusPoseId.DamageBoostLeftPose => DamageBoostLeft,
            SamusPoseId.DamageBoostRightPose => DamageBoostRight,
            SamusPoseId.SpinJumpRightPose => SpinJumpRight,
            SamusPoseId.SpinJumpLeftPose => SpinJumpLeft,
            SamusPoseId.SpaceJumpRightPose => SpaceJumpRight,
            SamusPoseId.SpaceJumpLeftPose => SpaceJumpLeft,
            SamusPoseId.ScrewAttackRightPose => ScrewAttackRight,
            SamusPoseId.ScrewAttackLeftPose => ScrewAttackLeft,
            SamusPoseId.MorphBallGroundRightPose => MorphBallGroundRight,
            SamusPoseId.MorphBallMovingRightPose => MorphBallMovingRight,
            SamusPoseId.MorphBallMovingLeftPose => MorphBallMovingLeft,
            SamusPoseId.MorphBallGroundLeftPose => MorphBallGroundLeft,
            SamusPoseId.GlitchBallRightPose => GlitchBallRight,
            SamusPoseId.GlitchBallLeftPose => GlitchBallLeft,
            SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingAimDiagonalUpRightPose or SamusPoseId.CrouchingAimDiagonalDownRightPose or SamusPoseId.CrouchingAimUpRightPose => CrouchingRight,
            SamusPoseId.CrouchingLeftPose or SamusPoseId.CrouchingAimDiagonalUpLeftPose or SamusPoseId.CrouchingAimDiagonalDownLeftPose or SamusPoseId.CrouchingAimUpLeftPose => CrouchingLeft,
            SamusPoseId.FallingRightPose or SamusPoseId.FallingAimUpRightPose or SamusPoseId.FallingAimDiagonalUpRightPose or SamusPoseId.FallingAimDiagonalDownRightPose => FallingRight,
            SamusPoseId.FallingLeftPose or SamusPoseId.FallingAimUpLeftPose or SamusPoseId.FallingAimDiagonalUpLeftPose or SamusPoseId.FallingAimDiagonalDownLeftPose => FallingLeft,
            SamusPoseId.MorphBallFallingRightPose => MorphBallFallingRight,
            SamusPoseId.MorphBallFallingLeftPose => MorphBallFallingLeft,
            SamusPoseId.GlitchBallKnockbackRightPose => GlitchBallKnockbackRight,
            SamusPoseId.GlitchBallKnockbackLeftPose => GlitchBallKnockbackLeft,
            SamusPoseId.MoonwalkFacingLeftPose or SamusPoseId.MoonwalkAimUpLeftPose or SamusPoseId.MoonwalkAimDownLeftPose => MoonwalkLeft,
            SamusPoseId.MoonwalkFacingRightPose or SamusPoseId.MoonwalkAimUpRightPose or SamusPoseId.MoonwalkAimDownRightPose => MoonwalkRight,
            SamusPoseId.KnockbackRightPose => KnockbackRight,
            SamusPoseId.KnockbackLeftPose => KnockbackLeft,
            SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallMovingRightPose => SpringBallGroundRight,
            SamusPoseId.SpringBallGroundLeftPose or SamusPoseId.SpringBallMovingLeftPose => SpringBallGroundLeft,
            SamusPoseId.SpringBallFallingRightPose => SpringBallFallingRight,
            SamusPoseId.SpringBallFallingLeftPose => SpringBallFallingLeft,
            SamusPoseId.SpringBallJumpRightPose => SpringBallJumpRight,
            SamusPoseId.SpringBallJumpLeftPose => SpringBallJumpLeft,
            SamusPoseId.CompactAirborneRightPose => CompactAirborneRight,
            SamusPoseId.CompactAirborneLeftPose => CompactAirborneLeft,
            SamusPoseId.CompactAirborneFallingRightPose => CompactAirborneFallingRight,
            SamusPoseId.CompactAirborneFallingLeftPose => CompactAirborneFallingLeft,
            SamusPoseId.WallJumpRightPose => WallJumpRight,
            SamusPoseId.WallJumpLeftPose => WallJumpLeft,
            SamusPoseId.RanIntoWallRightPose or SamusPoseId.RanIntoWallAimUpRightPose or SamusPoseId.RanIntoWallAimDownRightPose => RanIntoWallRight,
            SamusPoseId.RanIntoWallLeftPose or SamusPoseId.RanIntoWallAimUpLeftPose or SamusPoseId.RanIntoWallAimDownLeftPose => RanIntoWallLeft,
            SamusPoseId.NormalJumpGunExtendedRightPose => NormalJumpGunExtendedRight,
            SamusPoseId.NormalJumpGunExtendedLeftPose => NormalJumpGunExtendedLeft,
            SamusPoseId.NormalJumpAimDownRightPose => NormalJumpAimDownRight,
            SamusPoseId.NormalJumpAimDownLeftPose => NormalJumpAimDownLeft,
            SamusPoseId.UnmorphingTransitionRightPose => UnmorphingTransitionRight,
            SamusPoseId.UnmorphingTransitionLeftPose => UnmorphingTransitionLeft,
            SamusPoseId.TurningRightToLeftPose => TurningRightToLeft,
            SamusPoseId.TurningLeftToRightPose => TurningLeftToRight,
            SamusPoseId.TurningRightToLeftAimUpPose => TurningRightToLeftAimUp,
            SamusPoseId.TurningLeftToRightAimUpPose => TurningLeftToRightAimUp,
            SamusPoseId.TurningRightToLeftAimDiagonalDownPose => TurningRightToLeftAimDiagonalDown,
            SamusPoseId.TurningLeftToRightAimDiagonalDownPose => TurningLeftToRightAimDiagonalDown,
            SamusPoseId.ShinesparkWindupRightPose => ShinesparkWindupRight,
            SamusPoseId.ShinesparkWindupLeftPose => ShinesparkWindupLeft,
            SamusPoseId.FallingAimDownRightPose => FallingAimDownRight,
            SamusPoseId.FallingAimDownLeftPose => FallingAimDownLeft,
            SamusPoseId.DraygonHeldBallLeftPose => DraygonHeldBallLeft,
            SamusPoseId.DraygonGrabbedNeutralLeftPose or SamusPoseId.DraygonGrabbedAimUpLeftPose or SamusPoseId.DraygonGrabbedFiringLeftPose or SamusPoseId.DraygonGrabbedAimDownLeftPose or SamusPoseId.DraygonGrabbedMovingLeftPose => DraygonGrabbedLeft,
            SamusPoseId.DraygonGrabbedNeutralRightPose or SamusPoseId.DraygonGrabbedAimUpRightPose or SamusPoseId.DraygonGrabbedFiringRightPose or SamusPoseId.DraygonGrabbedAimDownRightPose or SamusPoseId.DraygonGrabbedMovingRightPose => DraygonGrabbedRight,
            SamusPoseId.MovingRightGunExtendedPose => MovingRightGunExtended,
            SamusPoseId.MovingLeftGunExtendedPose => MovingLeftGunExtended,
            SamusPoseId.FallingGunExtendedRightPose => FallingGunExtendedRight,
            SamusPoseId.FallingGunExtendedLeftPose => FallingGunExtendedLeft,
            SamusPoseId.MoonwalkTurnJumpLeftPose => MoonwalkTurnJumpLeft,
            SamusPoseId.MoonwalkTurnJumpRightPose => MoonwalkTurnJumpRight,
            SamusPoseId.MoonwalkTurnJumpAimUpLeftPose => MoonwalkTurnJumpAimUpLeft,
            SamusPoseId.MoonwalkTurnJumpAimUpRightPose => MoonwalkTurnJumpAimUpRight,
            SamusPoseId.MoonwalkTurnJumpAimDownLeftPose => MoonwalkTurnJumpAimDownLeft,
            SamusPoseId.MoonwalkTurnJumpAimDownRightPose => MoonwalkTurnJumpAimDownRight,
            SamusPoseId.TallStillRightPose or
                SamusPoseId.ForwardStandingPose or
                SamusPoseId.ForwardStillCrouchHeightPose => throw new InvalidOperationException($"Pose {pose} has no compiled input graph."),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };
        return true;
    }

    /// <summary>Evaluates the selected transition list using held and edge inputs.</summary>
    internal static SamusPoseInputMatch Match(SamusPoseInputList inputList, ControllerButton held, ControllerButton newlyPressed) =>
        SamusPoseInputRules.Match(inputList, held, newlyPressed);

    /// <summary>
    /// Exchanges Left and Right in <paramref name="input"/>. A left-facing list is its right-facing list read with the
    /// directions exchanged, its selected pose mirrored back.
    /// </summary>
    internal static ControllerButton MirrorButtons(ControllerButton input) =>
        (input & ~(ControllerButton.Left | ControllerButton.Right)) |
        ((input & ControllerButton.Left) != ControllerButton.None ? ControllerButton.Right : ControllerButton.None) |
        ((input & ControllerButton.Right) != ControllerButton.None ? ControllerButton.Left : ControllerButton.None);

    /// <summary>Requires every bit of an authored canonical input chord to be present.</summary>
    internal static bool Has(ControllerButton input, CanonicalPoseButtons required) =>
        (input & Buttons(required)) == Buttons(required);

    /// <summary>The controller buttons an authored chord names.</summary>
    private static ControllerButton Buttons(CanonicalPoseButtons chord)
    {
        ControllerButton buttons = ControllerButton.None;
        foreach ((CanonicalPoseButtons bit, ControllerButton button) in ChordButtons)
        {
            if ((chord & bit) == bit)
                buttons |= button;
        }
        return buttons;
    }

    /// <summary>Marks a selected target as a matched condition, distinct from an empty transition list.</summary>
    internal static SamusPoseInputMatch Accept(SamusPoseId target) =>
        new(new(target), HasConditions: true);
}
