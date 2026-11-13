using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Samus.Poses.CanonicalPoseButtons;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputs;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputList;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Ordered pose-input decisions; the first satisfied chord wins.</summary>
internal static class SamusPoseInputRulesLate
{
    /// <summary>Dispatches the later pose tables in game order, separating held chords from new presses.</summary>
    internal static SamusPoseInputMatch Match(SamusPoseInputList inputList, ControllerButton held, ControllerButton newlyPressed) => inputList switch
    {
        SpringBallGroundRightPoseList => MatchSpringBallGroundRightPoseList(held, newlyPressed),
        SpringBallGroundLeftPoseList => MatchSpringBallGroundLeftPoseList(held, newlyPressed),
        SpringBallFallingRightPoseList => MatchSpringBallFallingRightPoseList(held, newlyPressed),
        SpringBallFallingLeftPoseList => MatchSpringBallFallingRightPoseList(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        SpringBallJumpRightPoseList => MatchSpringBallJumpRightPoseList(held, newlyPressed),
        SpringBallJumpLeftPoseList => MatchSpringBallJumpLeftPoseList(held, newlyPressed),
        CompactAirborneRightPoseList => MatchCompactAirborneRightPoseList(held),
        CompactAirborneLeftPoseList => MatchCompactAirborneRightPoseList(MirrorButtons(held)).Mirrored(),
        CompactAirborneFallingRightPoseList => MatchCompactAirborneFallingRightPoseList(held),
        CompactAirborneFallingLeftPoseList => MatchCompactAirborneFallingRightPoseList(MirrorButtons(held)).Mirrored(),
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
        DraygonHeldBallLeftPoseList => MatchDraygonHeldBallLeftPoseList(newlyPressed),
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
            SamusPoseInputList.GlitchBallRightPoseList or
            SamusPoseInputList.GlitchBallLeftPoseList or
            SamusPoseInputList.CrouchingRightPoseList or
            SamusPoseInputList.CrouchingLeftPoseList or
            SamusPoseInputList.FallingRightPoseList or
            SamusPoseInputList.FallingLeftPoseList or
            SamusPoseInputList.MorphBallFallingRightPoseList or
            SamusPoseInputList.MorphBallFallingLeftPoseList or
            SamusPoseInputList.GlitchBallKnockbackRightPoseList or
            SamusPoseInputList.GlitchBallKnockbackLeftPoseList or
            SamusPoseInputList.MoonwalkFacingLeftPoseList or
            SamusPoseInputList.MoonwalkFacingRightPoseList or
            SamusPoseInputList.KnockbackRightPoseList or
            SamusPoseInputList.KnockbackLeftPoseList => throw new InvalidOperationException($"Pose-input list {inputList} belongs to the other rule section."),
        _ => throw new InvalidOperationException($"Pose-input list {inputList} belongs to the other rule section."),
    };

    /// <summary>The priority order of the spring ball ground right input list.</summary>
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

    /// <summary>The priority order of the spring ball ground left input list.</summary>
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


    /// <summary>The priority order of the spring ball falling right input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the spring ball jump right input list.</summary>
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

    /// <summary>The priority order of the spring ball jump left input list.</summary>
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


    /// <summary>The priority order of the unused compact airborne right input list.</summary>
    private static SamusPoseInputMatch MatchCompactAirborneRightPoseList(ControllerButton held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.CompactAirborneFallingLeftPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The priority order of the unused compact airborne falling right input list.</summary>
    private static SamusPoseInputMatch MatchCompactAirborneFallingRightPoseList(ControllerButton held)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.CompactAirborneFallingRightPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.CompactAirborneFallingRightPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.CompactAirborneFallingRightPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The priority order of the wall jump right input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the ran into wall right input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the normal jump gun extended right input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the normal jump aim down right input list.</summary>
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

    /// <summary>The priority order of the normal jump aim down left input list.</summary>
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

    /// <summary>The priority order of the unmorphing transition right input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the turning right to left input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the turning right to left aim up input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the turning right to left aim diagonal down input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the shinespark windup right input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the falling aim down right input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the unused draygon held ball left input list.</summary>
    private static SamusPoseInputMatch MatchDraygonHeldBallLeftPoseList(ControllerButton newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.DraygonHeldStandingUpLeftPose);
        return new(null, HasConditions: true);
    }


    /// <summary>The priority order of the draygon grabbed neutral right input list.</summary>
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

    /// <summary>The priority order of the draygon grabbed neutral left input list.</summary>
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

    /// <summary>The priority order of the moving right gun extended input list; the mirrored list has the same order mirrored.</summary>
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


    /// <summary>The priority order of the falling gun extended right input list; the mirrored list has the same order mirrored.</summary>
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



    /// <summary>The priority order of the moonwalk turn jump right input list; the mirrored list has the same order mirrored.</summary>
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
    /// The aimed-up and aimed-down moonwalk turn-jump input lists, in both directions: one priority order for an aimed
    /// moonwalk turn jump to the right, continuing <paramref name="aimedTurnJump"/>; the leftward lists are it mirrored.
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
