using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Input.GameplayAction;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputs;
using static SuperMetroid.Core.Samus.Poses.SamusPoseTransitionSetup;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Ordered pose-input decisions; the first satisfied chord wins, and each names the setup that installs its pose.</summary>
internal static partial class SamusPoseInputRules
{
    /// <summary>Selects the first satisfied input chord in <paramref name="inputList"/>, in the order the list authors them.</summary>
    internal static SamusPoseInputMatch Match(SamusPoseInputList inputList, GameplayAction held, GameplayAction newlyPressed) =>
        Ensure.NotNull(inputList).Match(held, newlyPressed);

    /// <summary>The list with no transitions: no input changes the pose.</summary>
    internal static SamusPoseInputMatch MatchNoInputTransitions()
    {
        return SamusPoseInputMatch.NoTransitions;
    }

    /// <summary>The priority order of both forward-facing poses' input list, in the power suit and suited.</summary>
    internal static SamusPoseInputMatch MatchForwardFacing(GameplayAction held)
    {
        if (Has(held, Right))
            return Accept(SamusPoseId.TurningLeftToRightPose, TurnOnGround);
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose, TurnOnGround);
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>The priority order of the right-facing running poses' input list, whatever their aim; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchRunningRight(GameplayAction held, GameplayAction newlyPressed) =>
        RunningOpening(held, newlyPressed)
        ?? (Has(held, Shoot | Right) ? Accept(SamusPoseId.MovingRightGunExtendedPose, ChangeGroundedAim)
            : Has(held, Right) ? Accept(SamusPoseId.MovingRightNormalPose, ChangeGroundedAim)
            : RunningClosing(held));

    /// <summary>
    /// The priority order of the right-facing gun-extended run's input list (#2557): the running chords, Right with or
    /// without Shoot keeping the gun-extended run; the mirrored list has the same order mirrored.
    /// </summary>
    internal static SamusPoseInputMatch MatchMovingRightGunExtended(GameplayAction held, GameplayAction newlyPressed) =>
        RunningOpening(held, newlyPressed)
        ?? (Has(held, Right) ? Accept(SamusPoseId.MovingRightGunExtendedPose, KeepPose) : RunningClosing(held));

    /// <summary>The right-facing running chords before its forward ones; none when no chord holds.</summary>
    private static SamusPoseInputMatch? RunningOpening(GameplayAction held, GameplayAction newlyPressed)
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
        return null;
    }

    /// <summary>The right-facing running chords after its forward ones.</summary>
    private static SamusPoseInputMatch RunningClosing(GameplayAction held)
    {
        if (Has(held, Left))
            return Accept(SamusPoseId.TurningRightToLeftPose, TurnOnGround);
        if (Has(held, Up))
            return Accept(SamusPoseId.StandingAimUpRightPose, ChangeGroundedAim);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.StandingAimDiagonalUpRightPose, ChangeGroundedAim);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.StandingAimDiagonalDownRightPose, ChangeGroundedAim);
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>The priority order of the right-facing neutral, forward, aimed-up and diagonal normal jumps' input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchNormalJumpRight(GameplayAction held) =>
        AimedJumpChords(held, gunExtended: ChangeAerialAim)
        ?? (Has(held, Jump) ? Accept(SamusPoseId.NeutralJumpRightPose, ChangeAerialAim)
            : Has(held, Shoot) ? Accept(SamusPoseId.NormalJumpGunExtendedRightPose, ChangeAerialAim)
            : SamusPoseInputMatch.Exhausted);

    /// <summary>
    /// The priority order of the right-facing gun-extended normal jump's input list (#2557): the normal jump's chords, the
    /// gun-extended pose kept rather than set up again, and no bare Jump chord; the mirrored list has the same order mirrored.
    /// </summary>
    internal static SamusPoseInputMatch MatchNormalJumpGunExtendedRight(GameplayAction held) =>
        AimedJumpChords(held, gunExtended: KeepPose)
        ?? (Has(held, Shoot) ? Accept(SamusPoseId.NormalJumpGunExtendedRightPose, KeepPose) : SamusPoseInputMatch.Exhausted);

    /// <summary>
    /// The chords a right-facing normal jump and its gun-extended pose test alike, in order (#2557), the gun-extended pose set
    /// up by <paramref name="gunExtended"/>; none when no chord holds.
    /// </summary>
    private static SamusPoseInputMatch? AimedJumpChords(GameplayAction held, SamusPoseTransitionSetup gunExtended)
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
            return Accept(SamusPoseId.NormalJumpGunExtendedRightPose, gunExtended);
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
        return null;
    }

    /// <summary>
    /// The priority order of a damage boost's input list, the boost facing <paramref name="toward"/> (#2557): Jump with Left is
    /// tested before Jump with Right whichever way it faces; Jump toward its facing jumps forward, away from it keeps the
    /// boost, and Jump alone jumps neutrally. Its poses are the right-facing boost's, mirrored for one facing left.
    /// </summary>
    internal static SamusPoseInputMatch MatchDamageBoost(GameplayAction toward, GameplayAction held)
    {
        SamusPoseInputMatch match = Has(held, Jump | Left) ? DamageBoostJump(Left)
            : Has(held, Jump | Right) ? DamageBoostJump(Right)
            : Has(held, Jump) ? Accept(SamusPoseId.NeutralJumpRightPose, JumpFromDamageBoost)
            : SamusPoseInputMatch.Exhausted;
        return toward == Left ? match.Mirrored() : match;

        SamusPoseInputMatch DamageBoostJump(GameplayAction direction) => direction == toward
            ? Accept(SamusPoseId.NormalJumpForwardRightPose, JumpFromDamageBoost)
            : Accept(SamusPoseId.DamageBoostRightPose, KeepPose);
    }

    /// <summary>
    /// The priority order of a right-facing spin's input list, the spin jump's, the Space Jump's or the Screw Attack's
    /// (#2557): the spin keeps <paramref name="spin"/> and turns into its left-facing mirror; the mirrored list has the same
    /// order mirrored.
    /// </summary>
    internal static SamusPoseInputMatch MatchSpinRight(SamusPoseId spin, GameplayAction held, GameplayAction newlyPressed)
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
            return Accept(spin, KeepPose);
        if (Has(held, Up))
            return Accept(SamusPoseId.NormalJumpAimUpRightPose, AimFromSpin);
        if (Has(held, AimUp))
            return Accept(SamusPoseId.NormalJumpAimDiagonalUpRightPose, AimFromSpin);
        if (Has(held, AimDown))
            return Accept(SamusPoseId.NormalJumpAimDiagonalDownRightPose, AimFromSpin);
        if (Has(held, Down))
            return Accept(SamusPoseId.NormalJumpAimDownRightPose, AimFromSpin);
        if (Has(held, Right))
            return Accept(spin, KeepPose);
        if (Has(held, Left))
            return Accept(SamusPoseMirror.Of(spin), ChangeSpinDirection);
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>
    /// The priority order of the morph ball's input list on the ground, still or rolling (#2557): Up or Jump unmorphs it
    /// through <paramref name="unmorph"/>, its facing's transition, and a direction rolls it that way.
    /// </summary>
    internal static SamusPoseInputMatch MatchMorphBall(SamusPoseId unmorph, GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(unmorph, MorphOrUnmorph);
        if (Has(newlyPressed, Jump))
            return Accept(unmorph, MorphOrUnmorph);
        if (Has(held, Right))
            return Accept(SamusPoseId.MorphBallMovingRightPose, ChangeBallPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.MorphBallMovingLeftPose, ChangeBallPose);
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>The priority order of the left-facing moonwalk poses' input list, whatever their aim.</summary>
    internal static SamusPoseInputMatch MatchMoonwalkLeft(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionLeftPose, CrouchOrStand);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.MoonwalkTurnToSpinJumpRightPose, StartMoonwalkTurnToSpinJump);
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
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>The priority order of the right-facing moonwalk poses' input list, whatever their aim.</summary>
    [MirroredAsymmetry("the right-facing moonwalk list exchanges the diagonal aim-up and aim-down chords and aimed poses as well as the directions")]
    internal static SamusPoseInputMatch MatchMoonwalkRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Down))
            return Accept(SamusPoseId.CrouchingTransitionRightPose, CrouchOrStand);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.MoonwalkTurnToSpinJumpLeftPose, StartMoonwalkTurnToSpinJump);
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
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>The priority order of the knockback right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchKnockbackRight(GameplayAction held)
    {
        if (Has(held, Jump | Left))
            return Accept(SamusPoseId.DamageBoostRightPose, StartDamageBoost);
        return SamusPoseInputMatch.Exhausted;
    }
    /// <summary>
    /// The priority order of the Spring Ball's input list on the ground (#2557): Up unmorphs it through
    /// <paramref name="unmorph"/> and Jump springs it into <paramref name="jump"/>, its facing's, and a direction rolls it
    /// that way.
    /// </summary>
    internal static SamusPoseInputMatch MatchSpringBallGround(SamusPoseId unmorph, SamusPoseId jump, GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(unmorph, MorphOrUnmorph);
        if (Has(newlyPressed, Jump))
            return Accept(jump, JumpSpringBall);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallMovingRightPose, ChangeBallPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallMovingLeftPose, ChangeBallPose);
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>
    /// The priority order of the Spring Ball jump's input list, the jump facing <paramref name="toward"/> (#2557): Up unmorphs
    /// it through its facing's transition, and Right is tested before Left whichever way it faces, a direction toward its
    /// facing keeping the jump and one away from it turning the ball.
    /// </summary>
    internal static SamusPoseInputMatch MatchSpringBallJump(GameplayAction toward, GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(toward == Left ? SamusPoseId.UnmorphingTransitionLeftPose : SamusPoseId.UnmorphingTransitionRightPose, MorphOrUnmorph);
        if (Has(held, Right))
            return Accept(SamusPoseId.SpringBallJumpRightPose, toward == Right ? KeepPose : ChangeBallPose);
        if (Has(held, Left))
            return Accept(SamusPoseId.SpringBallJumpLeftPose, toward == Left ? KeepPose : ChangeBallPose);
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>
    /// The priority order of the right-facing aimed-down normal jump's input list (#2557); the left-facing list is it mirrored
    /// without its forward Shoot-and-Jump chord (<see cref="MatchNormalJumpAimDownLeftOfRight"/>).
    /// </summary>
    internal static SamusPoseInputMatch MatchNormalJumpAimDownRight(GameplayAction held, GameplayAction newlyPressed) =>
        AimedDownJumpOpening(held, newlyPressed)
        ?? (Has(held, Shoot | Jump | Right) ? Accept(SamusPoseId.NormalJumpGunExtendedRightPose, ChangeCompactAerialPose) : AimedDownJumpClosing(held));

    /// <summary>
    /// The left-facing aimed-down normal jump's input list read as the right-facing one (#2557), its buttons and poses to be
    /// mirrored for the left-facing jump: the right-facing list without its forward Shoot-and-Jump chord.
    /// </summary>
    internal static SamusPoseInputMatch MatchNormalJumpAimDownLeftOfRight(GameplayAction held, GameplayAction newlyPressed) =>
        AimedDownJumpOpening(held, newlyPressed) ?? AimedDownJumpClosing(held);

    /// <summary>The right-facing aimed-down jump's chords before its forward Shoot-and-Jump chord; none when no chord holds.</summary>
    private static SamusPoseInputMatch? AimedDownJumpOpening(GameplayAction held, GameplayAction newlyPressed)
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
        return null;
    }

    /// <summary>The right-facing aimed-down jump's chords after its forward Shoot-and-Jump chord.</summary>
    private static SamusPoseInputMatch AimedDownJumpClosing(GameplayAction held)
    {
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
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>
    /// The priority order of an aimed right-to-left ground turn's input list, aimed up or diagonally down (#2557): the turn
    /// keeps <paramref name="turn"/>; the mirrored list has the same order mirrored.
    /// </summary>
    internal static SamusPoseInputMatch MatchAimedTurningRightToLeft(SamusPoseId turn, GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Left))
            return Accept(SamusPoseId.SpinJumpLeftPose, StartJump);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionLeftPose, StartJump);
        if (Has(held, Left))
            return Accept(turn, KeepPose);
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>The priority order of the input list of the left-facing ball pose Draygon holds Samus in.</summary>
    internal static SamusPoseInputMatch MatchDraygonHeldBallLeft(GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Up))
            return Accept(SamusPoseId.DraygonHeldStandingUpLeftPose, StandUpFromDraygonHold);
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
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
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>The priority order of the moonwalk turn jump right input list; the mirrored list has the same order mirrored.</summary>
    internal static SamusPoseInputMatch MatchMoonwalkTurnToSpinJumpRight(GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(held, Jump | Right))
            return Accept(SamusPoseId.SpinJumpRightPose, StartJump);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose, StartJump);
        if (Has(held, Right))
            return Accept(SamusPoseId.MoonwalkTurnToSpinJumpRightPose, KeepPose);
        return SamusPoseInputMatch.Exhausted;
    }

    /// <summary>
    /// The aimed-up and aimed-down moonwalk turn-jump input lists, in both directions: one priority order for an aimed
    /// moonwalk turn jump to the right, continuing <paramref name="aimedTurnJump"/>; the leftward lists are it mirrored.
    /// </summary>
    internal static SamusPoseInputMatch MatchMoonwalkTurnToSpinJumpAimedRight(SamusPoseId aimedTurnJump, GameplayAction held, GameplayAction newlyPressed)
    {
        if (Has(newlyPressed, Jump) && Has(held, Right))
            return Accept(SamusPoseId.SpinJumpRightPose, StartJump);
        if (Has(newlyPressed, Jump))
            return Accept(SamusPoseId.NeutralJumpTransitionRightPose, StartJump);
        if (Has(held, Right))
            return Accept(aimedTurnJump, KeepPose);
        return SamusPoseInputMatch.Exhausted;
    }
}
