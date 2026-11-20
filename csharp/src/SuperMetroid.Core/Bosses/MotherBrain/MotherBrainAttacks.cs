using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's second-phase attacks: choosing one, the bomb walk, the lasers and the hand beam.</summary>
[RoomLifetime]
internal sealed class MotherBrainAttacks
{
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;



    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainAttacks(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>What <c>RunMotherBrainHandBeamAttack</c> does for each <see cref="MotherBrainHandBeamPhase"/> member, each member's own arm.</summary>
    private static IdentityTable<MotherBrainHandBeamPhase, Action<MotherBrainEnemyState>> RunMotherBrainHandBeamAttackForMotherBrainHandBeamPhase => field ??= new(new Dictionary<MotherBrainHandBeamPhase, Action<MotherBrainEnemyState>>
    {
        [MotherBrainHandBeamPhase.BackUp] = static state =>
        {
            // She backs up slowly toward the back of the room; the walk's back limit can report her there first.
            if (!MotherBrainWalks.Backwards(state, MotherBrainFightDefinitions.BackOfRoomX, MotherBrainWalkSpeed.Slow))
                return;
            state.LowerNeckMovement = MotherBrainNeckMovement.Raise;
            state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
            state.HandBeamPhase = MotherBrainHandBeamPhase.WaitForBombs;
        },
        [MotherBrainHandBeamPhase.WaitForBombs] = static state =>
        {
            // Bombs share the room's places for enemy projectiles. Waits for
            // every active shell before starting the visually dense recursive beam.
            if (state.BombCounter != 0)
                return;
            MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.HandBeam);
            state.HandBeamPhase = MotherBrainHandBeamPhase.Firing;
        },
        [MotherBrainHandBeamPhase.Firing] = static state =>
        {
            // The charge-beam phase is intentionally empty. The body animation owns the
            // charge effects, projectile emission, the beam's hold, and phase advance.
        },
        [MotherBrainHandBeamPhase.Finish] = static state =>
        {
            MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.NeutralPhase2);
            state.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
            state.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
            state.HandBeamPhase = MotherBrainHandBeamPhase.BackUp;
            state.Phase2FightPhase = MotherBrainPhase2FightPhase.Thinking;
        },
    });

    /// <summary>Runs the body's current phase, one of four.</summary>
    internal static void RunMotherBrainHandBeamAttack(MotherBrainEnemyState state)
    {
        RunMotherBrainHandBeamAttackForMotherBrainHandBeamPhase[state.HandBeamPhase](state);
    }

    /// <summary>Advances the between-attacks delay and chooses when another attack may start.</summary>
    internal void RunMotherBrainSecondPhaseThinking(MotherBrainEnemyState state)
    {
        if (state.Head.Health == 0)
        {
            StartLiveMotherBrainRainbowBeam(state);
            return;
        }
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;

        int random = _room.Random.Read();
        if (state.Head.Health >= MotherBrainFightDefinitions.FullStrengthHealth)
        {
            if (MotherBrainRandomDefinitions.AttacksAtFullStrength.IsTakenBy(random))
            {
                // Choosing an attack only sets it up; the attack's first step runs on her next update.
                state.Phase2FightPhase = MotherBrainPhase2FightPhase.TryAttack;
                return;
            }
            HandleMotherBrainWalking(state);
            return;
        }

        if (MotherBrainRandomDefinitions.WalksWhenWounded.IsTakenBy(random))
        {
            HandleMotherBrainWalking(state);
            return;
        }
        if (MotherBrainRandomDefinitions.AttacksWhenWounded.IsTakenBy(random))
        {
            state.Phase2FightPhase = MotherBrainPhase2FightPhase.TryAttack;
            return;
        }
        // Choosing the hand beam only sets it up; its first step runs on her next update. The rainbow beam is a
        // separate attack she starts only when her health reaches zero.
        state.Phase2FightPhase = MotherBrainPhase2FightPhase.HandBeam;
    }

    /// <summary>What <c>RunMotherBrainSecondPhaseTryAttack</c> does for each <see cref="MotherBrainAttackPhase"/> member, each member's own arm.</summary>
    private IdentityTable<MotherBrainAttackPhase, Action<MotherBrainEnemyState, SamusState>> RunMotherBrainSecondPhaseTryAttackForMotherBrainAttackPhase => field ??= new(new Dictionary<MotherBrainAttackPhase, Action<MotherBrainEnemyState, SamusState>>
    {
        [MotherBrainAttackPhase.ChooseAttack] = (state, samus) => ChooseMotherBrainSecondPhaseAttack(state, samus),
        [MotherBrainAttackPhase.Cooldown] = (state, samus) =>
        {
            if (state.AttackCooldown.StepRunsOut())
                state.AttackPhase = MotherBrainAttackPhase.EndAttack;
        },
        [MotherBrainAttackPhase.EndAttack] = (state, samus) =>
        {
            state.AttackPhase = MotherBrainAttackPhase.ChooseAttack;
            state.Phase2FightPhase = MotherBrainPhase2FightPhase.Thinking;
        },
    });

    /// <summary>
    /// Runs the body's choice among its three attacks. The selected head
    /// animation is an ordinary enemy animation, so attacks remain synchronized with its frame
    /// durations instead of running on separate timers.
    /// </summary>
    internal void RunMotherBrainSecondPhaseTryAttack(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        RunMotherBrainSecondPhaseTryAttackForMotherBrainAttackPhase[state.AttackPhase](state, samus);
    }

    /// <summary>What <c>ChooseMotherBrainSecondPhaseAttack</c> does for each <see cref="MotherBrainSecondPhaseAttack"/> member, each member's own arm.</summary>
    private IdentityTable<MotherBrainSecondPhaseAttack, Action<MotherBrainEnemyState, SamusState>> ChooseMotherBrainSecondPhaseAttackForMotherBrainSecondPhaseAttack => field ??= new(new Dictionary<MotherBrainSecondPhaseAttack, Action<MotherBrainEnemyState, SamusState>>
    {
        [MotherBrainSecondPhaseAttack.Neutral] = (state, samus) => MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.NeutralPhase2),
        [MotherBrainSecondPhaseAttack.FourOnionRings] = (state, samus) => MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.FourOnionRingsPhase2),
        [MotherBrainSecondPhaseAttack.Laser] = (state, samus) =>
        {
            state.Phase2FightPhase = MotherBrainPhase2FightPhase.LaserPositionHeadQuickly;
            PositionMotherBrainHeadQuicklyForLaser(state, samus);
        },
        [MotherBrainSecondPhaseAttack.Bomb] = (state, samus) =>
        {
            if (state.BombCounter >= 1)
            {
                // A bomb attack with a bomb already alive does nothing, leaving the head's animation as it is. The body
                // still spends the full cooldown in phase one.
            }
            else
            {
                state.Phase2FightPhase = MotherBrainPhase2FightPhase.BombDecideWalking;
                DecideMotherBrainBombWalking(state);
            }
        },
    });

    /// <summary>Chooses Mother Brain's second-phase attack, whose airborne branches end the whole choice at once.</summary>
    private void ChooseMotherBrainSecondPhaseAttack(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        state.AttackCooldown.LastFor(MotherBrainWaitDefinitions.AttackCooldownUpdates);
        state.AttackPhase = MotherBrainAttackPhase.Cooldown;

        int random = _room.Random.Read();
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);

        if (IsMotherBrainAirAttackMovement(movementType))
        {
            // An airborne Samus ends the choice here: every airborne route
            // commits immediately and skips the proximity/fallback thresholds entirely.
            if (MotherBrainRandomDefinitions.AttackPresses.IsTakenBy(random))
            {
                MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.FourOnionRingsPhase2);
                return;
            }

            // The laser branch abandons the rest of the attack choice, then installs and
            // starts the quick laser aim. Running its first step here keeps it in the same
            // frame rather than introducing an otherwise visible idle frame.
            state.Phase2FightPhase = MotherBrainPhase2FightPhase.LaserPositionHeadQuickly;
            PositionMotherBrainHeadQuicklyForLaser(state, samus);
            return;
        }

        // Grounded movement types attempt a bomb on the pressing half of the attack roll. An already-live bomb falls back
        // to the attack Samus's distance chooses.
        if (MotherBrainRandomDefinitions.AttackPresses.IsTakenBy(random) && state.BombCounter < 1)
        {
            state.Phase2FightPhase = MotherBrainPhase2FightPhase.BombDecideWalking;
            DecideMotherBrainBombWalking(state);
            return;
        }

        int verticalDistance = Math.Abs(state.Head.Position.Y.Whole + MotherBrainFightDefinitions.AttackMeasureBelowHead - samus.Position.Y.Whole);
        WeightedRandomChoice<MotherBrainSecondPhaseAttack> attacks = verticalDistance < MotherBrainFightDefinitions.NearAttackHeight
            ? MotherBrainRandomDefinitions.NearAttack
            : MotherBrainRandomDefinitions.FarAttack;
        MotherBrainSecondPhaseAttack attack = attacks.Choose(random);
        ChooseMotherBrainSecondPhaseAttackForMotherBrainSecondPhaseAttack[attack](state, samus);
    }

    /// <summary>Identifies Samus movement types considered airborne by the attack chooser.</summary>
    private static bool IsMotherBrainAirAttackMovement(SamusMovementType movementType) =>
        movementType is
            SamusMovementType.NormalJumping or
            SamusMovementType.SpinJumping or
            SamusMovementType.Falling or
            SamusMovementType.MorphBallFalling or
            SamusMovementType.GlitchBallKnockback or
            SamusMovementType.CompactAirborne or
            SamusMovementType.SpringBallInAir or
            SamusMovementType.SpringBallFalling or
            SamusMovementType.WallJumping;

    /// <summary>Decides bomb walking, including its immediate posture decision.</summary>
    internal void DecideMotherBrainBombWalking(MotherBrainEnemyState state)
    {
        int random = _room.Random.Read();
        if (!MotherBrainRandomDefinitions.BombWalkStaysPut.IsTakenBy(random))
        {
            int targetX = MotherBrainRandomDefinitions.BombWalkShortRetreat.IsTakenBy(random) ? MotherBrainFightDefinitions.BombWalkShortRetreatX : MotherBrainFightDefinitions.BombWalkLongRetreatX;
            int targetMinusBody = targetX - state.Body.Position.X.Whole;
            if (targetMinusBody < 0)
            {
                state.BodyTargetXPosition = targetX;
                if (!MotherBrainWalks.Backwards(state, targetX, MotherBrainWalkSpeed.Medium))
                {
                    state.Phase2FightPhase = MotherBrainPhase2FightPhase.BombWalkingBackwards;
                    return;
                }
            }
        }

        DecideWhetherMotherBrainCrouchesForBomb(state);
    }

    /// <summary>Continues the bomb walk.</summary>
    internal void ContinueMotherBrainBombWalk(MotherBrainEnemyState state)
    {
        if (MotherBrainWalks.Backwards(state, state.BodyTargetXPosition, MotherBrainWalkSpeed.Medium))
            DecideWhetherMotherBrainCrouchesForBomb(state);
    }

    /// <summary>
    /// Decides whether Mother Brain crouches before her bomb. Unlike the attack's choice, this advances the room's random
    /// numbers: a number in the lower half fires at once, one in the upper half crouches slowly first.
    /// </summary>
    private void DecideWhetherMotherBrainCrouchesForBomb(MotherBrainEnemyState state)
    {
        if (BossRandomRolls.InLowerHalf(_room.Random.Generate()))
        {
            FireMotherBrainBomb(state);
            return;
        }

        state.Phase2FightPhase = MotherBrainPhase2FightPhase.BombCrouch;
        CrouchMotherBrainForBomb(state);
    }

    /// <summary>Crouches for the bomb.</summary>
    internal static void CrouchMotherBrainForBomb(MotherBrainEnemyState state)
    {
        if (TryMakeMotherBrainCrouch(state))
            FireMotherBrainBomb(state);
    }

    /// <summary>Fires the bomb from her mouth and starts the wait after it.</summary>
    private static void FireMotherBrainBomb(MotherBrainEnemyState state)
    {
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.BombPhase2);
        state.Phase2FightPhase = MotherBrainPhase2FightPhase.BombFired;
        state.PhaseTimer.LastFor(MotherBrainFightDefinitions.AfterBombUpdates);
    }

    /// <summary>Waits after the bomb, then stands her up in the update the wait ends.</summary>
    internal static void WaitAfterMotherBrainBomb(MotherBrainEnemyState state)
    {
        if (state.PhaseTimer.StepKeepsRunning())
            return;

        if (TryMakeMotherBrainStand(state))
        {
            FinishMotherBrainBombAttack(state);
            return;
        }

        state.Phase2FightPhase = MotherBrainPhase2FightPhase.BombStandUp;

        // The wait falls directly into the stand-up, so a crouched body starts standing twice on
        // this frame. Both starts restart the same animation and timer, and the stand-up
        // follows the wait in the same frame rather than a frame later.
        StandMotherBrainAfterBomb(state);
    }

    /// <summary>Stands after the bomb.</summary>
    internal static void StandMotherBrainAfterBomb(MotherBrainEnemyState state)
    {
        if (TryMakeMotherBrainStand(state))
            FinishMotherBrainBombAttack(state);
    }

    /// <summary>Returns the body and head to their neutral thinking phase after a bomb.</summary>
    private static void FinishMotherBrainBombAttack(MotherBrainEnemyState state) =>
        state.Phase2FightPhase = MotherBrainPhase2FightPhase.Thinking;

    /// <summary>Whether Mother Brain is crouched; a standing Mother Brain starts crouching slowly.</summary>
    private static bool TryMakeMotherBrainCrouch(MotherBrainEnemyState state)
    {
        if (state.Pose == MotherBrainBodyPose.Crouched)
            return true;
        if (state.Pose == MotherBrainBodyPose.Standing)
            // The static crouched animation is only appropriate during ascent initialization.
            // Combat must first lower the body so the subsequent stand reverses that move.
            MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.CrouchSlow);
        return false;
    }

    /// <summary>What <c>TryMakeMotherBrainStand</c> comes to for each <see cref="MotherBrainBodyPose"/> member, each member's own arm.</summary>
    private static IdentityTable<MotherBrainBodyPose, Func<MotherBrainEnemyState, bool>> TryMakeMotherBrainStandForMotherBrainBodyPose => field ??= new(new Dictionary<MotherBrainBodyPose, Func<MotherBrainEnemyState, bool>>
    {
        [MotherBrainBodyPose.Standing] = static state => true,
        [MotherBrainBodyPose.Crouched] = static state =>
        {
            MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.StandAfterCrouchFast);
            return false;
        },
        [MotherBrainBodyPose.LeaningDown] = static state =>
        {
            MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.StandAfterLeaning);
            return false;
        },
        [MotherBrainBodyPose.Walking] = static state => false,
        [MotherBrainBodyPose.CrouchingTransition] = static state => false,
        [MotherBrainBodyPose.DeathBeam] = static state => false,
    });

    /// <summary>Whether Mother Brain is standing; a crouched or leaning Mother Brain starts standing up.</summary>
    private static bool TryMakeMotherBrainStand(MotherBrainEnemyState state)
    {
        return TryMakeMotherBrainStandForMotherBrainBodyPose[state.Pose](state);
    }

    /// <summary>
    /// Positions the head quickly for the laser. Both neck joints initially turn at the quick neck turn
    /// (<see cref="MotherBrainNeckTurnDefinitions.Quick"/>) toward one of two authored target angles chosen from the head/Samus Y
    /// ordering; the head animation itself is deliberately left untouched here.
    /// </summary>
    internal static void PositionMotherBrainHeadQuicklyForLaser(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        RoomEnemyPlace head = state.Head;

        MotherBrainNeckMovement targetMovement = head.Position.Y.Whole >= samus.Position.Y.Whole
            ? MotherBrainNeckMovement.Raise
            : MotherBrainNeckMovement.Lower;
        state.LowerNeckMovement = targetMovement;
        state.UpperNeckMovement = targetMovement;
        state.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Quick;
        state.Phase2FightPhase = MotherBrainPhase2FightPhase.LaserPositionHeadSlowlyAndFire;
        state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.LaserQuickAimUpdates);
    }

    /// <summary>
    /// Positions the head slowly and fires the laser. As its timer runs out, neck rotation slows and the ordinary head animation
    /// owns the muzzle animation and projectile spawn.
    /// </summary>
    internal static void PositionMotherBrainHeadSlowlyAndFireLaser(
        MotherBrainEnemyState state)
    {
        if (!state.PhaseTimer.StepRunsOut())
            return;

        state.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Steady;
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.Laser);
        state.Phase2FightPhase = MotherBrainPhase2FightPhase.LaserFinishAttack;
        state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.LaserRecoveryUpdates);
    }

    /// <summary>
    /// Finishes the laser attack, including its same-frame jump back into thinking. The attack
    /// phase and cooldown intentionally remain as selected: the laser body state replaces
    /// the attack sequence, so the chain does not pass through that sequence's end phase.
    /// </summary>
    internal void FinishMotherBrainLaserAttack(MotherBrainEnemyState state)
    {
        if (!state.PhaseTimer.StepRunsOut())
            return;

        state.LowerNeckMovement = MotherBrainNeckMovement.BobUp;
        state.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
        state.Phase2FightPhase = MotherBrainPhase2FightPhase.Thinking;
        RunMotherBrainSecondPhaseThinking(state);
    }

    /// <summary>Advances the chosen walk direction, animation, and footstep cadence.</summary>
    private void HandleMotherBrainWalking(MotherBrainEnemyState state)
    {
        // The backward walk alters only the walk credit and starts an authored body animation;
        // the body animation moves her, and her body's pictures follow her.
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;
        if (state.WalkCounter.IsEmpty)
        {
            state.WalkCounter = MotherBrainWalkCredit.Of(MotherBrainFightDefinitions.SecondPhaseStartingCredit);
            if (state.Body.Position.X.Whole >= MotherBrainFightDefinitions.BackLimitX)
            {
                MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.WalkBackwardFast);
                return;
            }
        }
        else
        {
            state.WalkCounter = state.WalkCounter.AddCredit(MotherBrainFightDefinitions.SecondPhaseCreditPerTurn);
            if (!state.WalkCounter.BelowStep)
            {
                StartMotherBrainForwardWalk(state);
                return;
            }
            if (state.Body.Position.X.Whole >= MotherBrainFightDefinitions.BackLimitX)
                return;
        }

        // The choice reads the current random number; it does not draw a new one.
        if (MotherBrainForwardWalkDefinitions.ForwardWalk.IsTakenBy(_room.Random.Read()))
            StartMotherBrainForwardWalk(state);
    }

    /// <summary>Starts the body animation and state for a forward step.</summary>
    private static void StartMotherBrainForwardWalk(MotherBrainEnemyState state)
    {
        state.WalkCounter = MotherBrainWalkCredit.Of(MotherBrainFightDefinitions.AfterStepCredit);
        if (state.Body.Position.X.Whole < MotherBrainFightDefinitions.FrontLimitX)
            MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.WalkForwardMedium);
    }

    /// <summary>
    /// Goes straight on from the attack setup, at zero health, into the rainbow beam's setup in the same update; the
    /// beam's timer first counts down on her next update.
    /// </summary>
    private static void StartLiveMotherBrainRainbowBeam(MotherBrainEnemyState state)
    {
        state.BeginLateFight().StartAttackCycle();
        state.NextRainbowImage = MotherBrainRainbowImage.CycleStart;
    }
}
