using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Input.GameplayAction;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputs;
using static SuperMetroid.Core.Samus.Poses.SamusPoseTransitionSetup;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Ordered pose-input decisions; the first satisfied chord wins, and each names the setup that installs its pose.</summary>
internal static class SamusPoseInputRules
{
    /// <summary>Selects the first satisfied input chord in <paramref name="inputList"/>, in the order the list authors them.</summary>
    internal static SamusPoseInputMatch Match(SamusPoseInputList inputList, GameplayAction held, GameplayAction newlyPressed) =>
        Ensure.NotNull(inputList).Match(held, newlyPressed);

    /// <summary>The list with no transitions: no input changes the pose.</summary>
    internal static SamusPoseInputMatch MatchNoInputTransitions()
    {
        return new(null, HasConditions: false);
    }

    /// <summary>The priority order of both forward-facing poses' input list, in the power suit and suited.</summary>
    internal static SamusPoseInputMatch MatchForwardFacing(GameplayAction held)
    {
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose, TurnOnGround);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose, TurnOnGround);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the right-facing standing poses' and landings' input list, whatever their aim; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchStandingAndLandingRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Up))
            return Accept(SamusPoseId.NormalJumpTransitionAimUpRightPose, StartJump);
        if (Has(newlyPressed, Jump) && Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose, StartJump);
        if (Has(newlyPressed, Jump) && Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose, StartJump);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose, StartJump);
        if (Has(newlyPressed, Down) && Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.CrouchingTransitionAimUpRightPose, CrouchOrStand);
        if (Has(newlyPressed, Down) && Has(held, AimUp))
            return Accept(SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose, CrouchOrStand);
        if (Has(newlyPressed, Down) && Has(held, AimDown))
            return Accept(SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose, CrouchOrStand);
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionRightPose, CrouchOrStand);
        if (Has(held, AimDown | Shoot | Left))
            return Accept(SamusPoseId.MoonwalkAimDownRightPose, ChangeMoonwalkPose);
        if (Has(held, AimUp | Shoot | Left))
            return Accept(SamusPoseId.MoonwalkAimUpRightPose, ChangeMoonwalkPose);
        if (Has(held, AimUp | AimDown | Left))
            return Accept(SamusPoseId.TurningRightToLeftPose, TurnOnGround);
        if (Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.StandingAimUpRightPose, ChangeGroundedAim);
        if (Has(held, AimUp | Right))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, AimDown | Right))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose, ChangeGroundedAim);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose, ChangeGroundedAim);
        if (Has(held, Shoot | Left))
            return Accept(SamusPoseId.MoonwalkFacingRightPose, ChangeMoonwalkPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose, TurnOnGround);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpRightPose, ChangeGroundedAim);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownRightPose, ChangeGroundedAim);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightNormalPose, StartRunning);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the right-facing running poses' input list, whatever their aim; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchRunningRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionRightPose, CrouchOrStand);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.SpinJumpRightPose, StartJump);
        if (Has(held, AimUp | Right))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, AimDown | Right))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose, ChangeGroundedAim);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose, ChangeGroundedAim);
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.MovingRightGunExtendedPose, ChangeGroundedAim);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightNormalPose, ChangeGroundedAim);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose, TurnOnGround);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpRightPose, ChangeGroundedAim);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownRightPose, ChangeGroundedAim);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the right-facing jump transitions' input list, whatever their aim.</summary>
    internal static SamusPoseInputMatch MatchJumpTransitionRight(GameplayAction held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.TurningRightToLeftJumpPose, TurnInAir);
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, ChangeAerialAim);
        if (Has(held, Jump | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, ChangeCompactAerialPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, AimDown | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose, ChangeAerialAim);
        if (Has(held, Shoot | Jump))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, ChangeAerialAim);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, ChangeAerialAim);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the left-facing jump transitions' input list, whatever their aim.</summary>
    internal static SamusPoseInputMatch MatchJumpTransitionLeft(GameplayAction held)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.TurningLeftToRightJumpPose, TurnInAir);
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose, ChangeAerialAim);
        if (Has(held, Jump | Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose, ChangeCompactAerialPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose, ChangeAerialAim);
        if (Has(held, AimDown | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose, ChangeAerialAim);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.NormalJumpForwardLeftPose, ChangeAerialAim);
        if (Has(held, Shoot | Jump))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose, ChangeAerialAim);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightJumpPose, TurnInAir);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose, ChangeAerialAim);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the right-facing neutral, forward, aimed-up and diagonal normal jumps' input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchNormalJumpRight(GameplayAction held)
    {
        if (Has(held, Jump | Right | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, Jump | Right | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, AimUp | Jump | Right))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, AimDown | Jump | Right))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.TurningRightToLeftJumpPose, TurnInAir);
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, ChangeAerialAim);
        if (Has(held, Jump | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, ChangeCompactAerialPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, AimDown | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose, ChangeAerialAim);
        if (Has(held, Shoot | Jump))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, ChangeAerialAim);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftJumpPose, TurnInAir);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, ChangeAerialAim);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, ChangeCompactAerialPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose, ChangeAerialAim);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NeutralJumpRightPose, ChangeAerialAim);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, ChangeAerialAim);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the damage boost left input list.</summary>
    internal static SamusPoseInputMatch MatchDamageBoostLeft(GameplayAction held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.NormalJumpForwardLeftPose, JumpFromDamageBoost);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.DamageBoostLeftPose, KeepPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NeutralJumpLeftPose, JumpFromDamageBoost);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the damage boost right input list.</summary>
    internal static SamusPoseInputMatch MatchDamageBoostRight(GameplayAction held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.DamageBoostRightPose, KeepPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose, JumpFromDamageBoost);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NeutralJumpRightPose, JumpFromDamageBoost);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the spin jump right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchSpinJumpRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, AimFromSpin);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, AimFromSpin);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, AimFromSpin);
        if (Has(held, AimUp | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, AimFromSpin);
        if (Has(held, AimDown | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, AimFromSpin);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.SpinJumpRightPose, KeepPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, AimFromSpin);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, AimFromSpin);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, AimFromSpin);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, AimFromSpin);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose, KeepPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose, ChangeSpinDirection);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the space jump right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchSpaceJumpRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, AimFromSpin);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, AimFromSpin);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, AimFromSpin);
        if (Has(held, AimUp | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, AimFromSpin);
        if (Has(held, AimDown | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, AimFromSpin);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.SpaceJumpRightPose, KeepPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, AimFromSpin);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, AimFromSpin);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, AimFromSpin);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, AimFromSpin);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpaceJumpRightPose, KeepPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpaceJumpLeftPose, ChangeSpinDirection);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the screw attack right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchScrewAttackRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, AimFromSpin);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, AimFromSpin);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, AimFromSpin);
        if (Has(held, AimUp | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, AimFromSpin);
        if (Has(held, AimDown | Shoot))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, AimFromSpin);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.ScrewAttackRightPose, KeepPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, AimFromSpin);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, AimFromSpin);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, AimFromSpin);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, AimFromSpin);
        if (Has(held, Right))
            return Accept(SamusPoseId.ScrewAttackRightPose, KeepPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.ScrewAttackLeftPose, ChangeSpinDirection);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the right-facing morph ball's input list on the ground, still or rolling.</summary>
    internal static SamusPoseInputMatch MatchMorphBallRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(held, Right))
            return Accept(SamusPoseId.MorphBallMovingRightPose, ChangeBallPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MorphBallMovingLeftPose, ChangeBallPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the left-facing morph ball's input list on the ground, still or rolling.</summary>
    internal static SamusPoseInputMatch MatchMorphBallLeft(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose, MorphOrUnmorph);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose, MorphOrUnmorph);
        if (Has(held, Right))
            return Accept(SamusPoseId.MorphBallMovingRightPose, ChangeBallPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MorphBallMovingLeftPose, ChangeBallPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the crouching right input list.</summary>
    internal static SamusPoseInputMatch MatchCrouchingRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up) && Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.StandingTransitionAimUpRightPose, CrouchOrStand);
        if (Has(newlyPressed, Up) && Has(held, AimUp))
            return Accept(SamusPoseId.StandingTransitionAimDiagonalUpRightPose, CrouchOrStand);
        if (Has(newlyPressed, Up) && Has(held, AimDown))
            return Accept(SamusPoseId.StandingTransitionAimDiagonalDownRightPose, CrouchOrStand);
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.StandingTransitionRightPose, CrouchOrStand);
        if (Has(newlyPressed, Left))
            return Accept(SamusPoseId.TurningRightToLeftCrouchingPose, TurnOnGround);
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose, JumpFromCrouch);
        if (Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.CrouchingAimUpRightPose, ChangeGroundedAim);
        if (Has(held, AimUp | Right))
            return Accept(SamusPoseId.FacingRightNormalPose, StandFromCrouch);
        if (Has(held, AimDown | Right))
            return Accept(SamusPoseId.FacingRightNormalPose, StandFromCrouch);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.CrouchingAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.CrouchingAimDiagonalDownRightPose, ChangeGroundedAim);
        if (Has(held, Right))
            return Accept(SamusPoseId.FacingRightNormalPose, StandFromCrouch);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the crouching left input list.</summary>
    [MirroredAsymmetry("the left-facing crouch list exchanges the diagonal aim-up and aim-down chords as well as the directions")]
    internal static SamusPoseInputMatch MatchCrouchingLeft(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up) && Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.StandingTransitionAimUpLeftPose, CrouchOrStand);
        if (Has(newlyPressed, Up) && Has(held, AimUp))
            return Accept(SamusPoseId.StandingTransitionAimDiagonalUpLeftPose, CrouchOrStand);
        if (Has(newlyPressed, Up) && Has(held, AimDown))
            return Accept(SamusPoseId.StandingTransitionAimDiagonalDownLeftPose, CrouchOrStand);
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.StandingTransitionLeftPose, CrouchOrStand);
        if (Has(newlyPressed, Right))
            return Accept(SamusPoseId.TurningLeftToRightCrouchingPose, TurnOnGround);
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionLeftPose, MorphOrUnmorph);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose, JumpFromCrouch);
        if (Has(held, AimUp | AimDown))
            return Accept(SamusPoseId.CrouchingAimUpLeftPose, ChangeGroundedAim);
        if (Has(held, AimDown | Left))
            return Accept(SamusPoseId.FacingLeftNormalPose, StandFromCrouch);
        if (Has(held, AimUp | Left))
            return Accept(SamusPoseId.FacingLeftNormalPose, StandFromCrouch);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.CrouchingAimDiagonalUpLeftPose, ChangeGroundedAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.CrouchingAimDiagonalDownLeftPose, ChangeGroundedAim);
        if (Has(held, Left))
            return Accept(SamusPoseId.FacingLeftNormalPose, StandFromCrouch);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the falling right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchFallingRight(GameplayAction held)
    {
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Left | Up))
            return Accept(SamusPoseId.TurningRightToLeftFallingPose, TurnInAir);
        if (Has(held, Left | Down))
            return Accept(SamusPoseId.TurningRightToLeftFallingPose, TurnInAir);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftFallingPose, TurnInAir);
        if (Has(held, Up))
            return Accept(SamusPoseId.FallingAimUpRightPose, ChangeAerialAim);
        if (Has(held, Down))
            return Accept(SamusPoseId.FallingAimDownRightPose, ChangeCompactAerialPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.FallingGunExtendedRightPose, ChangeAerialAim);
        if (Has(held, Right))
            return Accept(SamusPoseId.FallingRightPose, ChangeAerialAim);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the morph ball falling right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchMorphBallFallingRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(held, Right))
            return Accept(SamusPoseId.MorphBallFallingRightPose, KeepPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MorphBallFallingLeftPose, ChangeBallPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the left-facing moonwalk poses' input list, whatever their aim.</summary>
    internal static SamusPoseInputMatch MatchMoonwalkLeft(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionLeftPose, CrouchOrStand);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.MoonwalkTurnJumpRightPose, StartMoonwalkTurnJump);
        if (Has(held, AimDown | Shoot | Right))
            return Accept(SamusPoseId.MoonwalkAimDownLeftPose, ChangeMoonwalkPose);
        if (Has(held, AimUp | Shoot | Right))
            return Accept(SamusPoseId.MoonwalkAimUpLeftPose, ChangeMoonwalkPose);
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.MoonwalkFacingLeftPose, ChangeMoonwalkPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MovingLeftNormalPose, ChangeMoonwalkPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose, TurnOnGround);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the right-facing moonwalk poses' input list, whatever their aim.</summary>
    [MirroredAsymmetry("the right-facing moonwalk list exchanges the diagonal aim-up and aim-down chords and aimed poses as well as the directions")]
    internal static SamusPoseInputMatch MatchMoonwalkRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionRightPose, CrouchOrStand);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.MoonwalkTurnJumpLeftPose, StartMoonwalkTurnJump);
        if (Has(held, AimUp | Shoot | Left))
            return Accept(SamusPoseId.MoonwalkAimUpRightPose, ChangeMoonwalkPose);
        if (Has(held, AimDown | Shoot | Left))
            return Accept(SamusPoseId.MoonwalkAimDownRightPose, ChangeMoonwalkPose);
        if (Has(held, Shoot | Left))
            return Accept(SamusPoseId.MoonwalkFacingRightPose, ChangeMoonwalkPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightNormalPose, ChangeMoonwalkPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose, TurnOnGround);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the knockback right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchKnockbackRight(GameplayAction held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.DamageBoostRightPose, StartDamageBoost);
        return new(null, HasConditions: true);
    }
    /// <summary>The priority order of the spring ball ground right input list.</summary>
    internal static SamusPoseInputMatch MatchSpringBallGroundRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.SpringBallJumpRightPose, JumpSpringBall);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallMovingRightPose, ChangeBallPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallMovingLeftPose, ChangeBallPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the spring ball ground left input list.</summary>
    internal static SamusPoseInputMatch MatchSpringBallGroundLeft(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose, MorphOrUnmorph);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.SpringBallJumpLeftPose, JumpSpringBall);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallMovingRightPose, ChangeBallPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallMovingLeftPose, ChangeBallPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the spring ball falling right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchSpringBallFallingRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallFallingLeftPose, ChangeBallPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallFallingRightPose, KeepPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the spring ball jump right input list.</summary>
    internal static SamusPoseInputMatch MatchSpringBallJumpRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallJumpRightPose, KeepPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallJumpLeftPose, ChangeBallPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the spring ball jump left input list.</summary>
    internal static SamusPoseInputMatch MatchSpringBallJumpLeft(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.UnmorphingTransitionLeftPose, MorphOrUnmorph);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallJumpRightPose, ChangeBallPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallJumpLeftPose, KeepPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the wall jump right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchWallJumpRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose, ChangeSpinDirection);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, AimFromSpin);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, AimFromSpin);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, AimFromSpin);
        if (Has(held, Jump))
            return Accept(SamusPoseId.WallJumpRightPose, KeepPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the ran into wall right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchRanIntoWallRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose, StartJump);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose, ChangeGroundedAim);
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionRightPose, CrouchOrStand);
        if (Has(held, AimDown | Left))
            return Accept(SamusPoseId.MoonwalkAimDownRightPose, ChangeMoonwalkPose);
        if (Has(held, AimUp | Left))
            return Accept(SamusPoseId.MoonwalkAimUpRightPose, ChangeMoonwalkPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpRightPose, ChangeGroundedAim);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownRightPose, ChangeGroundedAim);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose, TurnOnGround);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightNormalPose, StartRunning);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the normal jump gun extended right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchNormalJumpGunExtendedRight(GameplayAction held)
    {
        if (Has(held, Jump | Right | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, Jump | Right | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, AimUp | Jump | Right))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, AimDown | Jump | Right))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.TurningRightToLeftJumpPose, TurnInAir);
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, ChangeAerialAim);
        if (Has(held, Jump | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, ChangeCompactAerialPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, AimDown | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose, ChangeAerialAim);
        if (Has(held, Shoot | Jump))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, KeepPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftJumpPose, TurnInAir);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, ChangeAerialAim);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, ChangeCompactAerialPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose, ChangeAerialAim);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, KeepPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the normal jump aim down right input list.</summary>
    internal static SamusPoseInputMatch MatchNormalJumpAimDownRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(held, Jump | Right | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeCompactAerialPose);
        if (Has(held, Jump | Right | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeCompactAerialPose);
        if (Has(held, AimUp | Jump | Right))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeCompactAerialPose);
        if (Has(held, AimDown | Jump | Right))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeCompactAerialPose);
        if (Has(held, Shoot | Jump | Right))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, ChangeCompactAerialPose);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeCompactAerialPose);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeCompactAerialPose);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.TurningRightToLeftJumpPose, TurnInAir);
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, ChangeCompactAerialPose);
        if (Has(held, Jump | Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, KeepPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeCompactAerialPose);
        if (Has(held, AimDown | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeCompactAerialPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose, ChangeCompactAerialPose);
        if (Has(held, Shoot | Jump))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, ChangeCompactAerialPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftJumpPose, TurnInAir);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, ChangeCompactAerialPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, KeepPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, ChangeCompactAerialPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, ChangeCompactAerialPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.NormalJumpForwardRightPose, ChangeCompactAerialPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, KeepPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, ChangeCompactAerialPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the normal jump aim down left input list.</summary>
    internal static SamusPoseInputMatch MatchNormalJumpAimDownLeft(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionLeftPose, MorphOrUnmorph);
        if (Has(held, Jump | Left | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose, ChangeCompactAerialPose);
        if (Has(held, Jump | Left | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose, ChangeCompactAerialPose);
        if (Has(held, AimUp | Jump | Left))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose, ChangeCompactAerialPose);
        if (Has(held, AimDown | Jump | Left))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose, ChangeCompactAerialPose);
        if (Has(held, Left | Up))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose, ChangeCompactAerialPose);
        if (Has(held, Left | Down))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose, ChangeCompactAerialPose);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.TurningLeftToRightJumpPose, TurnInAir);
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose, ChangeCompactAerialPose);
        if (Has(held, Jump | Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose, KeepPose);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose, ChangeCompactAerialPose);
        if (Has(held, AimDown | Jump))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose, ChangeCompactAerialPose);
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.NormalJumpForwardLeftPose, ChangeCompactAerialPose);
        if (Has(held, Shoot | Jump))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose, ChangeCompactAerialPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightJumpPose, TurnInAir);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpLeftPose, ChangeCompactAerialPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose, KeepPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpLeftPose, ChangeCompactAerialPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownLeftPose, ChangeCompactAerialPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.NormalJumpForwardLeftPose, ChangeCompactAerialPose);
        if (Has(held, Jump))
            return Accept(SamusPoseId.NormalJumpAimDownLeftPose, KeepPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.NormalJumpGunExtendedLeftPose, ChangeCompactAerialPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the unmorphing transition right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchUnmorphingTransitionRight(GameplayAction held)
    {
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.FallingGunExtendedRightPose, FallFromUnmorph);
        if (Has(held, Shoot | Up))
            return Accept(SamusPoseId.FallingAimUpRightPose, FallFromUnmorph);
        if (Has(held, Shoot | Down))
            return Accept(SamusPoseId.FallingAimDownRightPose, FallFromUnmorph);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the turning right to left input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchTurningRightToLeft(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.SpinJumpLeftPose, StartJump);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose, StartJump);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose, KeepPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the turning right to left aim up input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchTurningRightToLeftAimUp(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose, StartJump);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose, StartJump);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftAimUpPose, KeepPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the turning right to left aim diagonal down input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchTurningRightToLeftAimDiagonalDown(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose, StartJump);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose, StartJump);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftAimDiagonalDownPose, KeepPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the shinespark windup right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchShinesparkWindupRight(GameplayAction held)
    {
        if (Has(held, Jump | Up))
            return Accept(SamusPoseId.ShinesparkVerticalRightPose, AimShinespark);
        if (Has(held, AimUp | Jump))
            return Accept(SamusPoseId.ShinesparkDiagonalRightPose, AimShinespark);
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.ShinesparkHorizontalRightPose, AimShinespark);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the falling aim down right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchFallingAimDownRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.MorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose, ChangeCompactAerialPose);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose, ChangeCompactAerialPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.FallingAimUpRightPose, ChangeCompactAerialPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.FallingAimDownRightPose, KeepPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftFallingPose, TurnInAir);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose, ChangeCompactAerialPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose, ChangeCompactAerialPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.FallingGunExtendedRightPose, ChangeCompactAerialPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.FallingRightPose, ChangeCompactAerialPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the input list of the left-facing ball pose Draygon holds Samus in.</summary>
    internal static SamusPoseInputMatch MatchDraygonHeldBallLeft(GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.DraygonHeldStandingUpLeftPose, StandUpFromDraygonHold);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the input list of every right-facing pose Draygon's grab holds Samus in.</summary>
    internal static SamusPoseInputMatch MatchDraygonGrabbedRight(GameplayAction held)
    {
        if (Has(held, Shoot | Right | Up))
            return Accept(SamusPoseId.DraygonGrabbedAimUpRightPose, ChangeDraygonGrabbedPose);
        if (Has(held, Shoot | Right | Down))
            return Accept(SamusPoseId.DraygonGrabbedAimDownRightPose, ChangeDraygonGrabbedPose);
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.DraygonGrabbedFiringRightPose, ChangeDraygonGrabbedPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.DraygonGrabbedAimUpRightPose, ChangeDraygonGrabbedPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.DraygonGrabbedAimDownRightPose, ChangeDraygonGrabbedPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.DraygonGrabbedFiringRightPose, ChangeDraygonGrabbedPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.DraygonGrabbedMovingRightPose, ChangeDraygonGrabbedPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.DraygonGrabbedMovingRightPose, ChangeDraygonGrabbedPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.DraygonGrabbedMovingRightPose, ChangeDraygonGrabbedPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.DraygonGrabbedMovingRightPose, ChangeDraygonGrabbedPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the input list of every left-facing pose Draygon's grab holds Samus in.</summary>
    internal static SamusPoseInputMatch MatchDraygonGrabbedLeft(GameplayAction held)
    {
        if (Has(held, Shoot | Left | Up))
            return Accept(SamusPoseId.DraygonGrabbedAimUpLeftPose, ChangeDraygonGrabbedPose);
        if (Has(held, Shoot | Left | Down))
            return Accept(SamusPoseId.DraygonGrabbedAimDownLeftPose, ChangeDraygonGrabbedPose);
        if (Has(held, Shoot | Left))
            return Accept(SamusPoseId.DraygonGrabbedFiringLeftPose, ChangeDraygonGrabbedPose);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.DraygonGrabbedAimUpLeftPose, ChangeDraygonGrabbedPose);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.DraygonGrabbedAimDownLeftPose, ChangeDraygonGrabbedPose);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.DraygonGrabbedFiringLeftPose, ChangeDraygonGrabbedPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.DraygonGrabbedMovingLeftPose, ChangeDraygonGrabbedPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.DraygonGrabbedMovingLeftPose, ChangeDraygonGrabbedPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.DraygonGrabbedMovingLeftPose, ChangeDraygonGrabbedPose);
        if (Has(held, Down))
            return Accept(SamusPoseId.DraygonGrabbedMovingLeftPose, ChangeDraygonGrabbedPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the moving right gun extended input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchMovingRightGunExtended(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionRightPose, CrouchOrStand);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.SpinJumpRightPose, StartJump);
        if (Has(held, AimUp | Right))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, AimDown | Right))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose, ChangeGroundedAim);
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.RunningAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.RunningAimDiagonalDownRightPose, ChangeGroundedAim);
        if (Has(held, Shoot | Right))
            return Accept(SamusPoseId.MovingRightGunExtendedPose, KeepPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.MovingRightGunExtendedPose, KeepPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose, TurnOnGround);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpRightPose, ChangeGroundedAim);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownRightPose, ChangeGroundedAim);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the falling gun extended right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchFallingGunExtendedRight(GameplayAction held)
    {
        if (Has(held, Right | Up))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, Right | Down))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Up))
            return Accept(SamusPoseId.FallingAimUpRightPose, ChangeAerialAim);
        if (Has(held, Down))
            return Accept(SamusPoseId.FallingAimDownRightPose, ChangeCompactAerialPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftFallingPose, TurnInAir);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.FallingAimDiagonalUpRightPose, ChangeAerialAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.FallingAimDiagonalDownRightPose, ChangeAerialAim);
        if (Has(held, Shoot))
            return Accept(SamusPoseId.FallingGunExtendedRightPose, KeepPose);
        if (Has(held, Right))
            return Accept(SamusPoseId.FallingGunExtendedRightPose, KeepPose);
        return new(null, HasConditions: true);
    }

    /// <summary>The priority order of the moonwalk turn jump right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchMoonwalkTurnJumpRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.SpinJumpRightPose, StartJump);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose, StartJump);
        if (Has(held, Right))
            return Accept(SamusPoseId.MoonwalkTurnJumpRightPose, KeepPose);
        return new(null, HasConditions: true);
    }

    /// <summary>
    /// The aimed-up and aimed-down moonwalk turn-jump input lists, in both directions: one priority order for an aimed
    /// moonwalk turn jump to the right, continuing <paramref name="aimedTurnJump"/>; the leftward lists are it mirrored.
    /// </summary>
    internal static SamusPoseInputMatch MatchMoonwalkTurnJumpAimedRight(SamusPoseId aimedTurnJump, GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose, StartJump);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose, StartJump);
        if (Has(held, Right))
            return Accept(aimedTurnJump, KeepPose);
        return new(null, HasConditions: true);
    }
}
