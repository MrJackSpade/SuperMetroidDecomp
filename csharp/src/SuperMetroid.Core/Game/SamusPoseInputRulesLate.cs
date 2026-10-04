using static SuperMetroid.Core.Game.CanonicalPoseButtons;
using static SuperMetroid.Core.Game.SamusPoseInputDefinitions;

namespace SuperMetroid.Core.Game;

/// <summary>Ordered native pose-input decisions; the first satisfied chord wins.</summary>
internal static class SamusPoseInputRulesLate
{
    internal static SamusPoseInputMatch Match(ushort pointer, ushort held, ushort newlyPressed) => pointer switch
    {
        SpringBallGroundRightPoseList => MatchSpringBallGroundRightPoseList(held, newlyPressed),
        SpringBallGroundLeftPoseList => MatchSpringBallGroundLeftPoseList(held, newlyPressed),
        SpringBallFallingRightPoseList => MatchSpringBallFallingRightPoseList(held, newlyPressed),
        SpringBallFallingLeftPoseList => MatchSpringBallFallingLeftPoseList(held, newlyPressed),
        SpringBallJumpRightPoseList => MatchSpringBallJumpRightPoseList(held, newlyPressed),
        SpringBallJumpLeftPoseList => MatchSpringBallJumpLeftPoseList(held, newlyPressed),
        UnusedPose63List => MatchUnusedPose63List(held, newlyPressed),
        UnusedPose64List => MatchUnusedPose64List(held, newlyPressed),
        UnusedPose65List => MatchUnusedPose65List(held, newlyPressed),
        UnusedPose66List => MatchUnusedPose66List(held, newlyPressed),
        WallJumpRightPoseList => MatchWallJumpRightPoseList(held, newlyPressed),
        WallJumpLeftPoseList => MatchWallJumpLeftPoseList(held, newlyPressed),
        RanIntoWallRightPoseList => MatchRanIntoWallRightPoseList(held, newlyPressed),
        RanIntoWallLeftPoseList => MatchRanIntoWallLeftPoseList(held, newlyPressed),
        NormalJumpGunExtendedRightPoseList => MatchNormalJumpGunExtendedRightPoseList(held, newlyPressed),
        NormalJumpGunExtendedLeftPoseList => MatchNormalJumpGunExtendedLeftPoseList(held, newlyPressed),
        NormalJumpAimDownRightPoseList => MatchNormalJumpAimDownRightPoseList(held, newlyPressed),
        NormalJumpAimDownLeftPoseList => MatchNormalJumpAimDownLeftPoseList(held, newlyPressed),
        UnmorphingTransitionRightPoseList => MatchUnmorphingTransitionRightPoseList(held, newlyPressed),
        UnmorphingTransitionLeftPoseList => MatchUnmorphingTransitionLeftPoseList(held, newlyPressed),
        TurningRightToLeftPoseList => MatchTurningRightToLeftPoseList(held, newlyPressed),
        TurningLeftToRightPoseList => MatchTurningLeftToRightPoseList(held, newlyPressed),
        TurningRightToLeftAimUpPoseList => MatchTurningRightToLeftAimUpPoseList(held, newlyPressed),
        TurningLeftToRightAimUpPoseList => MatchTurningLeftToRightAimUpPoseList(held, newlyPressed),
        TurningRightToLeftAimDiagonalDownPoseList => MatchTurningRightToLeftAimDiagonalDownPoseList(held, newlyPressed),
        TurningLeftToRightAimDiagonalDownPoseList => MatchTurningLeftToRightAimDiagonalDownPoseList(held, newlyPressed),
        ShinesparkWindupRightPoseList => MatchShinesparkWindupRightPoseList(held, newlyPressed),
        ShinesparkWindupLeftPoseList => MatchShinesparkWindupLeftPoseList(held, newlyPressed),
        FallingAimDownRightPoseList => MatchFallingAimDownRightPoseList(held, newlyPressed),
        FallingAimDownLeftPoseList => MatchFallingAimDownLeftPoseList(held, newlyPressed),
        UnusedPoseDfList => MatchUnusedPoseDfList(held, newlyPressed),
        DraygonGrabbedNeutralLeftPoseList => MatchDraygonGrabbedNeutralLeftPoseList(held, newlyPressed),
        DraygonGrabbedNeutralRightPoseList => MatchDraygonGrabbedNeutralRightPoseList(held, newlyPressed),
        MovingRightGunExtendedPoseList => MatchMovingRightGunExtendedPoseList(held, newlyPressed),
        MovingLeftGunExtendedPoseList => MatchMovingLeftGunExtendedPoseList(held, newlyPressed),
        FallingGunExtendedRightPoseList => MatchFallingGunExtendedRightPoseList(held, newlyPressed),
        FallingGunExtendedLeftPoseList => MatchFallingGunExtendedLeftPoseList(held, newlyPressed),
        MoonwalkTurnJumpLeftPoseList => MatchMoonwalkTurnJumpLeftPoseList(held, newlyPressed),
        MoonwalkTurnJumpRightPoseList => MatchMoonwalkTurnJumpRightPoseList(held, newlyPressed),
        MoonwalkTurnJumpAimUpLeftPoseList => MatchMoonwalkTurnJumpAimUpLeftPoseList(held, newlyPressed),
        MoonwalkTurnJumpAimUpRightPoseList => MatchMoonwalkTurnJumpAimUpRightPoseList(held, newlyPressed),
        MoonwalkTurnJumpAimDownLeftPoseList => MatchMoonwalkTurnJumpAimDownLeftPoseList(held, newlyPressed),
        MoonwalkTurnJumpAimDownRightPoseList => MatchMoonwalkTurnJumpAimDownRightPoseList(held, newlyPressed),
        _ => throw new InvalidOperationException($"Unknown compiled pose-input list ${pointer:X4}."),
    };

    /// <summary>$91:A90C TransitionTable_79_7B_FacingRight_MorphBall_Spring_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallGroundRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.SpringBallJumpRightPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.SpringBallMovingRightPose);
        if (Has(held, Left))
            return Accept(3, None, Left, SamusPoseId.SpringBallMovingLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:A926 TransitionTable_7A_7C_FacingLeft_MorphBall_Spring_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallGroundLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.SpringBallJumpLeftPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.SpringBallMovingRightPose);
        if (Has(held, Left))
            return Accept(3, None, Left, SamusPoseId.SpringBallMovingLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:A940 TransitionTable_7D_FacingRight_MorphBall_SpringBall_Falling: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallFallingRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Left))
            return Accept(1, None, Left, SamusPoseId.SpringBallFallingLeftPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.SpringBallFallingRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:A954 TransitionTable_7E_FacingLeft_MorphBall_SpringBall_Falling: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallFallingLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(1, None, Right, SamusPoseId.SpringBallFallingRightPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.SpringBallFallingLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:A968 TransitionTable_7F_FacingRight_MorphBall_SpringBall_InAir: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallJumpRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Right))
            return Accept(1, None, Right, SamusPoseId.SpringBallJumpRightPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.SpringBallJumpLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:A97C TransitionTable_80_FacingLeft_MorphBall_SpringBall_InAir: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpringBallJumpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(1, None, Right, SamusPoseId.SpringBallJumpRightPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.SpringBallJumpLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:A990 UNUSED_TransitionTable_63_91A990: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose63List(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(0, None, Jump | Left, SamusPoseId.UnusedPose66);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:A998 UNUSED_TransitionTable_64_91A998: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose64List(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(0, None, Jump | Right, SamusPoseId.UnusedPose65);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:A9A0 UNUSED_TransitionTable_65_91A9A0: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose65List(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(0, None, Jump | Right, SamusPoseId.UnusedPose65);
        if (Has(held, AimUp))
            return Accept(1, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(2, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Shoot))
            return Accept(3, None, Shoot, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Jump))
            return Accept(4, None, Jump, SamusPoseId.UnusedPose65);
        if (Has(held, Right))
            return Accept(5, None, Right, SamusPoseId.UnusedPose65);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:A9C6 UNUSED_TransitionTable_66_91A9C6: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose66List(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(0, None, Jump | Left, SamusPoseId.UnusedPose66);
        if (Has(held, AimUp))
            return Accept(1, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(2, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Shoot))
            return Accept(3, None, Shoot, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Jump))
            return Accept(4, None, Jump, SamusPoseId.UnusedPose66);
        if (Has(held, Left))
            return Accept(5, None, Left, SamusPoseId.UnusedPose66);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:A9EC TransitionTable_83_FacingRight_WallJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchWallJumpRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(0, Down, None, SamusPoseId.MorphingTransitionRightPose);
        if (Has(held, Left))
            return Accept(1, None, Left, SamusPoseId.SpinJumpLeftPose);
        if (Has(held, AimUp))
            return Accept(2, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(3, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Shoot))
            return Accept(4, None, Shoot, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Jump))
            return Accept(5, None, Jump, SamusPoseId.WallJumpRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AA12 TransitionTable_84_FacingLeft_WallJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchWallJumpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(0, Down, None, SamusPoseId.MorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(1, None, Right, SamusPoseId.SpinJumpRightPose);
        if (Has(held, AimUp))
            return Accept(2, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(3, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Shoot))
            return Accept(4, None, Shoot, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Jump))
            return Accept(5, None, Jump, SamusPoseId.WallJumpLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AA38 TransitionTable_89_CF_D1_FacingRight_RanIntoAWall: native priority order.</summary>
    private static SamusPoseInputMatch MatchRanIntoWallRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump))
            return Accept(0, Jump, None, SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right | Up))
            return Accept(1, None, Right | Up, SamusPoseId.RunningAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(2, None, Right | Down, SamusPoseId.RunningAimDiagonalDownRightPose);
        if (Has(newlyPressed, Down))
            return Accept(3, Down, None, SamusPoseId.CrouchingTransitionRightPose);
        if (Has(held, AimDown | Left))
            return Accept(4, None, AimDown | Left, SamusPoseId.MoonwalkAimDownRightPose);
        if (Has(held, AimUp | Left))
            return Accept(5, None, AimUp | Left, SamusPoseId.MoonwalkAimUpRightPose);
        if (Has(held, Up))
            return Accept(6, None, Up, SamusPoseId.StandingAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(7, None, AimUp, SamusPoseId.StandingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(8, None, AimDown, SamusPoseId.StandingAimDiagonalDownRightPose);
        if (Has(held, Left))
            return Accept(9, None, Left, SamusPoseId.TurningRightToLeftPose);
        if (Has(held, Right))
            return Accept(10, None, Right, SamusPoseId.MovingRightNormalPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AA7C TransitionTable_8A_D0_D2_FacingLeft_RanIntoAWall: native priority order.</summary>
    private static SamusPoseInputMatch MatchRanIntoWallLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump))
            return Accept(0, Jump, None, SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left | Up))
            return Accept(1, None, Left | Up, SamusPoseId.RunningAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(2, None, Left | Down, SamusPoseId.RunningAimDiagonalDownLeftPose);
        if (Has(newlyPressed, Down))
            return Accept(3, Down, None, SamusPoseId.CrouchingTransitionLeftPose);
        if (Has(held, AimDown | Right))
            return Accept(4, None, AimDown | Right, SamusPoseId.MoonwalkAimDownLeftPose);
        if (Has(held, AimUp | Right))
            return Accept(5, None, AimUp | Right, SamusPoseId.MoonwalkAimUpLeftPose);
        if (Has(held, Up))
            return Accept(6, None, Up, SamusPoseId.StandingAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(7, None, AimUp, SamusPoseId.StandingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(8, None, AimDown, SamusPoseId.StandingAimDiagonalDownLeftPose);
        if (Has(held, Right))
            return Accept(9, None, Right, SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Left))
            return Accept(10, None, Left, SamusPoseId.MovingLeftNormalPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AAC0 TransitionTable_13_FaceRight_NormalJump_NotMoving_GunExtend: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpGunExtendedRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Right | Up))
            return Accept(0, None, Jump | Right | Up, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, Jump | Right | Down))
            return Accept(1, None, Jump | Right | Down, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, AimUp | Jump | Right))
            return Accept(2, None, AimUp | Jump | Right, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Jump | Right))
            return Accept(3, None, AimDown | Jump | Right, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Right | Up))
            return Accept(4, None, Right | Up, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(5, None, Right | Down, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Left))
            return Accept(6, None, Jump | Left, SamusPoseId.TurningRightToLeftJumpPose);
        if (Has(held, Jump | Up))
            return Accept(7, None, Jump | Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Jump | Down))
            return Accept(8, None, Jump | Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Jump))
            return Accept(9, None, AimUp | Jump, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Jump))
            return Accept(10, None, AimDown | Jump, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(11, None, Jump | Right, SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Shoot | Jump))
            return Accept(12, None, Shoot | Jump, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Left))
            return Accept(13, None, Left, SamusPoseId.TurningRightToLeftJumpPose);
        if (Has(held, Up))
            return Accept(14, None, Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Down))
            return Accept(15, None, Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp))
            return Accept(16, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(17, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Right))
            return Accept(18, None, Right, SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Shoot))
            return Accept(19, None, Shoot, SamusPoseId.NormalJumpGunExtendedRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AB3A TransitionTable_14_FacingLeft_NormalJump_NotMoving_GunExtend: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpGunExtendedLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left | Up))
            return Accept(0, None, Jump | Left | Up, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, Jump | Left | Down))
            return Accept(1, None, Jump | Left | Down, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, AimUp | Jump | Left))
            return Accept(2, None, AimUp | Jump | Left, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Jump | Left))
            return Accept(3, None, AimDown | Jump | Left, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Left | Up))
            return Accept(4, None, Left | Up, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(5, None, Left | Down, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Right))
            return Accept(6, None, Jump | Right, SamusPoseId.TurningLeftToRightJumpPose);
        if (Has(held, Jump | Up))
            return Accept(7, None, Jump | Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Jump | Down))
            return Accept(8, None, Jump | Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp | Jump))
            return Accept(9, None, AimUp | Jump, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Jump))
            return Accept(10, None, AimDown | Jump, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Left))
            return Accept(11, None, Jump | Left, SamusPoseId.NormalJumpForwardLeftPose);
        if (Has(held, Shoot | Jump))
            return Accept(12, None, Shoot | Jump, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Right))
            return Accept(13, None, Right, SamusPoseId.TurningLeftToRightJumpPose);
        if (Has(held, Up))
            return Accept(14, None, Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Down))
            return Accept(15, None, Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp))
            return Accept(16, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(17, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Left))
            return Accept(18, None, Left, SamusPoseId.NormalJumpForwardLeftPose);
        if (Has(held, Shoot))
            return Accept(19, None, Shoot, SamusPoseId.NormalJumpGunExtendedLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:ABB4 TransitionTable_17_FacingRight_NormalJump_AimingDown: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpAimDownRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(0, Down, None, SamusPoseId.MorphingTransitionRightPose);
        if (Has(held, Jump | Right | Up))
            return Accept(1, None, Jump | Right | Up, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, Jump | Right | Down))
            return Accept(2, None, Jump | Right | Down, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, AimUp | Jump | Right))
            return Accept(3, None, AimUp | Jump | Right, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Jump | Right))
            return Accept(4, None, AimDown | Jump | Right, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Shoot | Jump | Right))
            return Accept(5, None, Shoot | Jump | Right, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Right | Up))
            return Accept(6, None, Right | Up, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(7, None, Right | Down, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Left))
            return Accept(8, None, Jump | Left, SamusPoseId.TurningRightToLeftJumpPose);
        if (Has(held, Jump | Up))
            return Accept(9, None, Jump | Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Jump | Down))
            return Accept(10, None, Jump | Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Jump))
            return Accept(11, None, AimUp | Jump, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Jump))
            return Accept(12, None, AimDown | Jump, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(13, None, Jump | Right, SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Shoot | Jump))
            return Accept(14, None, Shoot | Jump, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Left))
            return Accept(15, None, Left, SamusPoseId.TurningRightToLeftJumpPose);
        if (Has(held, Up))
            return Accept(16, None, Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Down))
            return Accept(17, None, Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp))
            return Accept(18, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(19, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Right))
            return Accept(20, None, Right, SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Jump))
            return Accept(21, None, Jump, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, Shoot))
            return Accept(22, None, Shoot, SamusPoseId.NormalJumpGunExtendedRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AC40 TransitionTable_18_FacingLeft_NormalJump_AimingDown: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpAimDownLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(0, Down, None, SamusPoseId.MorphingTransitionLeftPose);
        if (Has(held, Jump | Left | Up))
            return Accept(1, None, Jump | Left | Up, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, Jump | Left | Down))
            return Accept(2, None, Jump | Left | Down, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, AimUp | Jump | Left))
            return Accept(3, None, AimUp | Jump | Left, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Jump | Left))
            return Accept(4, None, AimDown | Jump | Left, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, AimDown | Jump | Left))
            return Accept(5, None, AimDown | Jump | Left, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Left | Up))
            return Accept(6, None, Left | Up, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(7, None, Left | Down, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Right))
            return Accept(8, None, Jump | Right, SamusPoseId.TurningLeftToRightJumpPose);
        if (Has(held, Jump | Up))
            return Accept(9, None, Jump | Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Jump | Down))
            return Accept(10, None, Jump | Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp | Jump))
            return Accept(11, None, AimUp | Jump, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Jump))
            return Accept(12, None, AimDown | Jump, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Left))
            return Accept(13, None, Jump | Left, SamusPoseId.NormalJumpForwardLeftPose);
        if (Has(held, Shoot | Jump))
            return Accept(14, None, Shoot | Jump, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Right))
            return Accept(15, None, Right, SamusPoseId.TurningLeftToRightJumpPose);
        if (Has(held, Up))
            return Accept(16, None, Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Down))
            return Accept(17, None, Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp))
            return Accept(18, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(19, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Left))
            return Accept(20, None, Left, SamusPoseId.NormalJumpForwardLeftPose);
        if (Has(held, Jump))
            return Accept(21, None, Jump, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, Shoot))
            return Accept(22, None, Shoot, SamusPoseId.NormalJumpGunExtendedLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:ACCC TransitionTable_3D_FacingRight_Unmorphing: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnmorphingTransitionRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Shoot | Right))
            return Accept(0, None, Shoot | Right, SamusPoseId.FallingGunExtendedRightPose);
        if (Has(held, Shoot | Up))
            return Accept(1, None, Shoot | Up, SamusPoseId.FallingAimUpRightPose);
        if (Has(held, Shoot | Down))
            return Accept(2, None, Shoot | Down, SamusPoseId.FallingAimDownRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:ACE0 TransitionTable_3E_FacingLeft_Unmorphing: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnmorphingTransitionLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Shoot | Left))
            return Accept(0, None, Shoot | Left, SamusPoseId.FallingGunExtendedLeftPose);
        if (Has(held, Shoot | Up))
            return Accept(1, None, Shoot | Up, SamusPoseId.FallingAimUpLeftPose);
        if (Has(held, Shoot | Down))
            return Accept(2, None, Shoot | Down, SamusPoseId.FallingAimDownLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:ACF4 TransitionTable_25_FacingRight_Turning_Standing: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningRightToLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(0, None, Jump | Left, SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.TurningRightToLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AD08 TransitionTable_26_FacingLeft_Turning_Standing: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningLeftToRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(0, None, Jump | Right, SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.TurningLeftToRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AD1C TransitionTable_8B_FacingRight_Turning_Standing_AimingUp: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningRightToLeftAimUpPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(0, Jump, Left, SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.TurningRightToLeftAimUpPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AD30 TransitionTable_8C_FacingLeft_Turning_Standing_AimingUp: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningLeftToRightAimUpPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(0, Jump, Right, SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.TurningLeftToRightAimUpPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AD44 TransitionTable_8D_FacingRight_Turning_Standing_AimDownRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningRightToLeftAimDiagonalDownPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(0, Jump, Left, SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.TurningRightToLeftAimDiagonalDownPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AD58 TransitionTable_8E_FacingLeft_Turning_Standing_AimDownLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchTurningLeftToRightAimDiagonalDownPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(0, Jump, Right, SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.TurningLeftToRightAimDiagonalDownPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AD6C TransitionTable_C7_FacingRight_VerticalShinesparkWindup: native priority order.</summary>
    private static SamusPoseInputMatch MatchShinesparkWindupRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Up))
            return Accept(0, None, Jump | Up, SamusPoseId.ShinesparkVerticalRightPose);
        if (Has(held, AimUp | Jump))
            return Accept(1, None, AimUp | Jump, SamusPoseId.ShinesparkDiagonalRightPose);
        if (Has(held, Jump | Right))
            return Accept(2, None, Jump | Right, SamusPoseId.ShinesparkHorizontalRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AD80 TransitionTable_C8_FacingLeft_VerticalShinesparkWindup: native priority order.</summary>
    private static SamusPoseInputMatch MatchShinesparkWindupLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Up))
            return Accept(0, None, Jump | Up, SamusPoseId.ShinesparkVerticalLeftPose);
        if (Has(held, AimUp | Jump))
            return Accept(1, None, AimUp | Jump, SamusPoseId.ShinesparkDiagonalLeftPose);
        if (Has(held, Jump | Left))
            return Accept(2, None, Jump | Left, SamusPoseId.ShinesparkHorizontalLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AD94 TransitionTable_2D_FacingRight_Falling_AimingDown: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingAimDownRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(0, Down, None, SamusPoseId.MorphingTransitionRightPose);
        if (Has(held, Right | Up))
            return Accept(1, None, Right | Up, SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(2, None, Right | Down, SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Up))
            return Accept(3, None, Up, SamusPoseId.FallingAimUpRightPose);
        if (Has(held, Down))
            return Accept(4, None, Down, SamusPoseId.FallingAimDownRightPose);
        if (Has(held, Left))
            return Accept(5, None, Left, SamusPoseId.TurningRightToLeftFallingPose);
        if (Has(held, AimUp))
            return Accept(6, None, AimUp, SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(7, None, AimDown, SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Shoot))
            return Accept(8, None, Shoot, SamusPoseId.FallingGunExtendedRightPose);
        if (Has(held, Right))
            return Accept(9, None, Right, SamusPoseId.FallingRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:ADD2 TransitionTable_2E_FacingLeft_Falling_AimingDown: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingAimDownLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(0, Down, None, SamusPoseId.MorphingTransitionLeftPose);
        if (Has(held, Left | Up))
            return Accept(1, None, Left | Up, SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(2, None, Left | Down, SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Up))
            return Accept(3, None, Up, SamusPoseId.FallingAimUpLeftPose);
        if (Has(held, Down))
            return Accept(4, None, Down, SamusPoseId.FallingAimDownLeftPose);
        if (Has(held, Right))
            return Accept(5, None, Right, SamusPoseId.TurningLeftToRightFallingPose);
        if (Has(held, AimUp))
            return Accept(6, None, AimUp, SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(7, None, AimDown, SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Shoot))
            return Accept(8, None, Shoot, SamusPoseId.FallingGunExtendedLeftPose);
        if (Has(held, Left))
            return Accept(9, None, Left, SamusPoseId.FallingLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AE10 UNUSED_TransitionTable_DF_91AE10: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPoseDfList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnusedPoseDe);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AE18 TransitionTable_BA_BB_BC_BD_BE_FacingLeft_GrabbedByDraygon: native priority order.</summary>
    private static SamusPoseInputMatch MatchDraygonGrabbedNeutralLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Shoot | Left | Up))
            return Accept(0, None, Shoot | Left | Up, SamusPoseId.DraygonGrabbedAimUpLeftPose);
        if (Has(held, Shoot | Left | Down))
            return Accept(1, None, Shoot | Left | Down, SamusPoseId.DraygonGrabbedAimDownLeftPose);
        if (Has(held, Shoot | Left))
            return Accept(2, None, Shoot | Left, SamusPoseId.DraygonGrabbedFiringLeftPose);
        if (Has(held, AimUp))
            return Accept(3, None, AimUp, SamusPoseId.DraygonGrabbedAimUpLeftPose);
        if (Has(held, AimDown))
            return Accept(4, None, AimDown, SamusPoseId.DraygonGrabbedAimDownLeftPose);
        if (Has(held, Shoot))
            return Accept(5, None, Shoot, SamusPoseId.DraygonGrabbedFiringLeftPose);
        if (Has(held, Left))
            return Accept(6, None, Left, SamusPoseId.DraygonGrabbedMovingLeftPose);
        if (Has(held, Right))
            return Accept(7, None, Right, SamusPoseId.DraygonGrabbedMovingLeftPose);
        if (Has(held, Up))
            return Accept(8, None, Up, SamusPoseId.DraygonGrabbedMovingLeftPose);
        if (Has(held, Down))
            return Accept(9, None, Down, SamusPoseId.DraygonGrabbedMovingLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AE56 TransitionTable_EC_ED_EE_EF_F0_FacingRight_GrabbedByDraygon: native priority order.</summary>
    private static SamusPoseInputMatch MatchDraygonGrabbedNeutralRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Shoot | Right | Up))
            return Accept(0, None, Shoot | Right | Up, SamusPoseId.DraygonGrabbedAimUpRightPose);
        if (Has(held, Shoot | Right | Down))
            return Accept(1, None, Shoot | Right | Down, SamusPoseId.DraygonGrabbedAimDownRightPose);
        if (Has(held, Shoot | Right))
            return Accept(2, None, Shoot | Right, SamusPoseId.DraygonGrabbedFiringRightPose);
        if (Has(held, AimUp))
            return Accept(3, None, AimUp, SamusPoseId.DraygonGrabbedAimUpRightPose);
        if (Has(held, AimDown))
            return Accept(4, None, AimDown, SamusPoseId.DraygonGrabbedAimDownRightPose);
        if (Has(held, Shoot))
            return Accept(5, None, Shoot, SamusPoseId.DraygonGrabbedFiringRightPose);
        if (Has(held, Left))
            return Accept(6, None, Left, SamusPoseId.DraygonGrabbedMovingRightPose);
        if (Has(held, Right))
            return Accept(7, None, Right, SamusPoseId.DraygonGrabbedMovingRightPose);
        if (Has(held, Up))
            return Accept(8, None, Up, SamusPoseId.DraygonGrabbedMovingRightPose);
        if (Has(held, Down))
            return Accept(9, None, Down, SamusPoseId.DraygonGrabbedMovingRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AE94 TransitionTable_0B_MovingRight_GunExtended: native priority order.</summary>
    private static SamusPoseInputMatch MatchMovingRightGunExtendedPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(0, Down, None, SamusPoseId.CrouchingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.SpinJumpRightPose);
        if (Has(held, AimUp | Right))
            return Accept(2, None, AimUp | Right, SamusPoseId.RunningAimDiagonalUpRightPose);
        if (Has(held, AimDown | Right))
            return Accept(3, None, AimDown | Right, SamusPoseId.RunningAimDiagonalDownRightPose);
        if (Has(held, Right | Up))
            return Accept(4, None, Right | Up, SamusPoseId.RunningAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(5, None, Right | Down, SamusPoseId.RunningAimDiagonalDownRightPose);
        if (Has(held, Shoot | Right))
            return Accept(6, None, Shoot | Right, SamusPoseId.MovingRightGunExtendedPose);
        if (Has(held, Right))
            return Accept(7, None, Right, SamusPoseId.MovingRightGunExtendedPose);
        if (Has(held, Left))
            return Accept(8, None, Left, SamusPoseId.TurningRightToLeftPose);
        if (Has(held, Up))
            return Accept(9, None, Up, SamusPoseId.StandingAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(10, None, AimUp, SamusPoseId.StandingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(11, None, AimDown, SamusPoseId.StandingAimDiagonalDownRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AEDE TransitionTable_0C_MovingLeft_GunExtended: native priority order.</summary>
    private static SamusPoseInputMatch MatchMovingLeftGunExtendedPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(0, Down, None, SamusPoseId.CrouchingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.SpinJumpLeftPose);
        if (Has(held, AimUp | Left))
            return Accept(2, None, AimUp | Left, SamusPoseId.RunningAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Left))
            return Accept(3, None, AimDown | Left, SamusPoseId.RunningAimDiagonalDownLeftPose);
        if (Has(held, Left | Up))
            return Accept(4, None, Left | Up, SamusPoseId.RunningAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(5, None, Left | Down, SamusPoseId.RunningAimDiagonalDownLeftPose);
        if (Has(held, Shoot | Left))
            return Accept(6, None, Shoot | Left, SamusPoseId.MovingLeftGunExtendedPose);
        if (Has(held, Left))
            return Accept(7, None, Left, SamusPoseId.MovingLeftGunExtendedPose);
        if (Has(held, Right))
            return Accept(8, None, Right, SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Up))
            return Accept(9, None, Up, SamusPoseId.StandingAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(10, None, AimUp, SamusPoseId.StandingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(11, None, AimDown, SamusPoseId.StandingAimDiagonalDownLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AF28 TransitionTable_67_FacingRight_Falling_GunExtended: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingGunExtendedRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Right | Up))
            return Accept(0, None, Right | Up, SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(1, None, Right | Down, SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Up))
            return Accept(2, None, Up, SamusPoseId.FallingAimUpRightPose);
        if (Has(held, Down))
            return Accept(3, None, Down, SamusPoseId.FallingAimDownRightPose);
        if (Has(held, Left))
            return Accept(4, None, Left, SamusPoseId.TurningRightToLeftFallingPose);
        if (Has(held, AimUp))
            return Accept(5, None, AimUp, SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(6, None, AimDown, SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Shoot))
            return Accept(7, None, Shoot, SamusPoseId.FallingGunExtendedRightPose);
        if (Has(held, Right))
            return Accept(8, None, Right, SamusPoseId.FallingGunExtendedRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AF60 TransitionTable_68_FacingLeft_Falling_GunExtended: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingGunExtendedLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Left | Up))
            return Accept(0, None, Left | Up, SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(1, None, Left | Down, SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Up))
            return Accept(2, None, Up, SamusPoseId.FallingAimUpLeftPose);
        if (Has(held, Down))
            return Accept(3, None, Down, SamusPoseId.FallingAimDownLeftPose);
        if (Has(held, Right))
            return Accept(4, None, Right, SamusPoseId.TurningLeftToRightFallingPose);
        if (Has(held, AimUp))
            return Accept(5, None, AimUp, SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(6, None, AimDown, SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Shoot))
            return Accept(7, None, Shoot, SamusPoseId.FallingGunExtendedLeftPose);
        if (Has(held, Left))
            return Accept(8, None, Left, SamusPoseId.FallingGunExtendedLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AF98 TransitionTable_BF_FacingRight_Moonwalking_TurnJumpLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(0, None, Jump | Left, SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.MoonwalkTurnJumpLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AFAC TransitionTable_C0_FacingLeft_Moonwalking_TurnJumpRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(0, None, Jump | Right, SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.MoonwalkTurnJumpRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AFC0 TransitionTable_C1_FaceRight_Moonwalk_TurnJumpLeft_AimUpRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpAimUpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(0, Jump, Left, SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.MoonwalkTurnJumpAimUpLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AFD4 TransitionTable_C2_FaceLeft_Moonwalk_TurnJumpRight_AimUpLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpAimUpRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(0, Jump, Right, SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.MoonwalkTurnJumpAimUpRightPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AFE8 TransitionTable_C3_FaceRight_Moonwalk_TurnJumpLeft_AimDownRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpAimDownLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(0, Jump, Left, SamusPoseId.SpinJumpLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.MoonwalkTurnJumpAimDownLeftPose);
        return new(null, -1, HasConditions: true);
    }

    /// <summary>$91:AFFC TransitionTable_C4_FaceLeft_Moonwalk_TurnJumpRight_AimDownLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpAimDownRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(0, Jump, Right, SamusPoseId.SpinJumpRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.MoonwalkTurnJumpAimDownRightPose);
        return new(null, -1, HasConditions: true);
    }
}
