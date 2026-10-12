using static SuperMetroid.Core.Game.CanonicalPoseButtons;
using static SuperMetroid.Core.Game.SamusPoseInputDefinitions;
using static SuperMetroid.Core.Game.SamusPoseInputList;

namespace SuperMetroid.Core.Game;

/// <summary>Ordered native pose-input decisions; the first satisfied chord wins.</summary>
internal static class SamusPoseInputRulesLate
{
    internal static SamusPoseInputMatch Match(SamusPoseInputList pointer, ushort held, ushort newlyPressed) => pointer switch
    {
        SpringBallGroundRightPoseList => MatchSpringBallGroundRightPoseList(held, newlyPressed),
        SpringBallGroundLeftPoseList => MatchSpringBallGroundLeftPoseList(held, newlyPressed),
        SpringBallFallingRightPoseList => MatchSpringBallFallingRightPoseList(held, newlyPressed),
        SpringBallFallingLeftPoseList => MatchSpringBallFallingLeftPoseList(held, newlyPressed),
        SpringBallJumpRightPoseList => MatchSpringBallJumpRightPoseList(held, newlyPressed),
        SpringBallJumpLeftPoseList => MatchSpringBallJumpLeftPoseList(held, newlyPressed),
        UnusedPose63List => MatchUnusedPose63List(held),
        UnusedPose64List => MatchUnusedPose64List(held),
        UnusedPose65List => MatchUnusedPose65List(held),
        UnusedPose66List => MatchUnusedPose66List(held),
        WallJumpRightPoseList => MatchWallJumpRightPoseList(held, newlyPressed),
        WallJumpLeftPoseList => MatchWallJumpLeftPoseList(held, newlyPressed),
        RanIntoWallRightPoseList => MatchRanIntoWallRightPoseList(held, newlyPressed),
        RanIntoWallLeftPoseList => MatchRanIntoWallLeftPoseList(held, newlyPressed),
        NormalJumpGunExtendedRightPoseList => MatchNormalJumpGunExtendedRightPoseList(held),
        NormalJumpGunExtendedLeftPoseList => MatchNormalJumpGunExtendedLeftPoseList(held),
        NormalJumpAimDownRightPoseList => MatchNormalJumpAimDownRightPoseList(held, newlyPressed),
        NormalJumpAimDownLeftPoseList => MatchNormalJumpAimDownLeftPoseList(held, newlyPressed),
        UnmorphingTransitionRightPoseList => MatchUnmorphingTransitionRightPoseList(held),
        UnmorphingTransitionLeftPoseList => MatchUnmorphingTransitionLeftPoseList(held),
        TurningRightToLeftPoseList => MatchTurningRightToLeftPoseList(held, newlyPressed),
        TurningLeftToRightPoseList => MatchTurningLeftToRightPoseList(held, newlyPressed),
        TurningRightToLeftAimUpPoseList => MatchTurningRightToLeftAimUpPoseList(held, newlyPressed),
        TurningLeftToRightAimUpPoseList => MatchTurningLeftToRightAimUpPoseList(held, newlyPressed),
        TurningRightToLeftAimDiagonalDownPoseList => MatchTurningRightToLeftAimDiagonalDownPoseList(held, newlyPressed),
        TurningLeftToRightAimDiagonalDownPoseList => MatchTurningLeftToRightAimDiagonalDownPoseList(held, newlyPressed),
        ShinesparkWindupRightPoseList => MatchShinesparkWindupRightPoseList(held),
        ShinesparkWindupLeftPoseList => MatchShinesparkWindupLeftPoseList(held),
        FallingAimDownRightPoseList => MatchFallingAimDownRightPoseList(held, newlyPressed),
        FallingAimDownLeftPoseList => MatchFallingAimDownLeftPoseList(held, newlyPressed),
        UnusedPoseDfList => MatchUnusedPoseDfList(newlyPressed),
        DraygonGrabbedNeutralLeftPoseList => MatchDraygonGrabbedNeutralLeftPoseList(held),
        DraygonGrabbedNeutralRightPoseList => MatchDraygonGrabbedNeutralRightPoseList(held),
        MovingRightGunExtendedPoseList => MatchMovingRightGunExtendedPoseList(held, newlyPressed),
        MovingLeftGunExtendedPoseList => MatchMovingLeftGunExtendedPoseList(held, newlyPressed),
        FallingGunExtendedRightPoseList => MatchFallingGunExtendedRightPoseList(held),
        FallingGunExtendedLeftPoseList => MatchFallingGunExtendedLeftPoseList(held),
        MoonwalkTurnJumpLeftPoseList => MatchMoonwalkTurnJumpLeftPoseList(held, newlyPressed),
        MoonwalkTurnJumpRightPoseList => MatchMoonwalkTurnJumpRightPoseList(held, newlyPressed),
        MoonwalkTurnJumpAimUpLeftPoseList => MatchMoonwalkTurnJumpAimUpLeftPoseList(held, newlyPressed),
        MoonwalkTurnJumpAimUpRightPoseList => MatchMoonwalkTurnJumpAimUpRightPoseList(held, newlyPressed),
        MoonwalkTurnJumpAimDownLeftPoseList => MatchMoonwalkTurnJumpAimDownLeftPoseList(held, newlyPressed),
        MoonwalkTurnJumpAimDownRightPoseList => MatchMoonwalkTurnJumpAimDownRightPoseList(held, newlyPressed),
        _ => throw new InvalidOperationException($"Pose-input list {pointer} belongs to the other rule section."),
    };

    /// <summary>$91:A90C TransitionTable_79_7B_FacingRight_MorphBall_Spring_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallGroundRightPoseList(ushort held, ushort newlyPressed)
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
    private static SamusPoseInputMatch MatchSpringBallGroundLeftPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A940 TransitionTable_7D_FacingRight_MorphBall_SpringBall_Falling: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallFallingRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallFallingLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallFallingRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A954 TransitionTable_7E_FacingLeft_MorphBall_SpringBall_Falling: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallFallingLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallFallingRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallFallingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A968 TransitionTable_7F_FacingRight_MorphBall_SpringBall_InAir: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallJumpRightPoseList(ushort held, ushort newlyPressed)
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
    private static SamusPoseInputMatch MatchSpringBallJumpLeftPoseList(ushort held, ushort newlyPressed)
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
    private static SamusPoseInputMatch MatchUnusedPose63List(ushort held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.UnusedPose66);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A998 UNUSED_TransitionTable_64_91A998: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose64List(ushort held)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.UnusedPose65);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A9A0 UNUSED_TransitionTable_65_91A9A0: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose65List(ushort held)
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
    private static SamusPoseInputMatch MatchUnusedPose66List(ushort held)
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

    /// <summary>$91:A9EC TransitionTable_83_FacingRight_WallJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchWallJumpRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:AA12 TransitionTable_84_FacingLeft_WallJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchWallJumpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.WallJumpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AA38 TransitionTable_89_CF_D1_FacingRight_RanIntoAWall: native priority order.</summary>
    private static SamusPoseInputMatch MatchRanIntoWallRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:AA7C TransitionTable_8A_D0_D2_FacingLeft_RanIntoAWall: native priority order.</summary>
    private static SamusPoseInputMatch MatchRanIntoWallLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left | Up))
            return Accept(SamusPoseId.RunningAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(SamusPoseId.RunningAimDiagonalDownLeftPose);
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionLeftPose);
        if (Has(held, AimDown | Right))
            return Accept(SamusPoseId.MoonwalkAimDownLeftPose);
        if (Has(held, AimUp | Right))
            return Accept(SamusPoseId.MoonwalkAimUpLeftPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MovingLeftNormalPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AAC0 TransitionTable_13_FaceRight_NormalJump_NotMoving_GunExtend: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpGunExtendedRightPoseList(ushort held)
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

    /// <summary>$91:AB3A TransitionTable_14_FacingLeft_NormalJump_NotMoving_GunExtend: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpGunExtendedLeftPoseList(ushort held)
    {
        if (Has(held, Jump | Left | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, Jump | Left | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, AimUp | Jump | Left))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
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
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:ABB4 TransitionTable_17_FacingRight_NormalJump_AimingDown: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpAimDownRightPoseList(ushort held, ushort newlyPressed)
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
    private static SamusPoseInputMatch MatchNormalJumpAimDownLeftPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:ACCC TransitionTable_3D_FacingRight_Unmorphing: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnmorphingTransitionRightPoseList(ushort held)
    {
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.FallingGunExtendedRightPose);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.FallingAimUpRightPose);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.FallingAimDownRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:ACE0 TransitionTable_3E_FacingLeft_Unmorphing: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnmorphingTransitionLeftPoseList(ushort held)
    {
        if (Has(held, Shoot | Left))
            return Accept(SamusPoseId.FallingGunExtendedLeftPose);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.FallingAimUpLeftPose);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.FallingAimDownLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:ACF4 TransitionTable_25_FacingRight_Turning_Standing: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningRightToLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AD08 TransitionTable_26_FacingLeft_Turning_Standing: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningLeftToRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AD1C TransitionTable_8B_FacingRight_Turning_Standing_AimingUp: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningRightToLeftAimUpPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftAimUpPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AD30 TransitionTable_8C_FacingLeft_Turning_Standing_AimingUp: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningLeftToRightAimUpPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightAimUpPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AD44 TransitionTable_8D_FacingRight_Turning_Standing_AimDownRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningRightToLeftAimDiagonalDownPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftAimDiagonalDownPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AD58 TransitionTable_8E_FacingLeft_Turning_Standing_AimDownLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningLeftToRightAimDiagonalDownPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightAimDiagonalDownPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AD6C TransitionTable_C7_FacingRight_VerticalShinesparkWindup: native priority order.</summary>
    private static SamusPoseInputMatch MatchShinesparkWindupRightPoseList(ushort held)
    {
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.ShinesparkVerticalRightPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.ShinesparkDiagonalRightPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.ShinesparkHorizontalRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AD80 TransitionTable_C8_FacingLeft_VerticalShinesparkWindup: native priority order.</summary>
    private static SamusPoseInputMatch MatchShinesparkWindupLeftPoseList(ushort held)
    {
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.ShinesparkVerticalLeftPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.ShinesparkDiagonalLeftPose);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.ShinesparkHorizontalLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AD94 TransitionTable_2D_FacingRight_Falling_AimingDown: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingAimDownRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:ADD2 TransitionTable_2E_FacingLeft_Falling_AimingDown: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingAimDownLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionLeftPose);
        if (Has(held, Left | Up))
            return Accept(SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.FallingAimUpLeftPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.FallingAimDownLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightFallingPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.FallingGunExtendedLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.FallingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AE10 UNUSED_TransitionTable_DF_91AE10: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPoseDfList(ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnusedPoseDe);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AE18 TransitionTable_BA_BB_BC_BD_BE_FacingLeft_GrabbedByDraygon: native priority order.</summary>
    private static SamusPoseInputMatch MatchDraygonGrabbedNeutralLeftPoseList(ushort held)
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

    /// <summary>$91:AE56 TransitionTable_EC_ED_EE_EF_F0_FacingRight_GrabbedByDraygon: native priority order.</summary>
    private static SamusPoseInputMatch MatchDraygonGrabbedNeutralRightPoseList(ushort held)
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

    /// <summary>$91:AE94 TransitionTable_0B_MovingRight_GunExtended: native priority order.</summary>
    private static SamusPoseInputMatch MatchMovingRightGunExtendedPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:AEDE TransitionTable_0C_MovingLeft_GunExtended: native priority order.</summary>
    private static SamusPoseInputMatch MatchMovingLeftGunExtendedPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(held, AimUp | Left))
            return Accept(SamusPoseId.RunningAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Left))
            return Accept(SamusPoseId.RunningAimDiagonalDownLeftPose);
        if (Has(held, Left | Up))
            return Accept(SamusPoseId.RunningAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(SamusPoseId.RunningAimDiagonalDownLeftPose);
        if (Has(held, Shoot | Left))
            return Accept(SamusPoseId.MovingLeftGunExtendedPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MovingLeftGunExtendedPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AF28 TransitionTable_67_FacingRight_Falling_GunExtended: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingGunExtendedRightPoseList(ushort held)
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

    /// <summary>$91:AF60 TransitionTable_68_FacingLeft_Falling_GunExtended: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingGunExtendedLeftPoseList(ushort held)
    {
        if (Has(held, Left | Up))
            return Accept(SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.FallingAimUpLeftPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.FallingAimDownLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightFallingPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.FallingGunExtendedLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.FallingGunExtendedLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AF98 TransitionTable_BF_FacingRight_Moonwalking_TurnJumpLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MoonwalkTurnJumpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AFAC TransitionTable_C0_FacingLeft_Moonwalking_TurnJumpRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MoonwalkTurnJumpRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AFC0 TransitionTable_C1_FaceRight_Moonwalk_TurnJumpLeft_AimUpRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpAimUpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MoonwalkTurnJumpAimUpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AFD4 TransitionTable_C2_FaceLeft_Moonwalk_TurnJumpRight_AimUpLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpAimUpRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MoonwalkTurnJumpAimUpRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AFE8 TransitionTable_C3_FaceRight_Moonwalk_TurnJumpLeft_AimDownRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpAimDownLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MoonwalkTurnJumpAimDownLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:AFFC TransitionTable_C4_FaceLeft_Moonwalk_TurnJumpRight_AimDownLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpAimDownRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MoonwalkTurnJumpAimDownRightPose);
        return new(null, HasConditions: true);
    }
}
