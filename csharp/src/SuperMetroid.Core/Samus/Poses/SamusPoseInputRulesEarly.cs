using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Samus.Poses.CanonicalPoseButtons;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputs;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputList;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Ordered native pose-input decisions; the first satisfied chord wins.</summary>
internal static class SamusPoseInputRulesEarly
{
    /// <summary>Selects the first native input chord in an early bank-$91 transition list and rejects lists owned by the other rule section.</summary>
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
        UnusedPose20List => MatchUnusedPose20List(),
        UnusedPose23List => MatchUnusedPose23List(),
        UnusedPose42List => MatchUnusedPose42List(),
        CrouchingRightPoseList => MatchCrouchingRightPoseList(held, newlyPressed),
        CrouchingLeftPoseList => MatchCrouchingLeftPoseList(held, newlyPressed),
        FallingRightPoseList => MatchFallingRightPoseList(held),
        FallingLeftPoseList => MatchFallingRightPoseList(MirrorButtons(held)).Mirrored(),
        MorphBallFallingRightPoseList => MatchMorphBallFallingRightPoseList(held, newlyPressed),
        MorphBallFallingLeftPoseList => MatchMorphBallFallingRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        UnusedKnockbackRightPoseList => MatchUnusedKnockbackRightPoseList(),
        UnusedKnockbackLeftPoseList => MatchUnusedKnockbackRightPoseList().Mirrored(),
        UnusedPose45List => MatchUnusedPose45List(held),
        UnusedPose46List => MatchUnusedPose46List(held),
        UnusedPose47List => MatchUnusedPose47List(),
        UnusedPose48List => MatchUnusedPose48List(),
        MoonwalkFacingLeftPoseList => MatchMoonwalkFacingLeftPoseList(held, newlyPressed),
        MoonwalkFacingRightPoseList => MatchMoonwalkFacingRightPoseList(held, newlyPressed),
        KnockbackRightPoseList => MatchKnockbackRightPoseList(held),
        KnockbackLeftPoseList => MatchKnockbackRightPoseList(MirrorButtons(held)).Mirrored(),
        UnusedPose5BList => MatchUnusedPose5BList(held),
        UnusedPose5CList => MatchUnusedPose5CList(held),
        SamusPoseInputList.SpringBallGroundRightPoseList or
            SamusPoseInputList.SpringBallGroundLeftPoseList or
            SamusPoseInputList.SpringBallFallingRightPoseList or
            SamusPoseInputList.SpringBallFallingLeftPoseList or
            SamusPoseInputList.SpringBallJumpRightPoseList or
            SamusPoseInputList.SpringBallJumpLeftPoseList or
            SamusPoseInputList.UnusedPose63List or
            SamusPoseInputList.UnusedPose64List or
            SamusPoseInputList.UnusedPose65List or
            SamusPoseInputList.UnusedPose66List or
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
            SamusPoseInputList.UnusedPoseDfList or
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

    /// <summary>$91:A0DC TransitionTable list for pose $2F: native priority order.</summary>
    private static SamusPoseInputMatch MatchEmptyTransitionList()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>$91:A0DE TransitionTable_00_9B_FacingForward: native priority order.</summary>
    private static SamusPoseInputMatch MatchForwardFacingPowerSuitPoseList(ControllerButton held)
    {
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A0EC TransitionTable_01_03_05_07_A4_A6_E0_E2_E4_E6_FacingRight: native priority order; $91:A172 TransitionTable_02_04_06_08_A5_A7_E1_E3_E5_E7_FacingLeft is the same list mirrored.</summary>
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


    /// <summary>$91:A1F8 TransitionTable_09_0D_0F_11_MovingRight: native priority order; $91:A242 TransitionTable_0A_0E_10_12_MovingLeft is the same list mirrored.</summary>
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


    /// <summary>$91:A28C TransitionTable_4B_55_57_59_FacingRight_NormalJumpTransition: native priority order.</summary>
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

    /// <summary>$91:A2BE TransitionTable_4C_56_58_5A_FacingLeft_NormalJumpTransition: native priority order.</summary>
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

    /// <summary>$91:A2F6 TransitionTable_15_4D_51_69_6B_FacingRight_NormalJump: native priority order; $91:A376 TransitionTable_16_4E_52_6A_6C_FacingLeft_NormalJump is the same list mirrored.</summary>
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


    /// <summary>$91:A3F6 TransitionTable_4F_FacingLeft_DamageBoost: native priority order.</summary>
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

    /// <summary>$91:A40A TransitionTable_50_FacingRight_DamageBoost: native priority order.</summary>
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

    /// <summary>$91:A41E TransitionTable_19_FacingRight_SpinJump: native priority order; $91:A46E TransitionTable_1A_FacingLeft_SpinJump is the same list mirrored.</summary>
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


    /// <summary>$91:A4BE TransitionTable_1B_FacingRight_SpaceJump: native priority order; $91:A50E TransitionTable_1C_FacingLeft_SpaceJump is the same list mirrored.</summary>
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


    /// <summary>$91:A55E TransitionTable_81_ScrewAttack: native priority order; $91:A5AE TransitionTable_82_FacingLeft_ScrewAttack is the same list mirrored.</summary>
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


    /// <summary>$91:A5FE TransitionTable_1D_FaceRight_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
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

    /// <summary>$91:A64C TransitionTable_1D_FaceLeft_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
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

    /// <summary>$91:A618 TransitionTable_1E_MoveRight_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
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

    /// <summary>$91:A632 TransitionTable_1F_MoveLeft_MorphBall_NoSpringBall_OnGround: native priority order.</summary>
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

    /// <summary>$91:A6BC TransitionTable_28_72_74_86_Crouching: native priority order.</summary>
    [MirroredAsymmetry("the native left-facing crouch list exchanges the diagonal aim-up and aim-down chords as well as the directions")]
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

    /// <summary>$91:A70C TransitionTable_29_2B_6D_6F_FacingRight_Falling: native priority order; $91:A750 TransitionTable_2A_2C_6E_70_FacingLeft_Falling is the same list mirrored.</summary>
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


    /// <summary>$91:A794 TransitionTable_31_FacingRight_MorphBall_NoSpringBall_InAir: native priority order; $91:A7AE TransitionTable_32_FacingLeft_MorphBall_NoSpringBall_InAir is the same list mirrored.</summary>
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


    /// <summary>$91:A7C8 TransitionTable list for pose $33: native priority order; $91:A7CA TransitionTable list for pose $34 is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchUnusedKnockbackRightPoseList()
    {
        return new(null, HasConditions: false);
    }


    /// <summary>$91:A7CC UNUSED_TransitionTable_45_91A7CC: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose45List(ControllerButton held)
    {
        if (Has(held, Shoot | Left))
            return Accept(SamusPoseId.UnusedPose45);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightNormalPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A7E0 UNUSED_TransitionTable_46_91A7E0: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose46List(ControllerButton held)
    {
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.UnusedPose46);
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

    /// <summary>$91:A8AC TransitionTable_4A_76_78_FacingRight_Moonwalk: native priority order.</summary>
    [MirroredAsymmetry("the native right-facing moonwalk list exchanges the diagonal aim-up and aim-down chords and aimed poses as well as the directions")]
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

    /// <summary>$91:A8E4 TransitionTable_53_FacingRight_Knockback: native priority order; $91:A8EC TransitionTable_54_FacingLeft_Knockback is the same list mirrored.</summary>
    private static SamusPoseInputMatch MatchKnockbackRightPoseList(ControllerButton held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.DamageBoostRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>$91:A8FC UNUSED_TransitionTable_5B_91A8FC: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose5BList(ControllerButton held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.UnusedPose66);
        return new(null, HasConditions: true);
    }

    /// <summary>$91:A904 UNUSED_TransitionTable_5C_91A904: native priority order.</summary>
    private static SamusPoseInputMatch MatchUnusedPose5CList(ControllerButton held)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.UnusedPose65);
        return new(null, HasConditions: true);
    }
}
