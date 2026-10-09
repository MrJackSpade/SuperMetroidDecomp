using static SuperMetroid.Core.Game.CanonicalPoseButtons;
using static SuperMetroid.Core.Game.SamusPoseInputDefinitions;

namespace SuperMetroid.Core.Game;

/// <summary>Ordered native pose-input decisions; the first satisfied chord wins.</summary>
internal static class SamusPoseInputRulesEarly
{
    internal static SamusPoseInputMatch Match(ushort pointer, ushort held, ushort newlyPressed) => pointer switch
    {
        EmptyTransitionList => MatchEmptyTransitionList(),
        ForwardFacingPowerSuitPoseList => MatchForwardFacingPowerSuitPoseList(held),
        FacingRightNormalPoseList => MatchFacingRightNormalPoseList(held, newlyPressed),
        FacingLeftNormalPoseList => MatchFacingLeftNormalPoseList(held, newlyPressed),
        MovingRightNormalPoseList => MatchMovingRightNormalPoseList(held, newlyPressed),
        MovingLeftNormalPoseList => MatchMovingLeftNormalPoseList(held, newlyPressed),
        NeutralJumpTransitionRightPoseList => MatchNeutralJumpTransitionRightPoseList(held),
        NeutralJumpTransitionLeftPoseList => MatchNeutralJumpTransitionLeftPoseList(held),
        NormalJumpAimUpRightPoseList => MatchNormalJumpAimUpRightPoseList(held),
        NormalJumpAimUpLeftPoseList => MatchNormalJumpAimUpLeftPoseList(held),
        DamageBoostLeftPoseList => MatchDamageBoostLeftPoseList(held),
        DamageBoostRightPoseList => MatchDamageBoostRightPoseList(held),
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
        UnusedPose20List => MatchUnusedPose20List(),
        UnusedPose23List => MatchUnusedPose23List(),
        UnusedPose42List => MatchUnusedPose42List(),
        CrouchingRightPoseList => MatchCrouchingRightPoseList(held, newlyPressed),
        CrouchingLeftPoseList => MatchCrouchingLeftPoseList(held, newlyPressed),
        FallingRightPoseList => MatchFallingRightPoseList(held),
        FallingLeftPoseList => MatchFallingLeftPoseList(held),
        MorphBallFallingRightPoseList => MatchMorphBallFallingRightPoseList(held, newlyPressed),
        MorphBallFallingLeftPoseList => MatchMorphBallFallingLeftPoseList(held, newlyPressed),
        UnusedKnockbackRightPoseList => MatchUnusedKnockbackRightPoseList(),
        UnusedKnockbackLeftPoseList => MatchUnusedKnockbackLeftPoseList(),
        UnusedPose45List => MatchUnusedPose45List(held),
        UnusedPose46List => MatchUnusedPose46List(held),
        UnusedPose47List => MatchUnusedPose47List(),
        UnusedPose48List => MatchUnusedPose48List(),
        MoonwalkFacingLeftPoseList => MatchMoonwalkFacingLeftPoseList(held, newlyPressed),
        MoonwalkFacingRightPoseList => MatchMoonwalkFacingRightPoseList(held, newlyPressed),
        KnockbackRightPoseList => MatchKnockbackRightPoseList(held),
        KnockbackLeftPoseList => MatchKnockbackLeftPoseList(held),
        UnusedPose5BList => MatchUnusedPose5BList(held),
        UnusedPose5CList => MatchUnusedPose5CList(held),
        _ => throw new InvalidOperationException($"Unknown compiled pose-input list ${pointer:X4}."),
    };

    /// <summary>$91:A0DC TransitionTable list for pose $2F: native priority order.</summary>
    private static SamusPoseInputMatch MatchEmptyTransitionList()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A0DE TransitionTable_00_9B_FacingForward: native priority order.</summary>
    private static SamusPoseInputMatch MatchForwardFacingPowerSuitPoseList(ushort held)
    {
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A0EC TransitionTable_01_03_05_07_A4_A6_E0_E2_E4_E6_FacingRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchFacingRightNormalPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A172 TransitionTable_02_04_06_08_A5_A7_E1_E3_E5_E7_FacingLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchFacingLeftNormalPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Up))
            return Accept(SamusPoseId.NormalJumpTransitionAimUpLeftPose);
        if (Has(newlyPressed, Jump) && Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose);
        if (Has(newlyPressed, Jump) && Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose);
        if (Has(newlyPressed, Down) && Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.CrouchingTransitionAimUpLeftPose);
        if (Has(newlyPressed, Down) && Has(held, AimUp))
            return Accept(SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose);
        if (Has(newlyPressed, Down) && Has(held, AimDown))
            return Accept(SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose);
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionLeftPose);
        if (Has(held, AimDown | Shoot | Right))
            return Accept(SamusPoseId.MoonwalkAimDownLeftPose);
        if (Has(held, AimUp | Shoot | Right))
            return Accept(SamusPoseId.MoonwalkAimUpLeftPose);
        if (Has(held, AimUp | AimDown | Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        if (Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.StandingAimUpLeftPose);
        if (Has(held, AimUp | Left))
            return Accept(SamusPoseId.RunningAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Left))
            return Accept(SamusPoseId.RunningAimDiagonalDownLeftPose);
        if (Has(held, Left | Up))
            return Accept(SamusPoseId.RunningAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(SamusPoseId.RunningAimDiagonalDownLeftPose);
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.MoonwalkFacingLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MovingLeftNormalPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A1F8 TransitionTable_09_0D_0F_11_MovingRight: native priority order.</summary>
    private static SamusPoseInputMatch MatchMovingRightNormalPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A242 TransitionTable_0A_0E_10_12_MovingLeft: native priority order.</summary>
    private static SamusPoseInputMatch MatchMovingLeftNormalPoseList(ushort held, ushort newlyPressed)
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
            return Accept(SamusPoseId.MovingLeftNormalPose);
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

    /// <summary>$91:A28C TransitionTable_4B_55_57_59_FacingRight_NormalJumpTransition: native priority order.</summary>
    private static SamusPoseInputMatch MatchNeutralJumpTransitionRightPoseList(ushort held)
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

    /// <summary>$91:A2BE TransitionTable_4C_56_58_5A_FacingLeft_NormalJumpTransition: native priority order.</summary>
    private static SamusPoseInputMatch MatchNeutralJumpTransitionLeftPoseList(ushort held)
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

    /// <summary>$91:A2F6 TransitionTable_15_4D_51_69_6B_FacingRight_NormalJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpAimUpRightPoseList(ushort held)
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

    /// <summary>$91:A376 TransitionTable_16_4E_52_6A_6C_FacingLeft_NormalJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchNormalJumpAimUpLeftPoseList(ushort held)
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
        if (Has(held, Jump))
            return Accept(SamusPoseId.NeutralJumpLeftPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A3F6 TransitionTable_4F_FacingLeft_DamageBoost: native priority order.</summary>
    private static SamusPoseInputMatch MatchDamageBoostLeftPoseList(ushort held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.NormalJumpForwardLeftPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.DamageBoostLeftPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NeutralJumpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A40A TransitionTable_50_FacingRight_DamageBoost: native priority order.</summary>
    private static SamusPoseInputMatch MatchDamageBoostRightPoseList(ushort held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.DamageBoostRightPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NeutralJumpRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A41E TransitionTable_19_FacingRight_SpinJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpinJumpRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A46E TransitionTable_1A_FacingLeft_SpinJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpinJumpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(newlyPressed, Shoot) && Has(held, Left))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A4BE TransitionTable_1B_FacingRight_SpaceJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpaceJumpRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A50E TransitionTable_1C_FacingLeft_SpaceJump: native priority order.</summary>
    private static SamusPoseInputMatch MatchSpaceJumpLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(newlyPressed, Shoot) && Has(held, Left))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.SpaceJumpLeftPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpaceJumpLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpaceJumpRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A55E TransitionTable_81_ScrewAttack: native priority order.</summary>
    private static SamusPoseInputMatch MatchScrewAttackRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A5AE TransitionTable_82_FacingLeft_ScrewAttack: native priority order.</summary>
    private static SamusPoseInputMatch MatchScrewAttackLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(newlyPressed, Shoot) && Has(held, Left))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, AimUp | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.ScrewAttackLeftPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.ScrewAttackLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.ScrewAttackRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A5FE TransitionTable_1D_FaceRight_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallGroundRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A618 TransitionTable_1E_MoveRight_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallMovingRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A632 TransitionTable_1F_MoveLeft_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallMovingLeftPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A64C TransitionTable_1D_FaceLeft_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallGroundLeftPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A666 TransitionTable list for pose $20: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose20List()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A668 TransitionTable list for pose $23: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose23List()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A66A TransitionTable list for pose $42: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose42List()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A66C TransitionTable_27_71_73_85_FacingRight_Crouching: native priority order.</summary>
    private static SamusPoseInputMatch MatchCrouchingRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A6BC TransitionTable_28_72_74_86_Crouching: native priority order.</summary>
    private static SamusPoseInputMatch MatchCrouchingLeftPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A70C TransitionTable_29_2B_6D_6F_FacingRight_Falling: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingRightPoseList(ushort held)
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

    /// <summary>$91:A750 TransitionTable_2A_2C_6E_70_FacingLeft_Falling: native priority order.</summary>
    private static SamusPoseInputMatch MatchFallingLeftPoseList(ushort held)
    {
        if (Has(held, Left | Up))
            return Accept(SamusPoseId.FallingAimDiagonalUpLeftPose);
        if (Has(held, Left | Down))
            return Accept(SamusPoseId.FallingAimDiagonalDownLeftPose);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.TurningLeftToRightFallingPose);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.TurningLeftToRightFallingPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightFallingPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.FallingAimUpLeftPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.FallingAimDownLeftPose);
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

    /// <summary>$91:A794 TransitionTable_31_FacingRight_MorphBall_NoSpringBall_InAir: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallFallingRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A7AE TransitionTable_32_FacingLeft_MorphBall_NoSpringBall_InAir: native priority order.</summary>
    private static SamusPoseInputMatch MatchMorphBallFallingLeftPoseList(ushort held, ushort newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MorphBallFallingLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MorphBallFallingRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A7C8 TransitionTable list for pose $33: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedKnockbackRightPoseList()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A7CA TransitionTable list for pose $34: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedKnockbackLeftPoseList()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A7CC UNUSED_TransitionTable_45_91A7CC: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose45List(ushort held)
    {
        if (Has(held, Shoot | Left))
            return Accept(UnusedPose45);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightNormalPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A7E0 UNUSED_TransitionTable_46_91A7E0: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose46List(ushort held)
    {
        if (Has(held, Shoot | Right))
            return Accept(UnusedPose46);
        if (Has(held, Left))
            return Accept(SamusPoseId.MovingLeftNormalPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A7F4 TransitionTable list for pose $47: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose47List()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A834 TransitionTable list for pose $48: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose48List()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A874 TransitionTable_49_75_77_FacingLeft_Moonwalk: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkFacingLeftPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A8AC TransitionTable_4A_76_78_FacingRight_Moonwalk: native priority order.</summary>
    private static SamusPoseInputMatch MatchMoonwalkFacingRightPoseList(ushort held, ushort newlyPressed)
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

    /// <summary>$91:A8E4 TransitionTable_53_FacingRight_Knockback: native priority order.</summary>
    private static SamusPoseInputMatch MatchKnockbackRightPoseList(ushort held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.DamageBoostRightPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A8EC TransitionTable_54_FacingLeft_Knockback: native priority order.</summary>
    private static SamusPoseInputMatch MatchKnockbackLeftPoseList(ushort held)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.DamageBoostLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A8FC UNUSED_TransitionTable_5B_91A8FC: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose5BList(ushort held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.UnusedPose66);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A904 UNUSED_TransitionTable_5C_91A904: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose5CList(ushort held)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.UnusedPose65);
        return new(null, HasConditions: true);
    }
}
