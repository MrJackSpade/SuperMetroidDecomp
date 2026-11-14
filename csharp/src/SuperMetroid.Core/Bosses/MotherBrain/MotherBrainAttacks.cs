using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's second-phase attacks: choosing one, the bomb walk, the lasers and the hand beam.</summary>
[RoomLifetime]
internal sealed class MotherBrainAttacks
{
    /// <summary>Provides room services, pictures, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Mother Brain's rainbow beam, its palette, and her death.</summary>
    private readonly MotherBrainRainbowAndDeath _rainbowAndDeath;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainAttacks(EnemyRoomBinding room, MotherBrainRainbowAndDeath rainbowAndDeath)
    {
        _room = room;
        _rainbowAndDeath = rainbowAndDeath;
    }

    /// <summary>Runs the body's current phase, one of four.</summary>
    internal static void RunMotherBrainHandBeamAttack(MotherBrainEnemyState state)
    {
        switch (state.HandBeamPhase)
        {
            case MotherBrainHandBeamPhase.BackUp:
                if (!TryWalkMotherBrainBackwardsForHandBeam(state))
                    return;
                state.LowerNeckMovement = MotherBrainNeckMovement.Raise;
                state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
                state.HandBeamPhase = MotherBrainHandBeamPhase.WaitForBombs;
                return;

            case MotherBrainHandBeamPhase.WaitForBombs:
                // Bombs occupy the same eighteen-slot projectile pool. Waits for
                // every active shell before starting the visually dense recursive beam.
                if (state.BombCounter != 0)
                    return;
                MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.HandBeam);
                state.HandBeamPhase = MotherBrainHandBeamPhase.Firing;
                return;

            case MotherBrainHandBeamPhase.Firing:
                // The charge-beam phase is intentionally empty. The body animation owns the
                // charge effects, projectile emission, 240-frame hold, and phase advance.
                return;

            case MotherBrainHandBeamPhase.Finish:
                MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.NeutralPhase2);
                state.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
                state.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
                state.HandBeamPhase = MotherBrainHandBeamPhase.BackUp;
                state.Phase = MotherBrainBodyPhase.SecondPhaseThinking;
                return;

            default:
                throw new InvalidDataException(
                    $"Mother Brain hand-beam phase {state.HandBeamPhase} is " +
                    "outside the four hand-beam phases.");
        }
    }

    /// <summary>
    /// Walks Mother Brain backwards toward X 40 at the slow gait; the helper's hard floor at X=48 can report arrival
    /// before target 40.
    /// </summary>
    private static bool TryWalkMotherBrainBackwardsForHandBeam(
        MotherBrainEnemyState state)
    {
        int targetMinusBody = CoordinateAxis.Difference(40, state.Body.Position.X.Whole);
        if (targetMinusBody >= 0)
            return true;
        if (state.Pose != MotherBrainBodyPose.Standing)
            return false;
        if (CoordinateAxis.Difference(state.Body.Position.X.Whole, 48) < 0)
            return true;

        MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.WalkBackwardSlow);
        return false;
    }

    /// <summary>Advances the between-attacks delay and chooses when another attack may start.</summary>
    internal void RunMotherBrainSecondPhaseThinking(
        MotherBrainEnemyState state,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles = null)
    {
        if (state.Head.Health == 0)
        {
            StartLiveMotherBrainRainbowBeam(
                state,
                RequireMotherBrainCombatSamus(samus),
                sharedProjectiles);
            return;
        }
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;

        int random = _room.Random.Read();
        if (state.Head.Health >= 4500)
        {
            if (MotherBrainRandomDefinitions.AttacksAtFullStrength.IsTakenBy(random))
            {
                // The attack setup installs the attack sequence and returns. It does not execute
                // phase zero until the body's next enemy-AI call.
                state.Phase = MotherBrainBodyPhase.SecondPhaseTryAttack;
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
            state.Phase = MotherBrainBodyPhase.SecondPhaseTryAttack;
            return;
        }
        // The hand-beam setup installs the ordinary red hand-beam sequence. It does not execute phase
        // zero until the next body AI call; the rainbow beam's sequence is separate and
        // entered only when health reaches zero.
        state.Phase = MotherBrainBodyPhase.SecondPhaseHandBeam;
    }

    /// <summary>
    /// Runs the three-entry body attack choice. The selected head
    /// animation is an ordinary enemy animation, so attacks remain synchronized with its frame
    /// durations instead of running on separate timers.
    /// </summary>
    internal void RunMotherBrainSecondPhaseTryAttack(
        MotherBrainEnemyState state,
        SamusState? samus)
    {
        switch (state.AttackPhase)
        {
            case MotherBrainAttackPhase.ChooseAttack:
                ChooseMotherBrainSecondPhaseAttack(state, RequireMotherBrainCombatSamus(samus));
                return;

            case MotherBrainAttackPhase.Cooldown:
                // The cooldown decrements before testing. The phase-zero initializer always
                // writes 64; a cooldown of zero would run past zero rather than end the attack.
                if (--state.AttackCooldown == 0)
                    state.AttackPhase = MotherBrainAttackPhase.EndAttack;
                return;

            case MotherBrainAttackPhase.EndAttack:
                state.AttackPhase = MotherBrainAttackPhase.ChooseAttack;
                state.Phase = MotherBrainBodyPhase.SecondPhaseThinking;
                return;

            default:
                throw new InvalidDataException(
                    $"Mother Brain attack phase {state.AttackPhase} is outside the " +
                    "three attack phases.");
        }
    }

    /// <summary>Chooses Mother Brain's second-phase attack, whose airborne branches end the whole choice at once.</summary>
    private void ChooseMotherBrainSecondPhaseAttack(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        state.AttackCooldown = 64;
        state.AttackPhase = MotherBrainAttackPhase.Cooldown;

        int random = _room.Random.Read();
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        if (!Enum.IsDefined(movementType))
        {
            throw new InvalidDataException(
                $"Mother Brain attack strategy cannot index Samus movement type {movementType}.");
        }

        if (IsMotherBrainAirAttackMovement(movementType))
        {
            // The helper skips the rest of its caller: every airborne route
            // commits immediately and skips the proximity/fallback thresholds entirely.
            if (MotherBrainRandomDefinitions.AttackPresses.IsTakenBy(random))
            {
                MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.FourOnionRingsPhase2);
                return;
            }

            // The laser branch abandons the strategy helper's caller, then installs and
            // jumps to the quick laser aim. Calling the first body state here reproduces that same-frame
            // tail call rather than introducing an otherwise visible idle frame.
            state.Phase = MotherBrainBodyPhase.SecondPhaseLaserPositionHeadQuickly;
            PositionMotherBrainHeadQuicklyForLaser(state, samus);
            return;
        }

        // Grounded movement types attempt a bomb on the pressing half of the attack roll. An already-live bomb falls back
        // to the attack Samus's distance chooses.
        if (MotherBrainRandomDefinitions.AttackPresses.IsTakenBy(random) && state.BombCounter < 1)
        {
            state.Phase = MotherBrainBodyPhase.SecondPhaseBombDecideWalking;
            DecideMotherBrainBombWalking(state);
            return;
        }

        int verticalDistance = CoordinateAxis.Distance(state.Head.Position.Y.Whole + 4, samus.Position.Y.Whole);
        WeightedRandomChoice<MotherBrainSecondPhaseAttack> attacks = verticalDistance < 32
            ? MotherBrainRandomDefinitions.NearAttack
            : MotherBrainRandomDefinitions.FarAttack;
        MotherBrainSecondPhaseAttack attack = attacks.Choose(random);
        switch (attack)
        {
            case MotherBrainSecondPhaseAttack.Neutral:
                MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.NeutralPhase2);
                return;
            case MotherBrainSecondPhaseAttack.FourOnionRings:
                MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.FourOnionRingsPhase2);
                return;
            case MotherBrainSecondPhaseAttack.Laser:
                state.Phase = MotherBrainBodyPhase.SecondPhaseLaserPositionHeadQuickly;
                PositionMotherBrainHeadQuicklyForLaser(state, samus);
                return;
            case MotherBrainSecondPhaseAttack.Bomb when state.BombCounter >= 1:
                // The bomb branch returns without changing the current head animation when one bomb is
                // already alive. The body still spends the full cooldown in phase one.
                return;
            case MotherBrainSecondPhaseAttack.Bomb:
                state.Phase = MotherBrainBodyPhase.SecondPhaseBombDecideWalking;
                DecideMotherBrainBombWalking(state);
                return;
            default:
                throw new InvalidOperationException($"Mother Brain's attack roll chose the undefined attack {attack}.");
        }
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
            int targetX = MotherBrainRandomDefinitions.BombWalkShortRetreat.IsTakenBy(random) ? 96 : 64;
            int targetMinusBody = CoordinateAxis.Difference(targetX, state.Body.Position.X.Whole);
            if (targetMinusBody < 0)
            {
                state.BodyTargetXPosition = targetX;
                if (!TryWalkMotherBrainBackwards(state, targetX))
                {
                    state.Phase = MotherBrainBodyPhase.SecondPhaseBombWalkingBackwards;
                    return;
                }
            }
        }

        DecideWhetherMotherBrainCrouchesForBomb(state);
    }

    /// <summary>Continues the bomb walk.</summary>
    internal void ContinueMotherBrainBombWalk(MotherBrainEnemyState state)
    {
        if (TryWalkMotherBrainBackwards(state, state.BodyTargetXPosition))
            DecideWhetherMotherBrainCrouchesForBomb(state);
    }

    /// <summary>
    /// Runs the fallthrough. This step advances the global RNG—unlike
    /// attack selection—and fires immediately on the lower half; the upper half enters the
    /// ordinary slow-crouch body animation before firing.
    /// </summary>
    private void DecideWhetherMotherBrainCrouchesForBomb(MotherBrainEnemyState state)
    {
        if (BossRandomRolls.InLowerHalf(_room.Random.Generate()))
        {
            FireMotherBrainBomb(state);
            return;
        }

        state.Phase = MotherBrainBodyPhase.SecondPhaseBombCrouch;
        CrouchMotherBrainForBomb(state);
    }

    /// <summary>Crouches for the bomb.</summary>
    internal static void CrouchMotherBrainForBomb(MotherBrainEnemyState state)
    {
        if (TryMakeMotherBrainCrouch(state))
            FireMotherBrainBomb(state);
    }

    /// <summary>Runs the unstored firing tail.</summary>
    private static void FireMotherBrainBomb(MotherBrainEnemyState state)
    {
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.BombPhase2);
        state.Phase = MotherBrainBodyPhase.SecondPhaseBombFired;
        state.PhaseTimer = 44;
    }

    /// <summary>Waits after the bomb, including its stand-up fallthrough.</summary>
    internal static void WaitAfterMotherBrainBomb(MotherBrainEnemyState state)
    {
        if (--state.PhaseTimer >= 0)
            return;

        if (TryMakeMotherBrainStand(state))
        {
            FinishMotherBrainBombAttack(state);
            return;
        }

        state.Phase = MotherBrainBodyPhase.SecondPhaseBombStandUp;

        // The wait falls directly into the stand-up, so a crouched body calls the helper twice on
        // this frame. Both calls restart the same animation and timer, and the stand-up
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
        state.Phase = MotherBrainBodyPhase.SecondPhaseThinking;

    /// <summary>
    /// Walks Mother Brain backwards toward <paramref name="targetX"/> at the medium gait; the hard floor at X=48 reports
    /// arrival before a target left of it.
    /// </summary>
    private static bool TryWalkMotherBrainBackwards(
        MotherBrainEnemyState state,
        int targetX)
    {
        int targetMinusBody = CoordinateAxis.Difference(targetX, state.Body.Position.X.Whole);
        if (targetMinusBody >= 0)
            return true;
        if (state.Pose != MotherBrainBodyPose.Standing)
            return false;
        if (CoordinateAxis.Difference(state.Body.Position.X.Whole, 48) < 0)
            return true;

        MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.WalkBackwardMedium);
        return false;
    }

    /// <summary>Literal pose/animation behavior of crouching.</summary>
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

    /// <summary>Literal pose/animation behavior of standing.</summary>
    private static bool TryMakeMotherBrainStand(MotherBrainEnemyState state)
    {
        switch (state.Pose)
        {
            case MotherBrainBodyPose.Standing:
                return true;
            case MotherBrainBodyPose.Crouched:
                MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.StandAfterCrouchFast);
                return false;
            case MotherBrainBodyPose.LeaningDown:
                MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.StandAfterLeaning);
                return false;
            case MotherBrainBodyPose.Walking:
            case MotherBrainBodyPose.CrouchingTransition:
            case MotherBrainBodyPose.DeathBeam:
                return false;
            default:
                throw new InvalidOperationException($"Undefined MotherBrainBodyPose {state.Pose}.");
        }
    }

    /// <summary>Requires a live Samus state for combat-only phase logic.</summary>
    internal static SamusState RequireMotherBrainCombatSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Mother Brain phase-two attack selection requires the active Samus actor.");

    /// <summary>
    /// Positions the head quickly for the laser. Both neck joints initially turn by two angle units
    /// per frame toward one of two authored target indices chosen from the head/Samus Y
    /// ordering; the head animation itself is deliberately left untouched here.
    /// </summary>
    internal static void PositionMotherBrainHeadQuicklyForLaser(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        RoomEnemySlot head = state.Head;

        // The comparison reads the wrapped difference as signed. Arena coordinates
        // never leave the signed range, and a wrapped coordinate keeps the same answer.
        int headMinusSamus = CoordinateAxis.Difference(head.Position.Y.Whole, samus.Position.Y.Whole);
        MotherBrainNeckMovement targetMovement = headMinusSamus >= 0
            ? MotherBrainNeckMovement.Raise
            : MotherBrainNeckMovement.Lower;
        state.LowerNeckMovement = targetMovement;
        state.UpperNeckMovement = targetMovement;
        state.NeckAngleDelta = 512;
        state.Phase = MotherBrainBodyPhase.SecondPhaseLaserPositionHeadSlowlyAndFire;
        state.PhaseTimer = 4;
    }

    /// <summary>
    /// Positions the head slowly and fires the laser. Its unsigned timer expires only after decrementing through
    /// zero to -1, at which point neck rotation slows and the ordinary head animation
    /// owns the muzzle animation and projectile spawn.
    /// </summary>
    internal static void PositionMotherBrainHeadSlowlyAndFireLaser(
        MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.NeckAngleDelta = state.NeckAngleDelta >= 0 ? 256 : -256;
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.Laser);
        state.Phase = MotherBrainBodyPhase.SecondPhaseLaserFinishAttack;
        state.PhaseTimer = 16;
    }

    /// <summary>
    /// Finishes the laser attack, including its same-frame jump back into thinking. The attack
    /// phase and cooldown intentionally remain as selected: the laser body state replaces
    /// the attack sequence, so the chain does not pass through that sequence's end phase.
    /// </summary>
    internal void FinishMotherBrainLaserAttack(
        MotherBrainEnemyState state,
        SamusState? samus)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.LowerNeckMovement = MotherBrainNeckMovement.BobUp;
        state.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
        state.Phase = MotherBrainBodyPhase.SecondPhaseThinking;
        RunMotherBrainSecondPhaseThinking(state, samus);
    }

    /// <summary>Advances the chosen walk direction, animation, and footstep cadence.</summary>
    private void HandleMotherBrainWalking(MotherBrainEnemyState state)
    {
        // The backward walk alters only the walk credit and starts an authored body animation;
        // actual displacement remains owned by the body animation, which also keeps the background scroll aligned.
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;
        if (state.WalkCounter.IsEmpty)
        {
            state.WalkCounter = MotherBrainWalkCredit.Of(1);
            if (state.Body.Position.X.Whole >= 48)
            {
                MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.WalkBackwardFast);
                return;
            }
        }
        else
        {
            state.WalkCounter = state.WalkCounter.AddCredit(6);
            if (!state.WalkCounter.BelowStep)
            {
                StartMotherBrainForwardWalk(state);
                return;
            }
            if (state.Body.Position.X.Whole >= 48)
                return;
        }

        // The choice reads the current random number; it does not draw a new one.
        if (MotherBrainForwardWalkDefinitions.ForwardWalk.IsTakenBy(_room.Random.Read()))
            StartMotherBrainForwardWalk(state);
    }

    /// <summary>Starts the body animation and state for a forward step.</summary>
    private static void StartMotherBrainForwardWalk(MotherBrainEnemyState state)
    {
        state.WalkCounter = MotherBrainWalkCredit.Of(128);
        if (state.Body.Position.X.Whole < 128)
            MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, MotherBrainBodySegment.WalkForwardMedium);
    }

    /// <summary>
    /// Executes the attack setup's zero-health tail call into the rainbow beam's setup. Setup happens on the
    /// same body-AI call, but the freshly installed rainbow timer is not decremented until
    /// Mother Brain's next enemy turn.
    /// </summary>
    internal void StartLiveMotherBrainRainbowBeam(
        MotherBrainEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        MotherBrainRainbowAndDeath.SynchronizeLiveMotherBrainRainbowActor(state, sequence);
        // The death-explosion counter keeps the fake-death ascent dust's last value; the death explosions
        // start from it. The sub-phase counter then holds a value that is always
        // negative, so its first DEC takes the same immediate branch as the sequence's zero.
        sequence.InheritDeathAndEscapeExplosionIndex(state.BodySubPhaseTimer);
        sequence.StartAttackCycle();
        state.RainbowBeamSequence = sequence;
        state.NextRainbowFrame = 0;
        state.RainbowAppliedHeadAnimation = null;
        state.RainbowAppliedSmallPurpleBreathWrites = 0;
        _rainbowAndDeath.ApplyLiveMotherBrainRainbowState(
            state,
            sequence,
            step: null,
            samus,
            sharedProjectiles);
    }
}
