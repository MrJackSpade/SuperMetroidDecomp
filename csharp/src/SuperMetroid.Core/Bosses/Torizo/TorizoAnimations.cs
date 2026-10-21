using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodyProgram;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed body programs (#641), in place of their bank-$AA lists. Generated from the verified
/// native lists: each frame shows a <see cref="TorizoFrame"/> pose, each callback becomes a <see cref="TorizoCue"/>
/// with its typed operands, and each tile transfer names its <see cref="TorizoInstructionVramTransferDefinitions"/>
/// entry. A callback that sleeps for a fixed time is its cue followed by a wait of that length.
/// </summary>
internal static class TorizoAnimations
{
    /// <summary>Every body program.</summary>
    internal static AnimationProgramSet<TorizoBodyProgram, TorizoFrame, TorizoCue> Programs { get; } = new(
        new Dictionary<TorizoBodyProgram, AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>[]>(
            [.. TorizoEncounterPrograms.Programs, .. TorizoFacingLeftPrograms.Programs, .. TorizoFacingRightPrograms.Programs,
             .. GoldenTorizoPrograms.Programs]));

    /// <summary>
    /// The program a sleeping list resumes into when the AI steps its pointer past the sleep, as Bomb Torizo's wake
    /// and Golden Torizo's trigger do.
    /// </summary>
    internal static TorizoBodyProgram AfterSleep(TorizoBodyProgram program) => program switch
    {
        P.BombTorizoInitial0
            or P.FacingLeftFacelessWalkingLeftLegMovingTrailing
            or P.GoldenInitial0 => EnumOrder.Next(program),
        P.SpecialCallableBlowUpBombTorizosGut or
            P.CallableBlowUpBombTorizosFace or
            P.DeathSequence0 or
            P.DeathSequence1 or
            P.DeathSequence2 or
            P.BombTorizoInitial0Resumed or
            P.BombTorizoInitial1 or
            P.BombTorizoInitial2 or
            P.FacingLeftTurningLeft or
            P.FacingLeftWalkingRightLegMoving or
            P.FacingLeftWalkingLeftLegMoving or
            P.FacingLeftSpewingChozoOrbsRightFootFwd0 or
            P.FacingLeftSpewingChozoOrbsRightFootFwd1 or
            P.FacingLeftSpewingChozoOrbsLeftFootFwd0 or
            P.FacingLeftSpewingChozoOrbsLeftFootFwd1 or
            P.FacingLeftSonicBoomsRightFootForward0 or
            P.FacingLeftSonicBoomsRightFootForward1 or
            P.FacingLeftSonicBoomsLeftFootForward0 or
            P.FacingLeftSonicBoomsLeftFootForward1 or
            P.FacingLeftExplosiveSwipeRightFootForward or
            P.FacingLeftExplosiveSwipeLeftFootForward or
            P.FacingLeftJumpingForwards0 or
            P.FacingLeftJumpingForwards1 or
            P.FacingLeftFalling0 or
            P.FacingLeftFalling1 or
            P.FacingLeftFalling2 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd0 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd1 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd2 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd3 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd4 or
            P.FacingLeftJumpingBackwardRightFootFwd0 or
            P.FacingLeftJumpingBackwardRightFootFwd1 or
            P.FacingLeftJumpingBackwardRightFootFwd2 or
            P.FacingLeftJumpingBackwardRightFootFwd3 or
            P.FacingLeftJumpingBackwardRightFootFwd4 or
            P.FacingLeftFacelessTurningLeft or
            P.FacingLeftFacelessWalkingRightLegMoving or
            P.FacingLeftFacelessWalkingLeftLegMoving or
            P.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed or
            P.FacingRightTurningRight or
            P.FacingRightWalkingLeftLegMoving or
            P.FacingRightWalkingRightLegMoving or
            P.FacingRightSpewingChozoOrbsLeftFootFwd0 or
            P.FacingRightSpewingChozoOrbsLeftFootFwd1 or
            P.FacingRightSpewingChozoOrbsRightFootFwd0 or
            P.FacingRightSpewingChozoOrbsRightFootFwd1 or
            P.FacingRightSonicBoomsLeftFootForward0 or
            P.FacingRightSonicBoomsLeftFootForward1 or
            P.FacingRightSonicBoomsRightFootForward0 or
            P.FacingRightSonicBoomsRightFootForward1 or
            P.FacingRightExplosiveSwipeLeftFootForward or
            P.FacingRightExplosiveSwipeRightFootForward or
            P.FacingRightJumpingForwards0 or
            P.FacingRightJumpingForwards1 or
            P.FacingRightFalling0 or
            P.FacingRightFalling1 or
            P.FacingRightFalling2 or
            P.FacingRightJumpBackwardLandRightFootFwd0 or
            P.FacingRightJumpBackwardLandRightFootFwd1 or
            P.FacingRightJumpBackwardLandRightFootFwd2 or
            P.FacingRightJumpBackwardLandRightFootFwd3 or
            P.FacingRightJumpBackwardLandRightFootFwd4 or
            P.FacingRightJumpBackwardsLandLeftFootFwd0 or
            P.FacingRightJumpBackwardsLandLeftFootFwd1 or
            P.FacingRightJumpBackwardsLandLeftFootFwd2 or
            P.FacingRightJumpBackwardsLandLeftFootFwd3 or
            P.FacingRightJumpBackwardsLandLeftFootFwd4 or
            P.FacingRightFacelessTurningRight or
            P.FacingRightFacelessWalkingLeftLegMoving or
            P.FacingRightFacelessWalkingRightLegMoving or
            P.GoldenInitial0Resumed or
            P.GoldenInitial1 or
            P.GoldenInitial2 or
            P.GoldenInitial3 or
            P.GoldenSpewChozoOrbsFaceLeftRightFootFwd0 or
            P.GoldenSpewChozoOrbsFaceLeftRightFootFwd1 or
            P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0 or
            P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1 or
            P.GoldenSonicBoomsFacingLeftRightFootFwd0 or
            P.GoldenSonicBoomsFacingLeftRightFootFwd1 or
            P.GoldenSonicBoomsFacingLeftLeftFootFwd0 or
            P.GoldenSonicBoomsFacingLeftLeftFootFwd1 or
            P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0 or
            P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1 or
            P.GoldenSpewChozoOrbFacingLeftRightFootFwd0 or
            P.GoldenSpewChozoOrbFacingLeftRightFootFwd1 or
            P.GoldenSonicBoomsFacingRightLeftFootFwd0 or
            P.GoldenSonicBoomsFacingRightLeftFootFwd1 or
            P.GoldenSonicBoomsFacingRightRightFootFwd0 or
            P.GoldenSonicBoomsFacingRightRightFootFwd1 or
            P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd or
            P.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd or
            P.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd or
            P.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd or
            P.GoldenCaughtSuperFacingLeftLeftLegFwd or
            P.GoldenCaughtSuperFacingLeftRightLegFwd or
            P.GoldenCaughtSuperFacingRightRightLegFwd or
            P.GoldenCaughtSuperFacingRightLeftLegFwd or
            P.GoldenSitDownAttackFacingLeft or
            P.GoldenSitDownAttackFacingRight or
            P.GoldenReleaseGoldenTorizoEggs0 or
            P.GoldenReleaseGoldenTorizoEggs1 or
            P.GoldenReleaseGoldenTorizoEggs2 or
            P.GoldenEyeBeamAttack0 or
            P.GoldenEyeBeamAttack1 or
            P.GoldenEyeBeamAttack2 or
            P.Stunned0 or
            P.Stunned1 or
            P.GoldenDodgeTurningLeft or
            P.GoldenTurningLeft or
            P.GoldenWalkingLeftRightLegMoving or
            P.GoldenWalkingLeftLeftLegMoving or
            P.GoldenDodgeTurningRight or
            P.GoldenTurningRight or
            P.GoldenWalkingRightLeftLegMoving or
            P.GoldenWalkingRightRightLegMoving => throw new ArgumentOutOfRangeException(nameof(program), program, "The program does not sleep before authored code."),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "The program does not sleep before authored code."),
    };
}
