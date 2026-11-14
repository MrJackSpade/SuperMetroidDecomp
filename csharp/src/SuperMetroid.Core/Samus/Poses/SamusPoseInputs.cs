using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputDefinitions;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputList;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Which pose each pose's input list selects for the held and newly pressed buttons.</summary>
internal static class SamusPoseInputs
{
    /// <summary>Maps a supported pose to its transition list, preserving shared and empty-list identities.</summary>
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
            SamusPoseId.TurningRightToLeftJumpPose or SamusPoseId.TurningLeftToRightJumpPose or SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or SamusPoseId.MorphingTransitionRightPose or SamusPoseId.MorphingTransitionLeftPose or SamusPoseId.GlitchBallMorphingRightPose or SamusPoseId.GlitchBallMorphingLeftPose or SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose or SamusPoseId.InstantMorphingRightPose or SamusPoseId.InstantMorphingLeftPose or SamusPoseId.TurningRightToLeftCrouchingPose or SamusPoseId.TurningLeftToRightCrouchingPose or SamusPoseId.TurningRightToLeftFallingPose or SamusPoseId.TurningLeftToRightFallingPose or SamusPoseId.TurningRightToLeftJumpAimUpPose or SamusPoseId.TurningLeftToRightJumpAimUpPose or SamusPoseId.TurningRightToLeftJumpAimDownPose or SamusPoseId.TurningLeftToRightJumpAimDownPose or SamusPoseId.TurningRightToLeftFallingAimUpPose or SamusPoseId.TurningLeftToRightFallingAimUpPose or SamusPoseId.TurningRightToLeftFallingAimDownPose or SamusPoseId.TurningLeftToRightFallingAimDownPose or SamusPoseId.TurningRightToLeftCrouchingAimUpPose or SamusPoseId.TurningLeftToRightCrouchingAimUpPose or SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or SamusPoseId.TurningRightToLeftAimDiagonalUpPose or SamusPoseId.TurningLeftToRightAimDiagonalUpPose or SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or SamusPoseId.GrappleStandingRightPose or SamusPoseId.GrappleStandingLeftPose or SamusPoseId.GrappleStandingDownRightPose or SamusPoseId.GrappleStandingDownLeftPose or SamusPoseId.GrappleAirborneGunExtendedRightPose or SamusPoseId.GrappleAirborneAimDownRightPose or SamusPoseId.GrappleAirborneAimDiagonalDownRightPose or SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose or SamusPoseId.GrappleCrouchingRightPose or SamusPoseId.GrappleCrouchingLeftPose or SamusPoseId.GrappleCrouchingDownRightPose or SamusPoseId.GrappleCrouchingDownLeftPose or SamusPoseId.GrappleWallContactLeftPose or SamusPoseId.GrappleWallContactRightPose or SamusPoseId.DraygonGrabbingLeftPose or SamusPoseId.ShinesparkHorizontalRightPose or SamusPoseId.ShinesparkHorizontalLeftPose or SamusPoseId.ShinesparkVerticalRightPose or SamusPoseId.ShinesparkVerticalLeftPose or SamusPoseId.ShinesparkDiagonalRightPose or SamusPoseId.ShinesparkDiagonalLeftPose or SamusPoseId.CrystalFlashRightPose or SamusPoseId.CrystalFlashLeftPose or SamusPoseId.XrayingStandingRightPose or SamusPoseId.XrayingStandingLeftPose or SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose or SamusPoseId.XrayingCrouchingRightPose or SamusPoseId.XrayingCrouchingLeftPose or SamusPoseId.CompactMorphingRightPose or SamusPoseId.DraygonHeldMorphingLeftPose or SamusPoseId.CompactStandingUpRightPose or SamusPoseId.DraygonHeldStandingUpLeftPose or SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose or SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose or SamusPoseId.CrouchingTransitionAimUpRightPose or SamusPoseId.CrouchingTransitionAimUpLeftPose or SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or SamusPoseId.StandingTransitionAimUpRightPose or SamusPoseId.StandingTransitionAimUpLeftPose or SamusPoseId.StandingTransitionAimDiagonalUpRightPose or SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or SamusPoseId.StandingTransitionAimDiagonalDownRightPose or SamusPoseId.StandingTransitionAimDiagonalDownLeftPose => EmptyTransitionList,
            SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose => ForwardFacingPowerSuitPoseList,
            SamusPoseId.FacingRightNormalPose or SamusPoseId.StandingAimUpRightPose or SamusPoseId.StandingAimDiagonalUpRightPose or SamusPoseId.StandingAimDiagonalDownRightPose or SamusPoseId.NormalLandingRightPose or SamusPoseId.SpinLandingRightPose or SamusPoseId.LandingAimUpRightPose or SamusPoseId.LandingAimDiagonalUpRightPose or SamusPoseId.LandingAimDiagonalDownRightPose or SamusPoseId.FiringLandingRightPose => FacingRightNormalPoseList,
            SamusPoseId.FacingLeftNormalPose or SamusPoseId.StandingAimUpLeftPose or SamusPoseId.StandingAimDiagonalUpLeftPose or SamusPoseId.StandingAimDiagonalDownLeftPose or SamusPoseId.NormalLandingLeftPose or SamusPoseId.SpinLandingLeftPose or SamusPoseId.LandingAimUpLeftPose or SamusPoseId.LandingAimDiagonalUpLeftPose or SamusPoseId.LandingAimDiagonalDownLeftPose or SamusPoseId.FiringLandingLeftPose => FacingLeftNormalPoseList,
            SamusPoseId.MovingRightNormalPose or SamusPoseId.RunningAimUpRightPose or SamusPoseId.RunningAimDiagonalUpRightPose or SamusPoseId.RunningAimDiagonalDownRightPose => MovingRightNormalPoseList,
            SamusPoseId.MovingLeftNormalPose or SamusPoseId.RunningAimUpLeftPose or SamusPoseId.RunningAimDiagonalUpLeftPose or SamusPoseId.RunningAimDiagonalDownLeftPose => MovingLeftNormalPoseList,
            SamusPoseId.NeutralJumpTransitionRightPose or SamusPoseId.NormalJumpTransitionAimUpRightPose or SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose => NeutralJumpTransitionRightPoseList,
            SamusPoseId.NeutralJumpTransitionLeftPose or SamusPoseId.NormalJumpTransitionAimUpLeftPose or SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose => NeutralJumpTransitionLeftPoseList,
            SamusPoseId.NormalJumpAimUpRightPose or SamusPoseId.NeutralJumpRightPose or SamusPoseId.NormalJumpForwardRightPose or SamusPoseId.NormalJumpAimDiagonalUpRightPose or SamusPoseId.NormalJumpAimDiagonalDownRightPose => NormalJumpAimUpRightPoseList,
            SamusPoseId.NormalJumpAimUpLeftPose or SamusPoseId.NeutralJumpLeftPose or SamusPoseId.NormalJumpForwardLeftPose or SamusPoseId.NormalJumpAimDiagonalUpLeftPose or SamusPoseId.NormalJumpAimDiagonalDownLeftPose => NormalJumpAimUpLeftPoseList,
            SamusPoseId.DamageBoostLeftPose => DamageBoostLeftPoseList,
            SamusPoseId.DamageBoostRightPose => DamageBoostRightPoseList,
            SamusPoseId.SpinJumpRightPose => SpinJumpRightPoseList,
            SamusPoseId.SpinJumpLeftPose => SpinJumpLeftPoseList,
            SamusPoseId.SpaceJumpRightPose => SpaceJumpRightPoseList,
            SamusPoseId.SpaceJumpLeftPose => SpaceJumpLeftPoseList,
            SamusPoseId.ScrewAttackRightPose => ScrewAttackRightPoseList,
            SamusPoseId.ScrewAttackLeftPose => ScrewAttackLeftPoseList,
            SamusPoseId.MorphBallGroundRightPose => MorphBallGroundRightPoseList,
            SamusPoseId.MorphBallMovingRightPose => MorphBallMovingRightPoseList,
            SamusPoseId.MorphBallMovingLeftPose => MorphBallMovingLeftPoseList,
            SamusPoseId.MorphBallGroundLeftPose => MorphBallGroundLeftPoseList,
            SamusPoseId.GlitchBallRightPose => GlitchBallRightPoseList,
            SamusPoseId.GlitchBallLeftPose => GlitchBallLeftPoseList,
            SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingAimDiagonalUpRightPose or SamusPoseId.CrouchingAimDiagonalDownRightPose or SamusPoseId.CrouchingAimUpRightPose => CrouchingRightPoseList,
            SamusPoseId.CrouchingLeftPose or SamusPoseId.CrouchingAimDiagonalUpLeftPose or SamusPoseId.CrouchingAimDiagonalDownLeftPose or SamusPoseId.CrouchingAimUpLeftPose => CrouchingLeftPoseList,
            SamusPoseId.FallingRightPose or SamusPoseId.FallingAimUpRightPose or SamusPoseId.FallingAimDiagonalUpRightPose or SamusPoseId.FallingAimDiagonalDownRightPose => FallingRightPoseList,
            SamusPoseId.FallingLeftPose or SamusPoseId.FallingAimUpLeftPose or SamusPoseId.FallingAimDiagonalUpLeftPose or SamusPoseId.FallingAimDiagonalDownLeftPose => FallingLeftPoseList,
            SamusPoseId.MorphBallFallingRightPose => MorphBallFallingRightPoseList,
            SamusPoseId.MorphBallFallingLeftPose => MorphBallFallingLeftPoseList,
            SamusPoseId.GlitchBallKnockbackRightPose => GlitchBallKnockbackRightPoseList,
            SamusPoseId.GlitchBallKnockbackLeftPose => GlitchBallKnockbackLeftPoseList,
            SamusPoseId.MoonwalkFacingLeftPose or SamusPoseId.MoonwalkAimUpLeftPose or SamusPoseId.MoonwalkAimDownLeftPose => MoonwalkFacingLeftPoseList,
            SamusPoseId.MoonwalkFacingRightPose or SamusPoseId.MoonwalkAimUpRightPose or SamusPoseId.MoonwalkAimDownRightPose => MoonwalkFacingRightPoseList,
            SamusPoseId.KnockbackRightPose => KnockbackRightPoseList,
            SamusPoseId.KnockbackLeftPose => KnockbackLeftPoseList,
            SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallMovingRightPose => SpringBallGroundRightPoseList,
            SamusPoseId.SpringBallGroundLeftPose or SamusPoseId.SpringBallMovingLeftPose => SpringBallGroundLeftPoseList,
            SamusPoseId.SpringBallFallingRightPose => SpringBallFallingRightPoseList,
            SamusPoseId.SpringBallFallingLeftPose => SpringBallFallingLeftPoseList,
            SamusPoseId.SpringBallJumpRightPose => SpringBallJumpRightPoseList,
            SamusPoseId.SpringBallJumpLeftPose => SpringBallJumpLeftPoseList,
            SamusPoseId.CompactAirborneRightPose => CompactAirborneRightPoseList,
            SamusPoseId.CompactAirborneLeftPose => CompactAirborneLeftPoseList,
            SamusPoseId.CompactAirborneFallingRightPose => CompactAirborneFallingRightPoseList,
            SamusPoseId.CompactAirborneFallingLeftPose => CompactAirborneFallingLeftPoseList,
            SamusPoseId.WallJumpRightPose => WallJumpRightPoseList,
            SamusPoseId.WallJumpLeftPose => WallJumpLeftPoseList,
            SamusPoseId.RanIntoWallRightPose or SamusPoseId.RanIntoWallAimUpRightPose or SamusPoseId.RanIntoWallAimDownRightPose => RanIntoWallRightPoseList,
            SamusPoseId.RanIntoWallLeftPose or SamusPoseId.RanIntoWallAimUpLeftPose or SamusPoseId.RanIntoWallAimDownLeftPose => RanIntoWallLeftPoseList,
            SamusPoseId.NormalJumpGunExtendedRightPose => NormalJumpGunExtendedRightPoseList,
            SamusPoseId.NormalJumpGunExtendedLeftPose => NormalJumpGunExtendedLeftPoseList,
            SamusPoseId.NormalJumpAimDownRightPose => NormalJumpAimDownRightPoseList,
            SamusPoseId.NormalJumpAimDownLeftPose => NormalJumpAimDownLeftPoseList,
            SamusPoseId.UnmorphingTransitionRightPose => UnmorphingTransitionRightPoseList,
            SamusPoseId.UnmorphingTransitionLeftPose => UnmorphingTransitionLeftPoseList,
            SamusPoseId.TurningRightToLeftPose => TurningRightToLeftPoseList,
            SamusPoseId.TurningLeftToRightPose => TurningLeftToRightPoseList,
            SamusPoseId.TurningRightToLeftAimUpPose => TurningRightToLeftAimUpPoseList,
            SamusPoseId.TurningLeftToRightAimUpPose => TurningLeftToRightAimUpPoseList,
            SamusPoseId.TurningRightToLeftAimDiagonalDownPose => TurningRightToLeftAimDiagonalDownPoseList,
            SamusPoseId.TurningLeftToRightAimDiagonalDownPose => TurningLeftToRightAimDiagonalDownPoseList,
            SamusPoseId.ShinesparkWindupRightPose => ShinesparkWindupRightPoseList,
            SamusPoseId.ShinesparkWindupLeftPose => ShinesparkWindupLeftPoseList,
            SamusPoseId.FallingAimDownRightPose => FallingAimDownRightPoseList,
            SamusPoseId.FallingAimDownLeftPose => FallingAimDownLeftPoseList,
            SamusPoseId.DraygonHeldBallLeftPose => DraygonHeldBallLeftPoseList,
            SamusPoseId.DraygonGrabbedNeutralLeftPose or SamusPoseId.DraygonGrabbedAimUpLeftPose or SamusPoseId.DraygonGrabbedFiringLeftPose or SamusPoseId.DraygonGrabbedAimDownLeftPose or SamusPoseId.DraygonGrabbedMovingLeftPose => DraygonGrabbedNeutralLeftPoseList,
            SamusPoseId.DraygonGrabbedNeutralRightPose or SamusPoseId.DraygonGrabbedAimUpRightPose or SamusPoseId.DraygonGrabbedFiringRightPose or SamusPoseId.DraygonGrabbedAimDownRightPose or SamusPoseId.DraygonGrabbedMovingRightPose => DraygonGrabbedNeutralRightPoseList,
            SamusPoseId.MovingRightGunExtendedPose => MovingRightGunExtendedPoseList,
            SamusPoseId.MovingLeftGunExtendedPose => MovingLeftGunExtendedPoseList,
            SamusPoseId.FallingGunExtendedRightPose => FallingGunExtendedRightPoseList,
            SamusPoseId.FallingGunExtendedLeftPose => FallingGunExtendedLeftPoseList,
            SamusPoseId.MoonwalkTurnJumpLeftPose => MoonwalkTurnJumpLeftPoseList,
            SamusPoseId.MoonwalkTurnJumpRightPose => MoonwalkTurnJumpRightPoseList,
            SamusPoseId.MoonwalkTurnJumpAimUpLeftPose => MoonwalkTurnJumpAimUpLeftPoseList,
            SamusPoseId.MoonwalkTurnJumpAimUpRightPose => MoonwalkTurnJumpAimUpRightPoseList,
            SamusPoseId.MoonwalkTurnJumpAimDownLeftPose => MoonwalkTurnJumpAimDownLeftPoseList,
            SamusPoseId.MoonwalkTurnJumpAimDownRightPose => MoonwalkTurnJumpAimDownRightPoseList,
            SamusPoseId.TallStillRightPose or
                SamusPoseId.ForwardStandingPose or
                SamusPoseId.ForwardStillCrouchHeightPose => throw new InvalidOperationException($"Pose {pose} has no compiled input graph."),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };
        return true;
    }

    /// <summary>Evaluates the selected transition list in its early or late graph section using held and edge inputs.</summary>
    internal static SamusPoseInputMatch Match(SamusPoseInputList inputList, ControllerButton held, ControllerButton newlyPressed) =>
        LaterLists.Contains(inputList)
            ? SamusPoseInputRulesLate.Match(inputList, held, newlyPressed)
            : SamusPoseInputRulesEarly.Match(inputList, held, newlyPressed);

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
