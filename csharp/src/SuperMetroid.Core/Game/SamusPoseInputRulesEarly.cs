using static SuperMetroid.Core.Game.CanonicalPoseButtons;
using static SuperMetroid.Core.Game.SamusPoseInputDefinitions;

namespace SuperMetroid.Core.Game;

/// <summary>Ordered native pose-input decisions; the first satisfied chord wins.</summary>
internal static class SamusPoseInputRulesEarly
{
    internal static SamusPoseInputMatch Match(ushort pointer, ushort held, ushort newlyPressed) => pointer switch
    {
        EmptyTransitionList => MatchEmptyTransitionList(held, newlyPressed),
        ForwardFacingPowerSuitPoseList => MatchForwardFacingPowerSuitPoseList(held, newlyPressed),
        FacingRightNormalPoseList => MatchFacingRightNormalPoseList(held, newlyPressed),
        FacingLeftNormalPoseList => MatchFacingLeftNormalPoseList(held, newlyPressed),
        MovingRightNormalPoseList => MatchMovingRightNormalPoseList(held, newlyPressed),
        MovingLeftNormalPoseList => MatchMovingLeftNormalPoseList(held, newlyPressed),
        NeutralJumpTransitionRightPoseList => MatchNeutralJumpTransitionRightPoseList(held, newlyPressed),
        NeutralJumpTransitionLeftPoseList => MatchNeutralJumpTransitionLeftPoseList(held, newlyPressed),
        NormalJumpAimUpRightPoseList => MatchNormalJumpAimUpRightPoseList(held, newlyPressed),
        NormalJumpAimUpLeftPoseList => MatchNormalJumpAimUpLeftPoseList(held, newlyPressed),
        DamageBoostLeftPoseList => MatchDamageBoostLeftPoseList(held, newlyPressed),
        DamageBoostRightPoseList => MatchDamageBoostRightPoseList(held, newlyPressed),
        SpinJumpRightPoseList => MatchSpinJumpRightPoseList(held, newlyPressed),
        SpinJumpLeftPoseList => MatchSpinJumpLeftPoseList(held, newlyPressed),
        SpaceJumpRightPoseList => MatchSpaceJumpRightPoseList(held, newlyPressed),
        SpaceJumpLeftPoseList => MatchSpaceJumpLeftPoseList(held, newlyPressed),
        ScrewAttackRightPoseList => MatchScrewAttackRightPoseList(held, newlyPressed),
        ScrewAttackLeftPoseList => MatchScrewAttackLeftPoseList(held, newlyPressed),
        MorphBallGroundRightPoseList => MatchMorphBallGroundRightPoseList(held, newlyPressed),
        MorphBallMovingRightPoseList => MatchMorphBallMovingRightPoseList(held, newlyPressed),
        MorphBallMovingLeftPoseList => MatchMorphBallMovingLeftPoseList(held, newlyPressed),
        MorphBallGroundLeftPoseList => MatchMorphBallGroundLeftPoseList(held, newlyPressed),
        UnusedPose20List => MatchUnusedPose20List(held, newlyPressed),
        UnusedPose23List => MatchUnusedPose23List(held, newlyPressed),
        UnusedPose42List => MatchUnusedPose42List(held, newlyPressed),
        CrouchingRightPoseList => MatchCrouchingRightPoseList(held, newlyPressed),
        CrouchingLeftPoseList => MatchCrouchingLeftPoseList(held, newlyPressed),
        FallingRightPoseList => MatchFallingRightPoseList(held, newlyPressed),
        FallingLeftPoseList => MatchFallingLeftPoseList(held, newlyPressed),
        MorphBallFallingRightPoseList => MatchMorphBallFallingRightPoseList(held, newlyPressed),
        MorphBallFallingLeftPoseList => MatchMorphBallFallingLeftPoseList(held, newlyPressed),
        UnusedKnockbackRightPoseList => MatchUnusedKnockbackRightPoseList(held, newlyPressed),
        UnusedKnockbackLeftPoseList => MatchUnusedKnockbackLeftPoseList(held, newlyPressed),
        UnusedPose45List => MatchUnusedPose45List(held, newlyPressed),
        UnusedPose46List => MatchUnusedPose46List(held, newlyPressed),
        UnusedPose47List => MatchUnusedPose47List(held, newlyPressed),
        UnusedPose48List => MatchUnusedPose48List(held, newlyPressed),
        MoonwalkFacingLeftPoseList => MatchMoonwalkFacingLeftPoseList(held, newlyPressed),
        MoonwalkFacingRightPoseList => MatchMoonwalkFacingRightPoseList(held, newlyPressed),
        KnockbackRightPoseList => MatchKnockbackRightPoseList(held, newlyPressed),
        KnockbackLeftPoseList => MatchKnockbackLeftPoseList(held, newlyPressed),
        UnusedPose5BList => MatchUnusedPose5BList(held, newlyPressed),
        UnusedPose5CList => MatchUnusedPose5CList(held, newlyPressed),
        _ => throw new InvalidOperationException($"Unknown compiled pose-input list ${pointer:X4}."),
    };

    /// <summary>$91:A0DC TransitionTable list for pose $2F: native priority order.</summary>
    private static SamusPoseInputMatch MatchEmptyTransitionList(ushort held, ushort newlyPressed)
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A0DE TransitionTable_00_9B_FacingForward: native priority order.</summary>
    private static SamusPoseInputMatch MatchForwardFacingPowerSuitPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Right))
            return Accept(0, None, Right, SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Left))
            return Accept(1, None, Left, SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A0EC TransitionTable_01_03_05_07_A4_A6_E0_E2_E4_E6_FacingRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchFacingRightNormalPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Up))
            return Accept(0, Jump, Up, SamusPoseId.NormalJumpTransitionAimUpRightPose);
        if (Has(newlyPressed, Jump) && Has(held, AimUp))
            return Accept(1, Jump, AimUp, SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose);
        if (Has(newlyPressed, Jump) && Has(held, AimDown))
            return Accept(2, Jump, AimDown, SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(3, Jump, None, SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(newlyPressed, Down) && Has(held, AimUp | AimDown))
            return Accept(4, Down, AimUp | AimDown, SamusPoseId.CrouchingTransitionAimUpRightPose);
        if (Has(newlyPressed, Down) && Has(held, AimUp))
            return Accept(5, Down, AimUp, SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose);
        if (Has(newlyPressed, Down) && Has(held, AimDown))
            return Accept(6, Down, AimDown, SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose);
        if (Has(newlyPressed, Down))
            return Accept(7, Down, None, SamusPoseId.CrouchingTransitionRightPose);
        if (Has(held, AimDown | Shoot | Left))
            return Accept(8, None, AimDown | Shoot | Left, SamusPoseId.MoonwalkAimDownRightPose);
        if (Has(held, AimUp | Shoot | Left))
            return Accept(9, None, AimUp | Shoot | Left, SamusPoseId.MoonwalkAimUpRightPose);
        if (Has(held, AimUp | AimDown | Left))
            return Accept(10, None, AimUp | AimDown | Left, SamusPoseId.TurningRightToLeftPose);
        if (Has(held, AimUp | AimDown))
            return Accept(11, None, AimUp | AimDown, SamusPoseId.StandingAimUpRightPose);
        if (Has(held, AimUp | Right))
            return Accept(12, None, AimUp | Right, SamusPoseId.RunningAimDiagonalUpRightPose);
        if (Has(held, AimDown | Right))
            return Accept(13, None, AimDown | Right, SamusPoseId.RunningAimDiagonalDownRightPose);
        if (Has(held, Right | Up))
            return Accept(14, None, Right | Up, SamusPoseId.RunningAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(15, None, Right | Down, SamusPoseId.RunningAimDiagonalDownRightPose);
        if (Has(held, Shoot | Left))
            return Accept(16, None, Shoot | Left, SamusPoseId.MoonwalkFacingRightPose);
        if (Has(held, Left))
            return Accept(17, None, Left, SamusPoseId.TurningRightToLeftPose);
        if (Has(held, Up))
            return Accept(18, None, Up, SamusPoseId.StandingAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(19, None, AimUp, SamusPoseId.StandingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(20, None, AimDown, SamusPoseId.StandingAimDiagonalDownRightPose);
        if (Has(held, Right))
            return Accept(21, None, Right, SamusPoseId.MovingRightNormalPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A172 TransitionTable_02_04_06_08_A5_A7_E1_E3_E5_E7_FacingLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchFacingLeftNormalPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Up))
            return Accept(0, Jump, Up, SamusPoseId.NormalJumpTransitionAimUpLeftPose);
        if (Has(newlyPressed, Jump) && Has(held, AimUp))
            return Accept(1, Jump, AimUp, SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose);
        if (Has(newlyPressed, Jump) && Has(held, AimDown))
            return Accept(2, Jump, AimDown, SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(3, Jump, None, SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(newlyPressed, Down) && Has(held, AimUp | AimDown))
            return Accept(4, Down, AimUp | AimDown, SamusPoseId.CrouchingTransitionAimUpLeftPose);
        if (Has(newlyPressed, Down) && Has(held, AimUp))
            return Accept(5, Down, AimUp, SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose);
        if (Has(newlyPressed, Down) && Has(held, AimDown))
            return Accept(6, Down, AimDown, SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose);
        if (Has(newlyPressed, Down))
            return Accept(7, Down, None, SamusPoseId.CrouchingTransitionLeftPose);
        if (Has(held, AimDown | Shoot | Right))
            return Accept(8, None, AimDown | Shoot | Right, SamusPoseId.MoonwalkAimDownLeftPose);
        if (Has(held, AimUp | Shoot | Right))
            return Accept(9, None, AimUp | Shoot | Right, SamusPoseId.MoonwalkAimUpLeftPose);
        if (Has(held, AimUp | AimDown | Right))
            return Accept(10, None, AimUp | AimDown | Right, SamusPoseId.TurningLeftToRightPose);
        if (Has(held, AimUp | AimDown))
            return Accept(11, None, AimUp | AimDown, SamusPoseId.StandingAimUpLeftPose);
        if (Has(held, AimUp | Left))
            return Accept(12, None, AimUp | Left, SamusPoseId.RunningAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Left))
            return Accept(13, None, AimDown | Left, SamusPoseId.RunningAimDiagonalDownLeftPose);
        if (Has(held, Left | Up))
            return Accept(14, None, Left | Up, SamusPoseId.RunningAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(15, None, Left | Down, SamusPoseId.RunningAimDiagonalDownLeftPose);
        if (Has(held, Shoot | Right))
            return Accept(16, None, Shoot | Right, SamusPoseId.MoonwalkFacingLeftPose);
        if (Has(held, Right))
            return Accept(17, None, Right, SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Up))
            return Accept(18, None, Up, SamusPoseId.StandingAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(19, None, AimUp, SamusPoseId.StandingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(20, None, AimDown, SamusPoseId.StandingAimDiagonalDownLeftPose);
        if (Has(held, Left))
            return Accept(21, None, Left, SamusPoseId.MovingLeftNormalPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A1F8 TransitionTable_09_0D_0F_11_MovingRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchMovingRightNormalPoseList(ushort held, ushort newlyPressed)
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
            return Accept(7, None, Right, SamusPoseId.MovingRightNormalPose);
        if (Has(held, Left))
            return Accept(8, None, Left, SamusPoseId.TurningRightToLeftPose);
        if (Has(held, Up))
            return Accept(9, None, Up, SamusPoseId.StandingAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(10, None, AimUp, SamusPoseId.StandingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(11, None, AimDown, SamusPoseId.StandingAimDiagonalDownRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A242 TransitionTable_0A_0E_10_12_MovingLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchMovingLeftNormalPoseList(ushort held, ushort newlyPressed)
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
            return Accept(7, None, Left, SamusPoseId.MovingLeftNormalPose);
        if (Has(held, Right))
            return Accept(8, None, Right, SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Up))
            return Accept(9, None, Up, SamusPoseId.StandingAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(10, None, AimUp, SamusPoseId.StandingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(11, None, AimDown, SamusPoseId.StandingAimDiagonalDownLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A28C TransitionTable_4B_55_57_59_FacingRight_NormalJumpTransition: native priority order.</summary>
    private static SamusPoseInputMatch MatchNeutralJumpTransitionRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(0, None, Jump | Left, SamusPoseId.TurningRightToLeftJumpPose);
        if (Has(held, Jump | Up))
            return Accept(1, None, Jump | Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Jump | Down))
            return Accept(2, None, Jump | Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Jump))
            return Accept(3, None, AimUp | Jump, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Jump))
            return Accept(4, None, AimDown | Jump, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(5, None, Jump | Right, SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Shoot | Jump))
            return Accept(6, None, Shoot | Jump, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Shoot))
            return Accept(7, None, Shoot, SamusPoseId.NormalJumpGunExtendedRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A2BE TransitionTable_4C_56_58_5A_FacingLeft_NormalJumpTransition: native priority order.</summary>
    private static SamusPoseInputMatch MatchNeutralJumpTransitionLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(0, None, Jump | Right, SamusPoseId.TurningLeftToRightJumpPose);
        if (Has(held, Jump | Up))
            return Accept(1, None, Jump | Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Jump | Down))
            return Accept(2, None, Jump | Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp | Jump))
            return Accept(3, None, AimUp | Jump, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Jump))
            return Accept(4, None, AimDown | Jump, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Left))
            return Accept(5, None, Jump | Left, SamusPoseId.NormalJumpForwardLeftPose);
        if (Has(held, Shoot | Jump))
            return Accept(6, None, Shoot | Jump, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Right))
            return Accept(7, None, Right, SamusPoseId.TurningLeftToRightJumpPose);
        if (Has(held, Shoot))
            return Accept(8, None, Shoot, SamusPoseId.NormalJumpGunExtendedLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A2F6 TransitionTable_15_4D_51_69_6B_FacingRight_NormalJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpAimUpRightPoseList(ushort held, ushort newlyPressed)
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
        if (Has(held, Jump))
            return Accept(19, None, Jump, SamusPoseId.NeutralJumpRightPose);
        if (Has(held, Shoot))
            return Accept(20, None, Shoot, SamusPoseId.NormalJumpGunExtendedRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A376 TransitionTable_16_4E_52_6A_6C_FacingLeft_NormalJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpAimUpLeftPoseList(ushort held, ushort newlyPressed)
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
        if (Has(held, Jump))
            return Accept(19, None, Jump, SamusPoseId.NeutralJumpLeftPose);
        if (Has(held, Shoot))
            return Accept(20, None, Shoot, SamusPoseId.NormalJumpGunExtendedLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A3F6 TransitionTable_4F_FacingLeft_DamageBoost: native priority order.</summary>
    private static SamusPoseInputMatch MatchDamageBoostLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(0, None, Jump | Left, SamusPoseId.NormalJumpForwardLeftPose);
        if (Has(held, Jump | Right))
            return Accept(1, None, Jump | Right, SamusPoseId.DamageBoostLeftPose);
        if (Has(held, Jump))
            return Accept(2, None, Jump, SamusPoseId.NeutralJumpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A40A TransitionTable_50_FacingRight_DamageBoost: native priority order.</summary>
    private static SamusPoseInputMatch MatchDamageBoostRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(0, None, Jump | Left, SamusPoseId.DamageBoostRightPose);
        if (Has(held, Jump | Right))
            return Accept(1, None, Jump | Right, SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Jump))
            return Accept(2, None, Jump, SamusPoseId.NeutralJumpRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A41E TransitionTable_19_FacingRight_SpinJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpinJumpRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(0, Shoot, None, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(newlyPressed, Shoot) && Has(held, Right))
            return Accept(1, Shoot, Right, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Shoot | Up))
            return Accept(2, None, Shoot | Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Shoot | Down))
            return Accept(3, None, Shoot | Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Shoot))
            return Accept(4, None, AimUp | Shoot, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Shoot))
            return Accept(5, None, AimDown | Shoot, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(6, None, Jump | Right, SamusPoseId.SpinJumpRightPose);
        if (Has(held, Up))
            return Accept(7, None, Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(8, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(9, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Down))
            return Accept(10, None, Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, Right))
            return Accept(11, None, Right, SamusPoseId.SpinJumpRightPose);
        if (Has(held, Left))
            return Accept(12, None, Left, SamusPoseId.SpinJumpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A46E TransitionTable_1A_FacingLeft_SpinJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpinJumpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(0, Shoot, None, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(newlyPressed, Shoot) && Has(held, Left))
            return Accept(1, Shoot, Left, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Shoot | Up))
            return Accept(2, None, Shoot | Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Shoot | Down))
            return Accept(3, None, Shoot | Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp | Shoot))
            return Accept(4, None, AimUp | Shoot, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Shoot))
            return Accept(5, None, AimDown | Shoot, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Left))
            return Accept(6, None, Jump | Left, SamusPoseId.SpinJumpLeftPose);
        if (Has(held, Up))
            return Accept(7, None, Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(8, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(9, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Down))
            return Accept(10, None, Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, Left))
            return Accept(11, None, Left, SamusPoseId.SpinJumpLeftPose);
        if (Has(held, Right))
            return Accept(12, None, Right, SamusPoseId.SpinJumpRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A4BE TransitionTable_1B_FacingRight_SpaceJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpaceJumpRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(0, Shoot, None, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(newlyPressed, Shoot) && Has(held, Right))
            return Accept(1, Shoot, Right, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Shoot | Up))
            return Accept(2, None, Shoot | Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Shoot | Down))
            return Accept(3, None, Shoot | Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Shoot))
            return Accept(4, None, AimUp | Shoot, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Shoot))
            return Accept(5, None, AimDown | Shoot, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(6, None, Jump | Right, SamusPoseId.SpaceJumpRightPose);
        if (Has(held, Up))
            return Accept(7, None, Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(8, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(9, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Down))
            return Accept(10, None, Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, Right))
            return Accept(11, None, Right, SamusPoseId.SpaceJumpRightPose);
        if (Has(held, Left))
            return Accept(12, None, Left, SamusPoseId.SpaceJumpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A50E TransitionTable_1C_FacingLeft_SpaceJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpaceJumpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(0, Shoot, None, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(newlyPressed, Shoot) && Has(held, Left))
            return Accept(1, Shoot, Left, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Shoot | Up))
            return Accept(2, None, Shoot | Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Shoot | Down))
            return Accept(3, None, Shoot | Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp | Shoot))
            return Accept(4, None, AimUp | Shoot, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Shoot))
            return Accept(5, None, AimDown | Shoot, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Left))
            return Accept(6, None, Jump | Left, SamusPoseId.SpaceJumpLeftPose);
        if (Has(held, Up))
            return Accept(7, None, Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(8, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(9, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Down))
            return Accept(10, None, Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, Left))
            return Accept(11, None, Left, SamusPoseId.SpaceJumpLeftPose);
        if (Has(held, Right))
            return Accept(12, None, Right, SamusPoseId.SpaceJumpRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A55E TransitionTable_81_ScrewAttack: native priority order.</summary>
    private static SamusPoseInputMatch MatchScrewAttackRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(0, Shoot, None, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(newlyPressed, Shoot) && Has(held, Right))
            return Accept(1, Shoot, Right, SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Shoot | Up))
            return Accept(2, None, Shoot | Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, Shoot | Down))
            return Accept(3, None, Shoot | Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, AimUp | Shoot))
            return Accept(4, None, AimUp | Shoot, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown | Shoot))
            return Accept(5, None, AimDown | Shoot, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Jump | Right))
            return Accept(6, None, Jump | Right, SamusPoseId.ScrewAttackRightPose);
        if (Has(held, Up))
            return Accept(7, None, Up, SamusPoseId.NormalJumpAimUpRightPose);
        if (Has(held, AimUp))
            return Accept(8, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(9, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Down))
            return Accept(10, None, Down, SamusPoseId.NormalJumpAimDownRightPose);
        if (Has(held, Right))
            return Accept(11, None, Right, SamusPoseId.ScrewAttackRightPose);
        if (Has(held, Left))
            return Accept(12, None, Left, SamusPoseId.ScrewAttackLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A5AE TransitionTable_82_FacingLeft_ScrewAttack: native priority order.</summary>
    private static SamusPoseInputMatch MatchScrewAttackLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(0, Shoot, None, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(newlyPressed, Shoot) && Has(held, Left))
            return Accept(1, Shoot, Left, SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Shoot | Up))
            return Accept(2, None, Shoot | Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Shoot | Down))
            return Accept(3, None, Shoot | Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp | Shoot))
            return Accept(4, None, AimUp | Shoot, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Shoot))
            return Accept(5, None, AimDown | Shoot, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Left))
            return Accept(6, None, Jump | Left, SamusPoseId.ScrewAttackLeftPose);
        if (Has(held, Up))
            return Accept(7, None, Up, SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(8, None, AimUp, SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(9, None, AimDown, SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Down))
            return Accept(10, None, Down, SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, Left))
            return Accept(11, None, Left, SamusPoseId.ScrewAttackLeftPose);
        if (Has(held, Right))
            return Accept(12, None, Right, SamusPoseId.ScrewAttackRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A5FE TransitionTable_1D_FaceRight_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallGroundRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.MorphBallMovingRightPose);
        if (Has(held, Left))
            return Accept(3, None, Left, SamusPoseId.MorphBallMovingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A618 TransitionTable_1E_MoveRight_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallMovingRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.MorphBallMovingRightPose);
        if (Has(held, Left))
            return Accept(3, None, Left, SamusPoseId.MorphBallMovingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A632 TransitionTable_1F_MoveLeft_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallMovingLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.MorphBallMovingRightPose);
        if (Has(held, Left))
            return Accept(3, None, Left, SamusPoseId.MorphBallMovingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A64C TransitionTable_1D_FaceLeft_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallGroundLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.MorphBallMovingRightPose);
        if (Has(held, Left))
            return Accept(3, None, Left, SamusPoseId.MorphBallMovingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A666 TransitionTable list for pose $20: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose20List(ushort held, ushort newlyPressed)
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A668 TransitionTable list for pose $23: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose23List(ushort held, ushort newlyPressed)
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A66A TransitionTable list for pose $42: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose42List(ushort held, ushort newlyPressed)
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A66C TransitionTable_27_71_73_85_FacingRight_Crouching: native priority order.</summary>
    private static SamusPoseInputMatch MatchCrouchingRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up) && Has(held, AimUp | AimDown))
            return Accept(0, Up, AimUp | AimDown, SamusPoseId.StandingTransitionAimUpRightPose);
        if (Has(newlyPressed, Up) && Has(held, AimUp))
            return Accept(1, Up, AimUp, SamusPoseId.StandingTransitionAimDiagonalUpRightPose);
        if (Has(newlyPressed, Up) && Has(held, AimDown))
            return Accept(2, Up, AimDown, SamusPoseId.StandingTransitionAimDiagonalDownRightPose);
        if (Has(newlyPressed, Up))
            return Accept(3, Up, None, SamusPoseId.StandingTransitionRightPose);
        if (Has(newlyPressed, Left))
            return Accept(4, Left, None, SamusPoseId.TurningRightToLeftCrouchingPose);
        if (Has(newlyPressed, Down))
            return Accept(5, Down, None, SamusPoseId.MorphingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(6, Jump, None, SamusPoseId.NeutralJumpTransitionRightPose);
        if (Has(held, AimUp | AimDown))
            return Accept(7, None, AimUp | AimDown, SamusPoseId.CrouchingAimUpRightPose);
        if (Has(held, AimUp | Right))
            return Accept(8, None, AimUp | Right, SamusPoseId.FacingRightNormalPose);
        if (Has(held, AimDown | Right))
            return Accept(9, None, AimDown | Right, SamusPoseId.FacingRightNormalPose);
        if (Has(held, AimUp))
            return Accept(10, None, AimUp, SamusPoseId.CrouchingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(11, None, AimDown, SamusPoseId.CrouchingAimDiagonalDownRightPose);
        if (Has(held, Right))
            return Accept(12, None, Right, SamusPoseId.FacingRightNormalPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A6BC TransitionTable_28_72_74_86_Crouching: native priority order.</summary>
    private static SamusPoseInputMatch MatchCrouchingLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up) && Has(held, AimUp | AimDown))
            return Accept(0, Up, AimUp | AimDown, SamusPoseId.StandingTransitionAimUpLeftPose);
        if (Has(newlyPressed, Up) && Has(held, AimUp))
            return Accept(1, Up, AimUp, SamusPoseId.StandingTransitionAimDiagonalUpLeftPose);
        if (Has(newlyPressed, Up) && Has(held, AimDown))
            return Accept(2, Up, AimDown, SamusPoseId.StandingTransitionAimDiagonalDownLeftPose);
        if (Has(newlyPressed, Up))
            return Accept(3, Up, None, SamusPoseId.StandingTransitionLeftPose);
        if (Has(newlyPressed, Right))
            return Accept(4, Right, None, SamusPoseId.TurningLeftToRightCrouchingPose);
        if (Has(newlyPressed, Down))
            return Accept(5, Down, None, SamusPoseId.MorphingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(6, Jump, None, SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(held, AimUp | AimDown))
            return Accept(7, None, AimUp | AimDown, SamusPoseId.CrouchingAimUpLeftPose);
        if (Has(held, AimDown | Left))
            return Accept(8, None, AimDown | Left, SamusPoseId.FacingLeftNormalPose);
        if (Has(held, AimUp | Left))
            return Accept(9, None, AimUp | Left, SamusPoseId.FacingLeftNormalPose);
        if (Has(held, AimUp))
            return Accept(10, None, AimUp, SamusPoseId.CrouchingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(11, None, AimDown, SamusPoseId.CrouchingAimDiagonalDownLeftPose);
        if (Has(held, Left))
            return Accept(12, None, Left, SamusPoseId.FacingLeftNormalPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A70C TransitionTable_29_2B_6D_6F_FacingRight_Falling: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Right | Up))
            return Accept(0, None, Right | Up, SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, Right | Down))
            return Accept(1, None, Right | Down, SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Left | Up))
            return Accept(2, None, Left | Up, SamusPoseId.TurningRightToLeftFallingPose);
        if (Has(held, Left | Down))
            return Accept(3, None, Left | Down, SamusPoseId.TurningRightToLeftFallingPose);
        if (Has(held, Left))
            return Accept(4, None, Left, SamusPoseId.TurningRightToLeftFallingPose);
        if (Has(held, Up))
            return Accept(5, None, Up, SamusPoseId.FallingAimUpRightPose);
        if (Has(held, Down))
            return Accept(6, None, Down, SamusPoseId.FallingAimDownRightPose);
        if (Has(held, AimUp))
            return Accept(7, None, AimUp, SamusPoseId.FallingAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(8, None, AimDown, SamusPoseId.FallingAimDiagonalDownRightPose);
        if (Has(held, Shoot))
            return Accept(9, None, Shoot, SamusPoseId.FallingGunExtendedRightPose);
        if (Has(held, Right))
            return Accept(10, None, Right, SamusPoseId.FallingRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A750 TransitionTable_2A_2C_6E_70_FacingLeft_Falling: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Left | Up))
            return Accept(0, None, Left | Up, SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(1, None, Left | Down, SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Right | Up))
            return Accept(2, None, Right | Up, SamusPoseId.TurningLeftToRightFallingPose);
        if (Has(held, Right | Down))
            return Accept(3, None, Right | Down, SamusPoseId.TurningLeftToRightFallingPose);
        if (Has(held, Right))
            return Accept(4, None, Right, SamusPoseId.TurningLeftToRightFallingPose);
        if (Has(held, Up))
            return Accept(5, None, Up, SamusPoseId.FallingAimUpLeftPose);
        if (Has(held, Down))
            return Accept(6, None, Down, SamusPoseId.FallingAimDownLeftPose);
        if (Has(held, AimUp))
            return Accept(7, None, AimUp, SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(8, None, AimDown, SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Shoot))
            return Accept(9, None, Shoot, SamusPoseId.FallingGunExtendedLeftPose);
        if (Has(held, Left))
            return Accept(10, None, Left, SamusPoseId.FallingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A794 TransitionTable_31_FacingRight_MorphBall_NoSpringBall_InAir: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallFallingRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.UnmorphingTransitionRightPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.MorphBallFallingRightPose);
        if (Has(held, Left))
            return Accept(3, None, Left, SamusPoseId.MorphBallFallingLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A7AE TransitionTable_32_FacingLeft_MorphBall_NoSpringBall_InAir: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallFallingLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(0, Up, None, SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.MorphBallFallingLeftPose);
        if (Has(held, Right))
            return Accept(3, None, Right, SamusPoseId.MorphBallFallingRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A7C8 TransitionTable list for pose $33: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedKnockbackRightPoseList(ushort held, ushort newlyPressed)
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A7CA TransitionTable list for pose $34: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedKnockbackLeftPoseList(ushort held, ushort newlyPressed)
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A7CC UNUSED_TransitionTable_45_91A7CC: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose45List(ushort held, ushort newlyPressed)
    {
        if (Has(held, Shoot | Left))
            return Accept(0, None, Shoot | Left, UnusedPose45);
        if (Has(held, Right))
            return Accept(1, None, Right, SamusPoseId.MovingRightNormalPose);
        if (Has(held, Left))
            return Accept(2, None, Left, SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A7E0 UNUSED_TransitionTable_46_91A7E0: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose46List(ushort held, ushort newlyPressed)
    {
        if (Has(held, Shoot | Right))
            return Accept(0, None, Shoot | Right, UnusedPose46);
        if (Has(held, Left))
            return Accept(1, None, Left, SamusPoseId.MovingLeftNormalPose);
        if (Has(held, Right))
            return Accept(2, None, Right, SamusPoseId.TurningLeftToRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A7F4 TransitionTable list for pose $47: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose47List(ushort held, ushort newlyPressed)
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A834 TransitionTable list for pose $48: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose48List(ushort held, ushort newlyPressed)
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A874 TransitionTable_49_75_77_FacingLeft_Moonwalk: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkFacingLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(0, Down, None, SamusPoseId.CrouchingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.MoonwalkTurnJumpRightPose);
        if (Has(newlyPressed, Jump) && Has(held, AimUp))
            return Accept(2, Jump, AimUp, SamusPoseId.MoonwalkTurnJumpAimUpRightPose);
        if (Has(newlyPressed, Jump) && Has(held, AimDown))
            return Accept(3, Jump, AimDown, SamusPoseId.MoonwalkTurnJumpAimDownRightPose);
        if (Has(held, AimDown | Shoot | Right))
            return Accept(4, None, AimDown | Shoot | Right, SamusPoseId.MoonwalkAimDownLeftPose);
        if (Has(held, AimUp | Shoot | Right))
            return Accept(5, None, AimUp | Shoot | Right, SamusPoseId.MoonwalkAimUpLeftPose);
        if (Has(held, Shoot | Right))
            return Accept(6, None, Shoot | Right, SamusPoseId.MoonwalkFacingLeftPose);
        if (Has(held, Left))
            return Accept(7, None, Left, SamusPoseId.MovingLeftNormalPose);
        if (Has(held, Right))
            return Accept(8, None, Right, SamusPoseId.TurningLeftToRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A8AC TransitionTable_4A_76_78_FacingRight_Moonwalk: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkFacingRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(0, Down, None, SamusPoseId.CrouchingTransitionRightPose);
        if (Has(newlyPressed, Jump))
            return Accept(1, Jump, None, SamusPoseId.MoonwalkTurnJumpLeftPose);
        if (Has(newlyPressed, Jump) && Has(held, AimUp))
            return Accept(2, Jump, AimUp, SamusPoseId.MoonwalkTurnJumpAimUpLeftPose);
        if (Has(newlyPressed, Jump) && Has(held, AimDown))
            return Accept(3, Jump, AimDown, SamusPoseId.MoonwalkTurnJumpAimDownLeftPose);
        if (Has(held, AimUp | Shoot | Left))
            return Accept(4, None, AimUp | Shoot | Left, SamusPoseId.MoonwalkAimUpRightPose);
        if (Has(held, AimDown | Shoot | Left))
            return Accept(5, None, AimDown | Shoot | Left, SamusPoseId.MoonwalkAimDownRightPose);
        if (Has(held, Shoot | Left))
            return Accept(6, None, Shoot | Left, SamusPoseId.MoonwalkFacingRightPose);
        if (Has(held, Right))
            return Accept(7, None, Right, SamusPoseId.MovingRightNormalPose);
        if (Has(held, Left))
            return Accept(8, None, Left, SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A8E4 TransitionTable_53_FacingRight_Knockback: native priority order.</summary>
    private static SamusPoseInputMatch MatchKnockbackRightPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(0, None, Jump | Left, SamusPoseId.DamageBoostRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A8EC TransitionTable_54_FacingLeft_Knockback: native priority order.</summary>
    private static SamusPoseInputMatch MatchKnockbackLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(0, None, Jump | Right, SamusPoseId.DamageBoostLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A8FC UNUSED_TransitionTable_5B_91A8FC: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose5BList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(0, None, Jump | Left, SamusPoseId.UnusedPose66);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A904 UNUSED_TransitionTable_5C_91A904: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose5CList(ushort held, ushort newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(0, None, Jump | Right, SamusPoseId.UnusedPose65);
        return new(null, HasConditions: true);
    }
}
