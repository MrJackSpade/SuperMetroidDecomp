using SuperMetroid.Core.Bosses.Artwork;
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
    /// <summary>Provides room scratch memory, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Mother Brain's rainbow beam, its palette, and her death.</summary>
    private readonly MotherBrainRainbowAndDeath _rainbowAndDeath;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainAttacks(EnemyRoomBinding room, MotherBrainRainbowAndDeath rainbowAndDeath, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _room = room;
        _rainbowAndDeath = rainbowAndDeath;
    }

    /// <summary>Ports the four-entry body dispatcher at <c>$A9:B87D-B8EA</c>.</summary>
    internal static void RunMotherBrainHandBeamAttack(MotherBrainEnemyState state)
    {
        switch (state.HandBeamPhase)
        {
            case MotherBrainHandBeamPhase.BackUp:
                if (!TryWalkMotherBrainBackwardsForHandBeam(state))
                    return;
                state.LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
                state.UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
                state.HandBeamPhase = MotherBrainHandBeamPhase.WaitForBombs;
                return;

            case MotherBrainHandBeamPhase.WaitForBombs:
                // Bombs occupy the same eighteen-slot projectile pool. Native waits for
                // every active shell before starting the visually dense recursive beam.
                if (state.BombCounter != 0)
                    return;
                MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, MotherBrainBodyProgram.HandBeam);
                state.HandBeamPhase = MotherBrainHandBeamPhase.Firing;
                return;

            case MotherBrainHandBeamPhase.Firing:
                // `$A9:B8C8` is intentionally just RTS. Body bytecode at $9A42 owns the
                // charge effects, projectile emission, 240-frame hold, and phase advance.
                return;

            case MotherBrainHandBeamPhase.Finish:
                MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.NeutralPhase2);
                state.LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
                state.UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
                state.HandBeamPhase = MotherBrainHandBeamPhase.BackUp;
                state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
                return;

            default:
                throw new InvalidDataException(
                    $"Mother Brain hand-beam phase {(ushort)state.HandBeamPhase} is " +
                    "outside the four-entry cartridge dispatcher.");
        }
    }

    /// <summary>
    /// Literal <c>MakeMotherBrainWalkBackwards($28, $08)</c>. Offset eight selects the slow
    /// authored gait; the helper's hard floor at X=$30 can report arrival before target $28.
    /// </summary>
    private static bool TryWalkMotherBrainBackwardsForHandBeam(
        MotherBrainEnemyState state)
    {
        ushort targetMinusBody = unchecked((ushort)(40 - state.Body.Position.X.Whole));
        if (unchecked((short)targetMinusBody) >= 0)
            return true;
        if (state.Pose != MotherBrainBodyPose.Standing)
            return false;
        if (unchecked((short)(state.Body.Position.X.Whole - 48)) < 0)
            return true;

        MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, MotherBrainBodyProgram.WalkBackwardSlow);
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
            if (random < 4096)
            {
                // `$B605` installs the attack dispatcher and returns. It does not execute
                // phase zero until the body's next enemy-AI call.
                state.Function = MotherBrainBodyFunction.SecondPhaseTryAttack;
                return;
            }
            HandleMotherBrainWalking(state);
            return;
        }

        if (random < 8192)
        {
            HandleMotherBrainWalking(state);
            return;
        }
        if (random >= 40960)
        {
            state.Function = MotherBrainBodyFunction.SecondPhaseTryAttack;
            return;
        }
        // `$B644` installs the ordinary red hand-beam dispatcher. It does not execute phase
        // zero until the next body AI call; `$B8F1` belongs to the separate rainbow beam
        // entered only when health reaches zero.
        state.Function = MotherBrainBodyFunction.SecondPhaseHandBeam;
    }

    /// <summary>
    /// Ports the three-entry body dispatcher at <c>$A9:B64B-B780</c>. The selected head
    /// list is ordinary enemy bytecode, so attacks remain synchronized with native map
    /// durations instead of being represented by host timers.
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
                // `$B764` decrements before testing. The phase-zero initializer always
                // writes $40, but unchecked arithmetic retains the native zero-underflow
                // behavior if a debugger edits this word while stopped here.
                state.AttackCooldown = state.AttackCooldown.Decrement().Remaining;
                if (state.AttackCooldown.IsZero)
                    state.AttackPhase = MotherBrainAttackPhase.EndAttack;
                return;

            case MotherBrainAttackPhase.EndAttack:
                state.AttackPhase = MotherBrainAttackPhase.ChooseAttack;
                state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
                return;

            default:
                throw new InvalidDataException(
                    $"Mother Brain attack phase {(ushort)state.AttackPhase} is outside the " +
                    "three-entry cartridge dispatcher.");
        }
    }

    /// <summary>Ports <c>$A9:B65A-B72B</c>, including its unusual stack-return strategy.</summary>
    private void ChooseMotherBrainSecondPhaseAttack(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        state.AttackCooldown = WrappingCountdown.Start(64);
        state.AttackPhase = MotherBrainAttackPhase.Cooldown;

        byte randomLow = unchecked((byte)_room.Random.Read());
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        if ((byte)movementType >= 28)
        {
            throw new InvalidDataException(
                $"Mother Brain attack strategy cannot index Samus movement type ${(byte)movementType:X2}.");
        }

        if (IsMotherBrainAirAttackMovement(movementType))
        {
            // The native helper removes its caller's return address: every airborne route
            // commits immediately and skips the proximity/fallback thresholds entirely.
            if (randomLow >= 128)
            {
                MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.FourOnionRingsPhase2);
                return;
            }

            // `$B702` discards the strategy helper's return address, then installs and
            // jumps to `$B80E`. Calling the first body state here reproduces that same-frame
            // tail call rather than introducing an otherwise visible idle frame.
            state.Function = MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly;
            PositionMotherBrainHeadQuicklyForLaser(state, samus);
            return;
        }

        // Grounded movement types attempt a bomb on the upper half of the current random
        // byte. An already-live bomb falls back to the normal threshold table.
        if (randomLow >= 128 && state.BombCounter < 1)
        {
            state.Function = MotherBrainBodyFunction.SecondPhaseBombDecideWalking;
            DecideMotherBrainBombWalking(state);
            return;
        }

        ushort verticalDistance = EnemyMath.WrappedMagnitude(unchecked((ushort)(
            state.Head.Position.Y.Whole + 4 - samus.Position.Y.Whole)));
        ReadOnlySpan<byte> thresholds = verticalDistance < 32
            ? [16, 32, 208]
            : [64, 128, 192];
        int choice = randomLow < thresholds[0]
            ? 0
            : randomLow < thresholds[1]
                ? 1
                : randomLow < thresholds[2]
                    ? 2
                    : 3;

        switch (choice)
        {
            case 0:
                MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.NeutralPhase2);
                return;
            case 1:
                MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.FourOnionRingsPhase2);
                return;
            case 2:
                state.Function = MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly;
                PositionMotherBrainHeadQuicklyForLaser(state, samus);
                return;
            case 3 when state.BombCounter >= 1:
                // `$B6B9` returns without changing the current head list when one bomb is
                // already alive. The body still spends the full cooldown in phase one.
                return;
            default:
                state.Function = MotherBrainBodyFunction.SecondPhaseBombDecideWalking;
                DecideMotherBrainBombWalking(state);
                return;
        }
    }

    /// <summary>Identifies Samus movement types considered airborne by the attack chooser.</summary>
    private static bool IsMotherBrainAirAttackMovement(SamusMovementType movementType) =>
        movementType is
            SamusMovementType.NormalJumping or
            SamusMovementType.SpinJumping or
            SamusMovementType.Falling or
            SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or
            SamusMovementType.Unused0B or
            SamusMovementType.Unused0C or
            SamusMovementType.Unused0D or
            SamusMovementType.SpringBallInAir or
            SamusMovementType.SpringBallFalling or
            SamusMovementType.WallJumping;

    /// <summary>Ports <c>$A9:B781-B7AB</c>, including its immediate posture decision.</summary>
    internal void DecideMotherBrainBombWalking(MotherBrainEnemyState state)
    {
        int random = _room.Random.Read();
        if (random < 65408)
        {
            ushort targetX = random >= 24576 ? (ushort)64 : (ushort)96;
            ushort targetMinusBody = unchecked((ushort)(targetX - state.Body.Position.X.Whole));
            if (unchecked((short)targetMinusBody) < 0)
            {
                state.BodyTargetXPosition = targetX;
                if (!TryWalkMotherBrainBackwards(state, targetX))
                {
                    state.Function = MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards;
                    return;
                }
            }
        }

        DecideWhetherMotherBrainCrouchesForBomb(state);
    }

    /// <summary>Ports stored body state <c>$A9:B7AC</c>.</summary>
    internal void ContinueMotherBrainBombWalk(MotherBrainEnemyState state)
    {
        if (TryWalkMotherBrainBackwards(state, state.BodyTargetXPosition))
            DecideWhetherMotherBrainCrouchesForBomb(state);
    }

    /// <summary>
    /// Ports the fallthrough at <c>$A9:B7B7</c>. This routine advances the global RNG—unlike
    /// attack selection—and fires immediately on the lower half; the upper half enters the
    /// ordinary slow-crouch body list before firing.
    /// </summary>
    private void DecideWhetherMotherBrainCrouchesForBomb(MotherBrainEnemyState state)
    {
        if (_room.Random.Generate() < 32768)
        {
            FireMotherBrainBomb(state);
            return;
        }

        state.Function = MotherBrainBodyFunction.SecondPhaseBombCrouch;
        CrouchMotherBrainForBomb(state);
    }

    /// <summary>Ports stored body state <c>$A9:B7C6</c>.</summary>
    internal static void CrouchMotherBrainForBomb(MotherBrainEnemyState state)
    {
        if (TryMakeMotherBrainCrouch(state))
            FireMotherBrainBomb(state);
    }

    /// <summary>Ports the unstored firing tail at <c>$A9:B7CB</c>.</summary>
    private static void FireMotherBrainBomb(MotherBrainEnemyState state)
    {
        MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.BombPhase2);
        state.Function = MotherBrainBodyFunction.SecondPhaseBombFired;
        state.FunctionTimer = WrappingCountdown.Start(44);
    }

    /// <summary>Ports stored body state <c>$A9:B7E8</c> and its stand-up fallthrough.</summary>
    internal static void WaitAfterMotherBrainBomb(MotherBrainEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (!state.FunctionTimer.IsNegative)
            return;

        if (TryMakeMotherBrainStand(state))
        {
            FinishMotherBrainBombAttack(state);
            return;
        }

        state.Function = MotherBrainBodyFunction.SecondPhaseBombStandUp;

        // `$B7F2` falls directly into `$B7F8`, so a crouched body calls the helper twice on
        // this frame. Reinstalling the same list/timer is observable to a debugger and must
        // not be collapsed into a host-only transition delay.
        StandMotherBrainAfterBomb(state);
    }

    /// <summary>Ports stored body state <c>$A9:B7F8</c>.</summary>
    internal static void StandMotherBrainAfterBomb(MotherBrainEnemyState state)
    {
        if (TryMakeMotherBrainStand(state))
            FinishMotherBrainBombAttack(state);
    }

    /// <summary>Returns the body and head to their neutral thinking phase after a bomb.</summary>
    private static void FinishMotherBrainBombAttack(MotherBrainEnemyState state) =>
        state.Function = MotherBrainBodyFunction.SecondPhaseThinking;

    /// <summary>Literal target/pose/list behavior of <c>$A9:C647</c> at delay-table offset six.</summary>
    private static bool TryWalkMotherBrainBackwards(
        MotherBrainEnemyState state,
        int targetX)
    {
        ushort targetMinusBody = unchecked((ushort)(targetX - state.Body.Position.X.Whole));
        if (unchecked((short)targetMinusBody) >= 0)
            return true;
        if (state.Pose != MotherBrainBodyPose.Standing)
            return false;
        if (unchecked((short)(state.Body.Position.X.Whole - 48)) < 0)
            return true;

        MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, MotherBrainBodyProgram.WalkBackwardMedium);
        return false;
    }

    /// <summary>Literal pose/list behavior of <c>$A9:C68E</c>.</summary>
    private static bool TryMakeMotherBrainCrouch(MotherBrainEnemyState state)
    {
        if (state.Pose == MotherBrainBodyPose.Crouched)
            return true;
        if (state.Pose == MotherBrainBodyPose.Standing)
            // The static crouched list is only appropriate during ascent initialization.
            // Combat must first lower the body so the subsequent stand reverses that move.
            MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, MotherBrainBodyProgram.CrouchSlow);
        return false;
    }

    /// <summary>Literal pose/list behavior of <c>$A9:C670</c>.</summary>
    private static bool TryMakeMotherBrainStand(MotherBrainEnemyState state)
    {
        switch (state.Pose)
        {
            case MotherBrainBodyPose.Standing:
                return true;
            case MotherBrainBodyPose.Crouched:
                MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, MotherBrainBodyProgram.StandAfterCrouchFast);
                return false;
            case MotherBrainBodyPose.LeaningDown:
                MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, MotherBrainBodyProgram.StandAfterLeaning);
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
    /// Ports body state <c>$A9:B80E</c>. Both neck joints initially turn by two angle units
    /// per frame toward one of two authored target indices chosen from the head/Samus Y
    /// ordering; the head instruction list itself is deliberately left untouched here.
    /// </summary>
    internal static void PositionMotherBrainHeadQuicklyForLaser(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        RoomEnemySlot head = state.Head;

        // CMP/BPL tests the sign bit of the wrapped 16-bit subtraction. Normal arena
        // coordinates cannot overflow signed range, but expressing the native flag test
        // explicitly keeps debugger-edited/wrapped coordinates cartridge-faithful too.
        ushort headMinusSamus = unchecked((ushort)(head.Position.Y.Whole - samus.Position.Y.Whole));
        MotherBrainNeckMovement targetIndex = unchecked((short)headMinusSamus) >= 0
            ? MotherBrainNeckMovement.Raise
            : MotherBrainNeckMovement.Lower;
        state.LowerNeckMovementIndex = targetIndex;
        state.UpperNeckMovementIndex = targetIndex;
        state.NeckAngleDelta = 512;
        state.Function = MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire;
        state.FunctionTimer = WrappingCountdown.Start(4);
    }

    /// <summary>
    /// Ports <c>$A9:B839</c>. Its unsigned timer expires only after decrementing through
    /// zero to <c>$FFFF</c>, at which point neck rotation slows and ordinary head bytecode
    /// owns the muzzle animation and projectile spawn.
    /// </summary>
    internal static void PositionMotherBrainHeadSlowlyAndFireLaser(
        MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.NeckAngleDelta = state.NeckAngleDelta >= 0 ? 256 : -256;
        MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.Laser);
        state.Function = MotherBrainBodyFunction.SecondPhaseLaserFinishAttack;
        state.FunctionTimer = WrappingCountdown.Start(16);
    }

    /// <summary>
    /// Ports <c>$A9:B863</c>, including its same-frame jump back into thinking. The attack
    /// phase/cooldown words intentionally remain as selected: the laser body state replaces
    /// `$B64B`, so the native chain does not pass through that dispatcher's end phase.
    /// </summary>
    internal void FinishMotherBrainLaserAttack(
        MotherBrainEnemyState state,
        SamusState? samus)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.LowerNeckMovementIndex = MotherBrainNeckMovement.BobUp;
        state.UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
        state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
        RunMotherBrainSecondPhaseThinking(state, samus);
    }

    /// <summary>Advances the chosen walk direction, animation, and footstep cadence.</summary>
    private void HandleMotherBrainWalking(MotherBrainEnemyState state)
    {
        // `$A9:C6B8` alters only the walk accumulator and installs a ROM-authored body list;
        // actual displacement remains bytecode-owned, which also keeps BG2 scroll aligned.
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;
        if (state.WalkCounter.IsEmpty)
        {
            state.WalkCounter = MotherBrainWalkCredit.Of(1);
            if (state.Body.Position.X.Whole >= 48)
            {
                MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, MotherBrainBodyProgram.WalkBackwardFast);
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

        // The cartridge samples the current RNG word; it does not advance the generator.
        if ((_room.Random.Read() & MotherBrainForwardWalkDefinitions.ForwardWalkRandomMask) >=
            MotherBrainForwardWalkDefinitions.ForwardWalkRandomThreshold)
            StartMotherBrainForwardWalk(state);
    }

    /// <summary>Starts the body animation and state for a forward step.</summary>
    private static void StartMotherBrainForwardWalk(MotherBrainEnemyState state)
    {
        state.WalkCounter = MotherBrainWalkCredit.Of(128);
        if (state.Body.Position.X.Whole < 128)
            MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, MotherBrainBodyProgram.WalkForwardMedium);
    }

    /// <summary>
    /// Executes `$A9:B605-$B610`'s zero-health tail call into `$B8EB`. Setup happens on the
    /// same body-AI call, but the freshly installed `$B91A` timer is not decremented until
    /// Mother Brain's next enemy turn.
    /// </summary>
    internal void StartLiveMotherBrainRainbowBeam(
        MotherBrainEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        var sequence = new MotherBrainRainbowBeamAttackSequence(_bossArtwork.MotherBrainCorpse, _room.Staging);
        MotherBrainRainbowAndDeath.SynchronizeLiveMotherBrainRainbowActor(state, sequence);
        // Word $0FF2 keeps the fake-death ascent dust's last value; the death explosions
        // start from it. Word $0FF0 then holds a bank-$A9 sub-function pointer, always
        // negative, so its first DEC takes the same immediate branch as the sequence's zero.
        sequence.InheritDeathAndEscapeExplosionIndex(state.BodySubFunctionTimer.RawValue);
        sequence.StartAttackCycle();
        state.RainbowBeamSequence = sequence;
        state.RainbowPaletteCursor = 0;
        state.RainbowAppliedHeadProgram = null;
        state.RainbowAppliedSmallPurpleBreathWrites = 0;
        _rainbowAndDeath.ApplyLiveMotherBrainRainbowState(
            state,
            sequence,
            step: null,
            samus,
            sharedProjectiles);
    }
}
