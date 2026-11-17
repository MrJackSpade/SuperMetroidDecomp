using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Input.GameplayAction;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputs;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputList;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Ordered pose-input decisions; the first satisfied chord wins.</summary>
internal static class SamusPoseInputRules
{
    /// <summary>Selects the first satisfied input chord in <paramref name="inputList"/>, in the order the list authors them.</summary>
    internal static SamusPoseInputMatch Match(SamusPoseInputList inputList, GameplayAction held, GameplayAction newlyPressed) => inputList switch
    {
        NoInputTransitions => MatchNoInputTransitions(),
        ForwardFacing => MatchForwardFacing(held),
        StandingAndLandingRight => MatchStandingAndLandingRight(held, newlyPressed),
        StandingAndLandingLeft => MatchStandingAndLandingRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        RunningRight => MatchRunningRight(held, newlyPressed),
        RunningLeft => MatchRunningRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        JumpTransitionRight => MatchJumpTransitionRight(held),
        JumpTransitionLeft => MatchJumpTransitionLeft(held),
        NormalJumpRight => MatchNormalJumpRight(held),
        NormalJumpLeft => MatchNormalJumpRight(MirrorButtons(held)).Mirrored(),
        DamageBoostLeft => MatchDamageBoostLeft(held),
        DamageBoostRight => MatchDamageBoostRight(held),
        SpinJumpRight => MatchSpinJumpRight(held, newlyPressed),
        SpinJumpLeft => MatchSpinJumpRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        SpaceJumpRight => MatchSpaceJumpRight(held, newlyPressed),
        SpaceJumpLeft => MatchSpaceJumpRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        ScrewAttackRight => MatchScrewAttackRight(held, newlyPressed),
        ScrewAttackLeft => MatchScrewAttackRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        MorphBallGroundRight => MatchMorphBallGroundRight(held, newlyPressed),
        MorphBallMovingRight => MatchMorphBallMovingRight(held, newlyPressed),
        MorphBallMovingLeft => MatchMorphBallMovingLeft(held, newlyPressed),
        MorphBallGroundLeft => MatchMorphBallGroundLeft(held, newlyPressed),
        GlitchBallRight => MatchGlitchBallRight(),
        GlitchBallLeft => MatchGlitchBallRight().Mirrored(),
        CrouchingRight => MatchCrouchingRight(held, newlyPressed),
        CrouchingLeft => MatchCrouchingLeft(held, newlyPressed),
        FallingRight => MatchFallingRight(held),
        FallingLeft => MatchFallingRight(MirrorButtons(held)).Mirrored(),
        MorphBallFallingRight => MatchMorphBallFallingRight(held, newlyPressed),
        MorphBallFallingLeft => MatchMorphBallFallingRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        GlitchBallKnockbackRight => MatchGlitchBallKnockbackRight(),
        GlitchBallKnockbackLeft => MatchGlitchBallKnockbackRight().Mirrored(),
        MoonwalkLeft => MatchMoonwalkLeft(held, newlyPressed),
        MoonwalkRight => MatchMoonwalkRight(held, newlyPressed),
        KnockbackRight => MatchKnockbackRight(held),
        KnockbackLeft => MatchKnockbackRight(MirrorButtons(held)).Mirrored(),
        SpringBallGroundRight => MatchSpringBallGroundRight(held, newlyPressed),
        SpringBallGroundLeft => MatchSpringBallGroundLeft(held, newlyPressed),
        SpringBallFallingRight => MatchSpringBallFallingRight(held, newlyPressed),
        SpringBallFallingLeft => MatchSpringBallFallingRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        SpringBallJumpRight => MatchSpringBallJumpRight(held, newlyPressed),
        SpringBallJumpLeft => MatchSpringBallJumpLeft(held, newlyPressed),
        WallJumpRight => MatchWallJumpRight(held, newlyPressed),
        WallJumpLeft => MatchWallJumpRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        RanIntoWallRight => MatchRanIntoWallRight(held, newlyPressed),
        RanIntoWallLeft => MatchRanIntoWallRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        NormalJumpGunExtendedRight => MatchNormalJumpGunExtendedRight(held),
        NormalJumpGunExtendedLeft => MatchNormalJumpGunExtendedRight(MirrorButtons(held)).Mirrored(),
        NormalJumpAimDownRight => MatchNormalJumpAimDownRight(held, newlyPressed),
        NormalJumpAimDownLeft => MatchNormalJumpAimDownLeft(held, newlyPressed),
        UnmorphingTransitionRight => MatchUnmorphingTransitionRight(held),
        UnmorphingTransitionLeft => MatchUnmorphingTransitionRight(MirrorButtons(held)).Mirrored(),
        TurningRightToLeft => MatchTurningRightToLeft(held, newlyPressed),
        TurningLeftToRight => MatchTurningRightToLeft(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        TurningRightToLeftAimUp => MatchTurningRightToLeftAimUp(held, newlyPressed),
        TurningLeftToRightAimUp => MatchTurningRightToLeftAimUp(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        TurningRightToLeftAimDiagonalDown => MatchTurningRightToLeftAimDiagonalDown(held, newlyPressed),
        TurningLeftToRightAimDiagonalDown => MatchTurningRightToLeftAimDiagonalDown(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        ShinesparkWindupRight => MatchShinesparkWindupRight(held),
        ShinesparkWindupLeft => MatchShinesparkWindupRight(MirrorButtons(held)).Mirrored(),
        FallingAimDownRight => MatchFallingAimDownRight(held, newlyPressed),
        FallingAimDownLeft => MatchFallingAimDownRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        DraygonHeldBallLeft => MatchDraygonHeldBallLeft(newlyPressed),
        DraygonGrabbedLeft => MatchDraygonGrabbedLeft(held),
        DraygonGrabbedRight => MatchDraygonGrabbedRight(held),
        MovingRightGunExtended => MatchMovingRightGunExtended(held, newlyPressed),
        MovingLeftGunExtended => MatchMovingRightGunExtended(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        FallingGunExtendedRight => MatchFallingGunExtendedRight(held),
        FallingGunExtendedLeft => MatchFallingGunExtendedRight(MirrorButtons(held)).Mirrored(),
        MoonwalkTurnJumpLeft => MatchMoonwalkTurnJumpRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        MoonwalkTurnJumpRight => MatchMoonwalkTurnJumpRight(held, newlyPressed),
        MoonwalkTurnJumpAimUpLeft => MatchMoonwalkTurnJumpAimedRight(SamusPoseId.MoonwalkTurnJumpAimUpRightPose,
            MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        MoonwalkTurnJumpAimUpRight => MatchMoonwalkTurnJumpAimedRight(SamusPoseId.MoonwalkTurnJumpAimUpRightPose, held, newlyPressed),
        MoonwalkTurnJumpAimDownLeft => MatchMoonwalkTurnJumpAimedRight(SamusPoseId.MoonwalkTurnJumpAimDownRightPose,
            MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored(),
        MoonwalkTurnJumpAimDownRight => MatchMoonwalkTurnJumpAimedRight(SamusPoseId.MoonwalkTurnJumpAimDownRightPose, held, newlyPressed),
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusPoseInputList)} {inputList}."),
    };

    /// <summary>The list with no transitions: no input changes the pose.</summary>
    private static SamusPoseInputMatch MatchNoInputTransitions()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>The priority order of both forward-facing poses' input list, in the power suit and suited.</summary>
    private static SamusPoseInputMatch MatchForwardFacing(GameplayAction held)
    {
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the right-facing standing poses' and landings' input list, whatever their aim; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchStandingAndLandingRight(GameplayAction held, GameplayAction newlyPressed)
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

    /// <summary>The priority order of the right-facing running poses' input list, whatever their aim; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchRunningRight(GameplayAction held, GameplayAction newlyPressed)
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

    /// <summary>The priority order of the right-facing jump transitions' input list, whatever their aim.</summary>
    private static SamusPoseInputMatch MatchJumpTransitionRight(GameplayAction held)
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

    /// <summary>The priority order of the left-facing jump transitions' input list, whatever their aim.</summary>
    private static SamusPoseInputMatch MatchJumpTransitionLeft(GameplayAction held)
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

    /// <summary>The priority order of the right-facing neutral, forward, aimed-up and diagonal normal jumps' input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchNormalJumpRight(GameplayAction held)
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
    private static SamusPoseInputMatch MatchDamageBoostLeft(GameplayAction held)
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
    private static SamusPoseInputMatch MatchDamageBoostRight(GameplayAction held)
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
    private static SamusPoseInputMatch MatchSpinJumpRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchSpaceJumpRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchScrewAttackRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchMorphBallGroundRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchMorphBallGroundLeft(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchMorphBallMovingRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchMorphBallMovingLeft(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchGlitchBallRight()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>The priority order of the crouching right input list.</summary>
    private static SamusPoseInputMatch MatchCrouchingRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchCrouchingLeft(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchFallingRight(GameplayAction held)
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
    private static SamusPoseInputMatch MatchMorphBallFallingRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchGlitchBallKnockbackRight()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>The priority order of the left-facing moonwalk poses' input list, whatever their aim.</summary>
    private static SamusPoseInputMatch MatchMoonwalkLeft(GameplayAction held, GameplayAction newlyPressed)
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

    /// <summary>The priority order of the right-facing moonwalk poses' input list, whatever their aim.</summary>
    [MirroredAsymmetry("the right-facing moonwalk list exchanges the diagonal aim-up and aim-down chords and aimed poses as well as the directions")]
    private static SamusPoseInputMatch MatchMoonwalkRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchKnockbackRight(GameplayAction held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.DamageBoostRightPose);
        return new(null, HasConditions: true);
    }
    /// <summary>The priority order of the spring ball ground right input list.</summary>
    private static SamusPoseInputMatch MatchSpringBallGroundRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchSpringBallGroundLeft(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchSpringBallFallingRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchSpringBallJumpRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchSpringBallJumpLeft(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallJumpRightPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallJumpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the wall jump right input list; the mirrored list has the same order mirrored.</summary>
    private static SamusPoseInputMatch MatchWallJumpRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchRanIntoWallRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchNormalJumpGunExtendedRight(GameplayAction held)
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
    private static SamusPoseInputMatch MatchNormalJumpAimDownRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchNormalJumpAimDownLeft(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchUnmorphingTransitionRight(GameplayAction held)
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
    private static SamusPoseInputMatch MatchTurningRightToLeft(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchTurningRightToLeftAimUp(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchTurningRightToLeftAimDiagonalDown(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchShinesparkWindupRight(GameplayAction held)
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
    private static SamusPoseInputMatch MatchFallingAimDownRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchDraygonHeldBallLeft(GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.DraygonHeldStandingUpLeftPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the input list of every right-facing pose Draygon's grab holds Samus in.</summary>
    private static SamusPoseInputMatch MatchDraygonGrabbedRight(GameplayAction held)
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

    /// <summary>The priority order of the input list of every left-facing pose Draygon's grab holds Samus in.</summary>
    private static SamusPoseInputMatch MatchDraygonGrabbedLeft(GameplayAction held)
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
    private static SamusPoseInputMatch MatchMovingRightGunExtended(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchFallingGunExtendedRight(GameplayAction held)
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
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpRight(GameplayAction held, GameplayAction newlyPressed)
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
    private static SamusPoseInputMatch MatchMoonwalkTurnJumpAimedRight(SamusPoseId aimedTurnJump, GameplayAction held, GameplayAction newlyPressed)
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
