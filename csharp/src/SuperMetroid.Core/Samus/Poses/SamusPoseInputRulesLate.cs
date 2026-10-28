using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Samus.Poses.CanonicalPoseButtons;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputs;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputList;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Ordered native pose-input decisions; the first satisfied chord wins.</summary>
internal static class SamusPoseInputRulesLate
{
    /// <summary>Dispatches the later bank-$91 pose tables in cartridge order, separating held chords from new presses.</summary>
    internal static SamusPoseInputMatch Match(SamusPoseInputList inputList, ControllerButton held, ControllerButton newlyPressed) => inputList switch
    {
        SpringBallGroundRightPoseList => MatchSpringBallGroundRightPoseList(held, newlyPressed),
        SpringBallGroundLeftPoseList => MatchSpringBallGroundLeftPoseList(held, newlyPressed),
        SpringBallFallingRightPoseList => MatchSpringBallFallingRightPoseList(held, newlyPressed),
        SpringBallFallingLeftPoseList => MatchSpringBallFallingRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        SpringBallJumpRightPoseList => MatchSpringBallJumpRightPoseList(held, newlyPressed),
        SpringBallJumpLeftPoseList => MatchSpringBallJumpLeftPoseList(held, newlyPressed),
        UnusedPose63List => MatchUnusedPose63List(held),
        UnusedPose64List => MatchUnusedPose64List(held),
        UnusedPose65List => MatchUnusedPose65List(held),
        UnusedPose66List => MatchUnusedPose66List(held),
        WallJumpRightPoseList => MatchWallJumpRightPoseList(held, newlyPressed),
        WallJumpLeftPoseList => MatchWallJumpRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        RanIntoWallRightPoseList => MatchRanIntoWallRightPoseList(held, newlyPressed),
        RanIntoWallLeftPoseList => MatchRanIntoWallRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        NormalJumpGunExtendedRightPoseList => MatchNormalJumpGunExtendedRightPoseList(held),
        NormalJumpGunExtendedLeftPoseList => MatchNormalJumpGunExtendedRightPoseList(MirrorButtons(held)).Mirrored(),
        NormalJumpAimDownRightPoseList => MatchNormalJumpAimDownRightPoseList(held, newlyPressed),
        NormalJumpAimDownLeftPoseList => MatchNormalJumpAimDownLeftPoseList(held, newlyPressed),
        UnmorphingTransitionRightPoseList => MatchUnmorphingTransitionRightPoseList(held),
        UnmorphingTransitionLeftPoseList => MatchUnmorphingTransitionRightPoseList(MirrorButtons(held)).Mirrored(),
        TurningRightToLeftPoseList => MatchTurningRightToLeftPoseList(held, newlyPressed),
        TurningLeftToRightPoseList => MatchTurningRightToLeftPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        TurningRightToLeftAimUpPoseList => MatchTurningRightToLeftAimUpPoseList(held, newlyPressed),
        TurningLeftToRightAimUpPoseList => MatchTurningRightToLeftAimUpPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        TurningRightToLeftAimDiagonalDownPoseList => MatchTurningRightToLeftAimDiagonalDownPoseList(held, newlyPressed),
        TurningLeftToRightAimDiagonalDownPoseList => MatchTurningRightToLeftAimDiagonalDownPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        ShinesparkWindupRightPoseList => MatchShinesparkWindupRightPoseList(held),
        ShinesparkWindupLeftPoseList => MatchShinesparkWindupRightPoseList(MirrorButtons(held)).Mirrored(),
        FallingAimDownRightPoseList => MatchFallingAimDownRightPoseList(held, newlyPressed),
        FallingAimDownLeftPoseList => MatchFallingAimDownRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        UnusedPoseDfList => MatchUnusedPoseDfList(newlyPressed),
        DraygonGrabbedNeutralLeftPoseList => MatchDraygonGrabbedNeutralLeftPoseList(held),
        DraygonGrabbedNeutralRightPoseList => MatchDraygonGrabbedNeutralRightPoseList(held),
        MovingRightGunExtendedPoseList => MatchMovingRightGunExtendedPoseList(held, newlyPressed),
        MovingLeftGunExtendedPoseList => MatchMovingRightGunExtendedPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        FallingGunExtendedRightPoseList => MatchFallingGunExtendedRightPoseList(held),
        FallingGunExtendedLeftPoseList => MatchFallingGunExtendedRightPoseList(MirrorButtons(held)).Mirrored(),
        MoonwalkTurnJumpLeftPoseList => MatchMoonwalkTurnJumpRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        MoonwalkTurnJumpRightPoseList => MatchMoonwalkTurnJumpRightPoseList(held, newlyPressed),
        MoonwalkTurnJumpAimUpLeftPoseList => MatchMoonwalkTurnJumpAimedRightPoseList(SamusPoseId.MoonwalkTurnJumpAimUpRightPose,
            MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        MoonwalkTurnJumpAimUpRightPoseList => MatchMoonwalkTurnJumpAimedRightPoseList(SamusPoseId.MoonwalkTurnJumpAimUpRightPose, held, newlyPressed),
        MoonwalkTurnJumpAimDownLeftPoseList => MatchMoonwalkTurnJumpAimedRightPoseList(SamusPoseId.MoonwalkTurnJumpAimDownRightPose,
            MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        MoonwalkTurnJumpAimDownRightPoseList => MatchMoonwalkTurnJumpAimedRightPoseList(SamusPoseId.MoonwalkTurnJumpAimDownRightPose, held, newlyPressed),
        SamusPoseInputList.EmptyTransitionList or
            SamusPoseInputList.ForwardFacingPowerSuitPoseList or
            SamusPoseInputList.FacingRightNormalPoseList or
            SamusPoseInputList.FacingLeftNormalPoseList or
            SamusPoseInputList.MovingRightNormalPoseList or
            SamusPoseInputList.MovingLeftNormalPoseList or
            SamusPoseInputList.NeutralJumpTransitionRightPoseList or
            SamusPoseInputList.NeutralJumpTransitionLeftPoseList or
            SamusPoseInputList.NormalJumpAimUpRightPoseList or
            SamusPoseInputList.NormalJumpAimUpLeftPoseList or
            SamusPoseInputList.DamageBoostLeftPoseList or
            SamusPoseInputList.DamageBoostRightPoseList or
            SamusPoseInputList.SpinJumpRightPoseList or
            SamusPoseInputList.SpinJumpLeftPoseList or
            SamusPoseInputList.SpaceJumpRightPoseList or
            SamusPoseInputList.SpaceJumpLeftPoseList or
            SamusPoseInputList.ScrewAttackRightPoseList or
            SamusPoseInputList.ScrewAttackLeftPoseList or
            SamusPoseInputList.MorphBallGroundRightPoseList or
            SamusPoseInputList.MorphBallMovingRightPoseList or
            SamusPoseInputList.MorphBallMovingLeftPoseList or
            SamusPoseInputList.MorphBallGroundLeftPoseList or
            SamusPoseInputList.UnusedPose20List or
            SamusPoseInputList.UnusedPose23List or
            SamusPoseInputList.UnusedPose42List or
            SamusPoseInputList.CrouchingRightPoseList or
            SamusPoseInputList.CrouchingLeftPoseList or
            SamusPoseInputList.FallingRightPoseList or
            SamusPoseInputList.FallingLeftPoseList or
            SamusPoseInputList.MorphBallFallingRightPoseList or
            SamusPoseInputList.MorphBallFallingLeftPoseList or
            SamusPoseInputList.UnusedKnockbackRightPoseList or
            SamusPoseInputList.UnusedKnockbackLeftPoseList or
            SamusPoseInputList.UnusedPose45List or
            SamusPoseInputList.UnusedPose46List or
            SamusPoseInputList.UnusedPose47List or
            SamusPoseInputList.UnusedPose48List or
            SamusPoseInputList.MoonwalkFacingLeftPoseList or
            SamusPoseInputList.MoonwalkFacingRightPoseList or
            SamusPoseInputList.KnockbackRightPoseList or
            SamusPoseInputList.KnockbackLeftPoseList or
            SamusPoseInputList.UnusedPose5BList or
            SamusPoseInputList.UnusedPose5CList => throw new InvalidOperationException($"Pose-input list {inputList} belongs to the other rule section."),
        _ => throw new InvalidOperationException($"Pose-input list {inputList} belongs to the other rule section."),
    };

    /// <summary>$91:A90C TransitionTable_79_7B_FacingRight_MorphBall_Spring_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallGroundRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.SpringBallJumpRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallMovingRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallMovingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A926 TransitionTable_7A_7C_FacingLeft_MorphBall_Spring_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallGroundLeftPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.SpringBallJumpLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallMovingRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallMovingLeftPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:A940 TransitionTable_7D_FacingRight_MorphBall_SpringBall_Falling: native priority order; $91:A954 TransitionTable_7E_FacingLeft_MorphBall_SpringBall_Falling is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchSpringBallFallingRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallFallingLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallFallingRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:A968 TransitionTable_7F_FacingRight_MorphBall_SpringBall_InAir: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallJumpRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallJumpRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallJumpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A97C TransitionTable_80_FacingLeft_MorphBall_SpringBall_InAir: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallJumpLeftPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallJumpRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallJumpLeftPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:A990 UNUSED_TransitionTable_63_91A990: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose63List(ControllerButton held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.UnusedPose66);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A998 UNUSED_TransitionTable_64_91A998: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose64List(ControllerButton held)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.UnusedPose65);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A9A0 UNUSED_TransitionTable_65_91A9A0: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose65List(ControllerButton held)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.UnusedPose65);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.UnusedPose65);
        if (Has(held, Right))
            return Accept(SamusPoseId.UnusedPose65);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A9C6 UNUSED_TransitionTable_66_91A9C6: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose66List(ControllerButton held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.UnusedPose66);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.UnusedPose66);
        if (Has(held, Left))
            return Accept(SamusPoseId.UnusedPose66);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A9EC TransitionTable_83_FacingRight_WallJump: native priority order; $91:AA12 TransitionTable_84_FacingLeft_WallJump is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchWallJumpRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.WallJumpRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:AA38 TransitionTable_89_CF_D1_FacingRight_RanIntoAWall: native priority order; $91:AA7C TransitionTable_8A_D0_D2_FacingLeft_RanIntoAWall is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchRanIntoWallRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose);
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionRightPose);
        if (Has(held, AimDown | Left))
            return Accept(SamusPoseId.MoonwalkAimDownRightPose);
        if (Has(held, AimUp | Left))
            return Accept(SamusPoseId.MoonwalkAimUpRightPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightNormalPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:AAC0 TransitionTable_13_FaceRight_NormalJump_NotMoving_GunExtend: native priority order; $91:AB3A TransitionTable_14_FacingLeft_NormalJump_NotMoving_GunExtend is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchNormalJumpGunExtendedRightPoseList(ControllerButton held)
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
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:ABB4 TransitionTable_17_FacingRight_NormalJump_AimingDown: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpAimDownRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionRightPose);
        if (Has(held, Jump | Right | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, Jump | Right | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, AimUp | Jump | Right))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Jump | Right))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Shoot | Jump | Right))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
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
            return Accept(SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AC40 TransitionTable_18_FacingLeft_NormalJump_AimingDown: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpAimDownLeftPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionLeftPose);
        if (Has(held, Jump | Left | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, Jump | Left | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, AimUp | Jump | Left))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Jump | Left))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, AimDown | Jump | Left))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Left | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
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
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.NormalJumpForwardLeftPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:ACCC TransitionTable_3D_FacingRight_Unmorphing: native priority order; $91:ACE0 TransitionTable_3E_FacingLeft_Unmorphing is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchUnmorphingTransitionRightPoseList(ControllerButton held)
    {
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.FallingGunExtendedRightPose);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.FallingAimUpRightPose);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.FallingAimDownRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:ACF4 TransitionTable_25_FacingRight_Turning_Standing: native priority order; $91:AD08 TransitionTable_26_FacingLeft_Turning_Standing is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchTurningRightToLeftPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:AD1C TransitionTable_8B_FacingRight_Turning_Standing_AimingUp: native priority order; $91:AD30 TransitionTable_8C_FacingLeft_Turning_Standing_AimingUp is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchTurningRightToLeftAimUpPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftAimUpPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:AD44 TransitionTable_8D_FacingRight_Turning_Standing_AimDownRight: native priority order; $91:AD58 TransitionTable_8E_FacingLeft_Turning_Standing_AimDownLeft is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchTurningRightToLeftAimDiagonalDownPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftAimDiagonalDownPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:AD6C TransitionTable_C7_FacingRight_VerticalShinesparkWindup: native priority order; $91:AD80 TransitionTable_C8_FacingLeft_VerticalShinesparkWindup is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchShinesparkWindupRightPoseList(ControllerButton held)
    {
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.ShinesparkVerticalRightPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.ShinesparkDiagonalRightPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.ShinesparkHorizontalRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:AD94 TransitionTable_2D_FacingRight_Falling_AimingDown: native priority order; $91:ADD2 TransitionTable_2E_FacingLeft_Falling_AimingDown is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchFallingAimDownRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionRightPose);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.FallingAimUpRightPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.FallingAimDownRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftFallingPose);
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


    /// <summary>$91:AE10 UNUSED_TransitionTable_DF_91AE10: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPoseDfList(ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnusedPoseDe);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:AE56 TransitionTable_EC_ED_EE_EF_F0_FacingRight_GrabbedByDraygon: native priority order.</summary>
    private static SamusPoseInputMatch MatchDraygonGrabbedNeutralRightPoseList(ControllerButton held)
    {
        if (Has(held, Shoot | Right | Up))
            return Accept(SamusPoseId.DraygonGrabbedAimUpRightPose);
        if (Has(held, Shoot | Right | Down))
            return Accept(SamusPoseId.DraygonGrabbedAimDownRightPose);
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.DraygonGrabbedFiringRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.DraygonGrabbedAimUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.DraygonGrabbedAimDownRightPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.DraygonGrabbedFiringRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.DraygonGrabbedMovingRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.DraygonGrabbedMovingRightPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.DraygonGrabbedMovingRightPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.DraygonGrabbedMovingRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AE18 TransitionTable_BA_BB_BC_BD_BE_FacingLeft_GrabbedByDraygon: native priority order.</summary>
    private static SamusPoseInputMatch MatchDraygonGrabbedNeutralLeftPoseList(ControllerButton held)
    {
        if (Has(held, Shoot | Left | Up))
            return Accept(SamusPoseId.DraygonGrabbedAimUpLeftPose);
        if (Has(held, Shoot | Left | Down))
            return Accept(SamusPoseId.DraygonGrabbedAimDownLeftPose);
        if (Has(held, Shoot | Left))
            return Accept(SamusPoseId.DraygonGrabbedFiringLeftPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.DraygonGrabbedAimUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.DraygonGrabbedAimDownLeftPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.DraygonGrabbedFiringLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.DraygonGrabbedMovingLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.DraygonGrabbedMovingLeftPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.DraygonGrabbedMovingLeftPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.DraygonGrabbedMovingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AE94 TransitionTable_0B_MovingRight_GunExtended: native priority order; $91:AEDE TransitionTable_0C_MovingLeft_GunExtended is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchMovingRightGunExtendedPoseList(ControllerButton held, ControllerButton newlyPressed)
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
            return Accept(SamusPoseId.MovingRightGunExtendedPose);
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


    /// <summary>$91:AF28 TransitionTable_67_FacingRight_Falling_GunExtended: native priority order; $91:AF60 TransitionTable_68_FacingLeft_Falling_GunExtended is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchFallingGunExtendedRightPoseList(ControllerButton held)
    {
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.FallingAimUpRightPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.FallingAimDownRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftFallingPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.FallingGunExtendedRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.FallingGunExtendedRightPose);
        return new(null, HasConditions: true);
    }



    /// <summary>$91:AFAC TransitionTable_C0_FacingLeft_Moonwalking_TurnJumpRight: native priority order; $91:AF98 TransitionTable_BF_FacingRight_Moonwalking_TurnJumpLeft is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpRightPoseList(ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MoonwalkTurnJumpRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>
    /// $91:AFD4 TransitionTable_C2_FaceLeft_Moonwalk_TurnJumpRight_AimUpLeft; $91:AFFC TransitionTable_C4_FaceLeft_Moonwalk_TurnJumpRight_AimDownLeft; $91:AFC0 TransitionTable_C1_FaceRight_Moonwalk_TurnJumpLeft_AimUpRight; $91:AFE8 TransitionTable_C3_FaceRight_Moonwalk_TurnJumpLeft_AimDownRight: one native priority order for an aimed moonwalk turn jump to the right, continuing <paramref name="aimedTurnJump"/>;
    /// the leftward lists are it mirrored.
    /// </summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpAimedRightPoseList(SamusPoseId aimedTurnJump, ControllerButton held, ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(aimedTurnJump);
        return new(null, HasConditions: true);
    }


}
