using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Samus.Poses.CanonicalPoseButtons;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputs;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputList;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Ordered pose-input decisions; the first satisfied chord wins.</summary>
internal static class SamusPoseInputRulesEarly
{
    /// <summary>Selects the first input chord in an early transition list and rejects lists owned by the other rule section.</summary>
    internal static SamusPoseInputMatch Match(SamusPoseInputList inputList, ControllerButton held, ControllerButton newlyPressed) => inputList switch
    {
        EmptyTransitionList => MatchEmptyTransitionList(),
        ForwardFacingPowerSuitPoseList => MatchForwardFacingPowerSuitPoseList(held),
        FacingRightNormalPoseList => MatchFacingRightNormalPoseList(held, newlyPressed),
        FacingLeftNormalPoseList => MatchFacingRightNormalPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        MovingRightNormalPoseList => MatchMovingRightNormalPoseList(held, newlyPressed),
        MovingLeftNormalPoseList => MatchMovingRightNormalPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        NeutralJumpTransitionRightPoseList => MatchNeutralJumpTransitionRightPoseList(held),
        NeutralJumpTransitionLeftPoseList => MatchNeutralJumpTransitionLeftPoseList(held),
        NormalJumpAimUpRightPoseList => MatchNormalJumpAimUpRightPoseList(held),
        NormalJumpAimUpLeftPoseList => MatchNormalJumpAimUpRightPoseList(MirrorButtons(held)).Mirrored(),
        DamageBoostLeftPoseList => MatchDamageBoostLeftPoseList(held),
        DamageBoostRightPoseList => MatchDamageBoostRightPoseList(held),
        SpinJumpRightPoseList => MatchSpinJumpRightPoseList(held, newlyPressed),
        SpinJumpLeftPoseList => MatchSpinJumpRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        SpaceJumpRightPoseList => MatchSpaceJumpRightPoseList(held, newlyPressed),
        SpaceJumpLeftPoseList => MatchSpaceJumpRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        ScrewAttackRightPoseList => MatchScrewAttackRightPoseList(held, newlyPressed),
        ScrewAttackLeftPoseList => MatchScrewAttackRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        MorphBallGroundRightPoseList => MatchMorphBallGroundRightPoseList(held, newlyPressed),
        MorphBallMovingRightPoseList => MatchMorphBallMovingRightPoseList(held, newlyPressed),
        MorphBallMovingLeftPoseList => MatchMorphBallMovingLeftPoseList(held, newlyPressed),
        MorphBallGroundLeftPoseList => MatchMorphBallGroundLeftPoseList(held, newlyPressed),
        GlitchBallRightPoseList => MatchGlitchBallRightPoseList(),
        GlitchBallLeftPoseList => MatchGlitchBallRightPoseList().Mirrored(),
        CrouchingRightPoseList => MatchCrouchingRightPoseList(held, newlyPressed),
        CrouchingLeftPoseList => MatchCrouchingLeftPoseList(held, newlyPressed),
        FallingRightPoseList => MatchFallingRightPoseList(held),
        FallingLeftPoseList => MatchFallingRightPoseList(MirrorButtons(held)).Mirrored(),
        MorphBallFallingRightPoseList => MatchMorphBallFallingRightPoseList(held, newlyPressed),
        MorphBallFallingLeftPoseList => MatchMorphBallFallingRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        GlitchBallKnockbackRightPoseList => MatchGlitchBallKnockbackRightPoseList(),
        GlitchBallKnockbackLeftPoseList => MatchGlitchBallKnockbackRightPoseList().Mirrored(),
        MoonwalkFacingLeftPoseList => MatchMoonwalkFacingLeftPoseList(held, newlyPressed),
        MoonwalkFacingRightPoseList => MatchMoonwalkFacingRightPoseList(held, newlyPressed),
        KnockbackRightPoseList => MatchKnockbackRightPoseList(held),
        KnockbackLeftPoseList => MatchKnockbackRightPoseList(MirrorButtons(held)).Mirrored(),
        SamusPoseInputList.SpringBallGroundRightPoseList or
            SamusPoseInputList.SpringBallGroundLeftPoseList or
            SamusPoseInputList.SpringBallFallingRightPoseList or
            SamusPoseInputList.SpringBallFallingLeftPoseList or
            SamusPoseInputList.SpringBallJumpRightPoseList or
            SamusPoseInputList.SpringBallJumpLeftPoseList or
            SamusPoseInputList.CompactAirborneRightPoseList or
            SamusPoseInputList.CompactAirborneLeftPoseList or
            SamusPoseInputList.CompactAirborneFallingRightPoseList or
            SamusPoseInputList.CompactAirborneFallingLeftPoseList or
            SamusPoseInputList.WallJumpRightPoseList or
            SamusPoseInputList.WallJumpLeftPoseList or
            SamusPoseInputList.RanIntoWallRightPoseList or
            SamusPoseInputList.RanIntoWallLeftPoseList or
            SamusPoseInputList.NormalJumpGunExtendedRightPoseList or
            SamusPoseInputList.NormalJumpGunExtendedLeftPoseList or
            SamusPoseInputList.NormalJumpAimDownRightPoseList or
            SamusPoseInputList.NormalJumpAimDownLeftPoseList or
            SamusPoseInputList.UnmorphingTransitionRightPoseList or
            SamusPoseInputList.UnmorphingTransitionLeftPoseList or
            SamusPoseInputList.TurningRightToLeftPoseList or
            SamusPoseInputList.TurningLeftToRightPoseList or
            SamusPoseInputList.TurningRightToLeftAimUpPoseList or
            SamusPoseInputList.TurningLeftToRightAimUpPoseList or
            SamusPoseInputList.TurningRightToLeftAimDiagonalDownPoseList or
            SamusPoseInputList.TurningLeftToRightAimDiagonalDownPoseList or
            SamusPoseInputList.ShinesparkWindupRightPoseList or
            SamusPoseInputList.ShinesparkWindupLeftPoseList or
            SamusPoseInputList.FallingAimDownRightPoseList or
            SamusPoseInputList.FallingAimDownLeftPoseList or
            SamusPoseInputList.DraygonHeldBallLeftPoseList or
            SamusPoseInputList.DraygonGrabbedNeutralLeftPoseList or
            SamusPoseInputList.DraygonGrabbedNeutralRightPoseList or
            SamusPoseInputList.MovingRightGunExtendedPoseList or
            SamusPoseInputList.MovingLeftGunExtendedPoseList or
            SamusPoseInputList.FallingGunExtendedRightPoseList or
            SamusPoseInputList.FallingGunExtendedLeftPoseList or
            SamusPoseInputList.MoonwalkTurnJumpLeftPoseList or
            SamusPoseInputList.MoonwalkTurnJumpRightPoseList or
            SamusPoseInputList.MoonwalkTurnJumpAimUpLeftPoseList or
            SamusPoseInputList.MoonwalkTurnJumpAimUpRightPoseList or
            SamusPoseInputList.MoonwalkTurnJumpAimDownLeftPoseList or
            SamusPoseInputList.MoonwalkTurnJumpAimDownRightPoseList => throw new InvalidOperationException($"Pose-input list {inputList} belongs to the other rule section."),
        _ => throw new InvalidOperationException($"Pose-input list {inputList} belongs to the other rule section."),
    };

    /// <summary>The input-transition list of <see cref="SamusPoseId.TurningRightToLeftJumpPose"/>: priority order.</summary>
    private static SamusPoseInputMatch MatchEmptyTransitionList()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>The priority order of the forward facing power suit input list.</summary>
    private static SamusPoseInputMatch MatchForwardFacingPowerSuitPoseList(ControllerButton held)
    {
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the facing right normal input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchFacingRightNormalPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Up))
            return Accept(SamusPoseId.NormalJumpTransitionAimUpRightPose);
        if (Has(newlyPressed, Jump) && Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose);
        if (Has(newlyPressed, Jump) && Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(newlyPressed, Down) && Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.CrouchingTransitionAimUpRightPose);
        if (Has(newlyPressed, Down) && Has(held, AimUp))
            return Accept(SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose);
        if (Has(newlyPressed, Down) && Has(held, AimDown))
            return Accept(SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose);
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionRightPose);
        if (Has(held, AimDown | Shoot | Left))
            return Accept(SamusPoseId.MoonwalkAimDownRightPose);
        if (Has(held, AimUp | Shoot | Left))
            return Accept(SamusPoseId.MoonwalkAimUpRightPose);
        if (Has(held, AimUp | AimDown | Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        if (Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.StandingAimUpRightPose);
        if (Has(held, AimUp | Right))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose);
        if (Has(held, AimDown | Right))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose);
        if (Has(held, Shoot | Left))
            return Accept(SamusPoseId.MoonwalkFacingRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightNormalPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The priority order of the moving right normal input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchMovingRightNormalPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(held, AimUp | Right))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose);
        if (Has(held, AimDown | Right))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose);
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.MovingRightGunExtendedPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightNormalPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The priority order of the neutral jump transition right input list.</summary>
    private static SamusPoseInputMatch MatchNeutralJumpTransitionRightPoseList(ControllerButton held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.TurningRightToLeftJumpPose);
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Jump | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Shoot | Jump))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the neutral jump transition left input list.</summary>
    private static SamusPoseInputMatch MatchNeutralJumpTransitionLeftPoseList(ControllerButton held)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.TurningLeftToRightJumpPose);
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Jump | Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.NormalJumpForwardLeftPose);
        if (Has(held, Shoot | Jump))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightJumpPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the normal jump aim up right input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchNormalJumpAimUpRightPoseList(ControllerButton held)
    {
        if (Has(held, Jump | Right | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, Jump | Right | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, AimUp | Jump | Right))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Jump | Right))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.TurningRightToLeftJumpPose);
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Jump | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Shoot | Jump))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftJumpPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NeutralJumpRightPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The priority order of the damage boost left input list.</summary>
    private static SamusPoseInputMatch MatchDamageBoostLeftPoseList(ControllerButton held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.NormalJumpForwardLeftPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.DamageBoostLeftPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NeutralJumpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the damage boost right input list.</summary>
    private static SamusPoseInputMatch MatchDamageBoostRightPoseList(ControllerButton held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.DamageBoostRightPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NeutralJumpRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the spin jump right input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchSpinJumpRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(newlyPressed, Shoot) && Has(held, Right))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The priority order of the space jump right input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchSpaceJumpRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(newlyPressed, Shoot) && Has(held, Right))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.SpaceJumpRightPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpaceJumpRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpaceJumpLeftPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The priority order of the screw attack right input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchScrewAttackRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(newlyPressed, Shoot) && Has(held, Right))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.ScrewAttackRightPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.ScrewAttackRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.ScrewAttackLeftPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The priority order of the morph ball ground right input list.</summary>
    private static SamusPoseInputMatch MatchMorphBallGroundRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MorphBallMovingRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MorphBallMovingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the morph ball ground left input list.</summary>
    private static SamusPoseInputMatch MatchMorphBallGroundLeftPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MorphBallMovingRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MorphBallMovingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the morph ball moving right input list.</summary>
    private static SamusPoseInputMatch MatchMorphBallMovingRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MorphBallMovingRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MorphBallMovingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the morph ball moving left input list.</summary>
    private static SamusPoseInputMatch MatchMorphBallMovingLeftPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MorphBallMovingRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MorphBallMovingLeftPose);
        return new(null, HasConditions: true);
    }



    /// <summary>The input-transition list of <see cref="SamusPoseId.GlitchBallRightPose"/>: priority order.</summary>
    private static SamusPoseInputMatch MatchGlitchBallRightPoseList()
    {
        return new(null, HasConditions: false);
    }


    /// <summary>The priority order of the crouching right input list.</summary>
    private static SamusPoseInputMatch MatchCrouchingRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up) && Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.StandingTransitionAimUpRightPose);
        if (Has(newlyPressed, Up) && Has(held, AimUp))
            return Accept(SamusPoseId.StandingTransitionAimDiagonalUpRightPose);
        if (Has(newlyPressed, Up) && Has(held, AimDown))
            return Accept(SamusPoseId.StandingTransitionAimDiagonalDownRightPose);
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.StandingTransitionRightPose);
        if (Has(newlyPressed, Left))
            return Accept(SamusPoseId.TurningRightToLeftCrouchingPose);
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.CrouchingAimUpRightPose);
        if (Has(held, AimUp | Right))
            return Accept(SamusPoseId.FacingRightNormalPose);
        if (Has(held, AimDown | Right))
            return Accept(SamusPoseId.FacingRightNormalPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.CrouchingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.CrouchingAimDiagonalDownRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.FacingRightNormalPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the crouching left input list.</summary>
    [MirroredAsymmetry("the left-facing crouch list exchanges the diagonal aim-up and aim-down chords as well as the directions")]
    private static SamusPoseInputMatch MatchCrouchingLeftPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up) && Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.StandingTransitionAimUpLeftPose);
        if (Has(newlyPressed, Up) && Has(held, AimUp))
            return Accept(SamusPoseId.StandingTransitionAimDiagonalUpLeftPose);
        if (Has(newlyPressed, Up) && Has(held, AimDown))
            return Accept(SamusPoseId.StandingTransitionAimDiagonalDownLeftPose);
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.StandingTransitionLeftPose);
        if (Has(newlyPressed, Right))
            return Accept(SamusPoseId.TurningLeftToRightCrouchingPose);
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.CrouchingAimUpLeftPose);
        if (Has(held, AimDown | Left))
            return Accept(SamusPoseId.FacingLeftNormalPose);
        if (Has(held, AimUp | Left))
            return Accept(SamusPoseId.FacingLeftNormalPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.CrouchingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.CrouchingAimDiagonalDownLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.FacingLeftNormalPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the falling right input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchFallingRightPoseList(ControllerButton held)
    {
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Left | Up))
            return Accept(SamusPoseId.TurningRightToLeftFallingPose);
        if (Has(held, Left | Down))
            return Accept(SamusPoseId.TurningRightToLeftFallingPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftFallingPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.FallingAimUpRightPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.FallingAimDownRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.FallingGunExtendedRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.FallingRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The priority order of the morph ball falling right input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchMorphBallFallingRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MorphBallFallingRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MorphBallFallingLeftPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The input-transition list of <see cref="SamusPoseId.GlitchBallKnockbackRightPose"/>: priority order; the input-transition list of <see cref="SamusPoseId.GlitchBallKnockbackLeftPose"/> is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchGlitchBallKnockbackRightPoseList()
    {
        return new(null, HasConditions: false);
    }


    /// <summary>The priority order of the moonwalk facing left input list.</summary>
    private static SamusPoseInputMatch MatchMoonwalkFacingLeftPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.MoonwalkTurnJumpRightPose);
        if (Has(newlyPressed, Jump) && Has(held, AimUp))
            return Accept(SamusPoseId.MoonwalkTurnJumpAimUpRightPose);
        if (Has(newlyPressed, Jump) && Has(held, AimDown))
            return Accept(SamusPoseId.MoonwalkTurnJumpAimDownRightPose);
        if (Has(held, AimDown | Shoot | Right))
            return Accept(SamusPoseId.MoonwalkAimDownLeftPose);
        if (Has(held, AimUp | Shoot | Right))
            return Accept(SamusPoseId.MoonwalkAimUpLeftPose);
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.MoonwalkFacingLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MovingLeftNormalPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the moonwalk facing right input list.</summary>
    [MirroredAsymmetry("the right-facing moonwalk list exchanges the diagonal aim-up and aim-down chords and aimed poses as well as the directions")]
    private static SamusPoseInputMatch MatchMoonwalkFacingRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.MoonwalkTurnJumpLeftPose);
        if (Has(newlyPressed, Jump) && Has(held, AimUp))
            return Accept(SamusPoseId.MoonwalkTurnJumpAimUpLeftPose);
        if (Has(newlyPressed, Jump) && Has(held, AimDown))
            return Accept(SamusPoseId.MoonwalkTurnJumpAimDownLeftPose);
        if (Has(held, AimUp | Shoot | Left))
            return Accept(SamusPoseId.MoonwalkAimUpRightPose);
        if (Has(held, AimDown | Shoot | Left))
            return Accept(SamusPoseId.MoonwalkAimDownRightPose);
        if (Has(held, Shoot | Left))
            return Accept(SamusPoseId.MoonwalkFacingRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightNormalPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the knockback right input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchKnockbackRightPoseList(ControllerButton held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.DamageBoostRightPose);
        return new(null, HasConditions: true);
    }


}
