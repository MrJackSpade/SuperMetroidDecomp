namespace SuperMetroid.Core.Game;

/// <summary>
/// Live-room adapter for Mother Brain's long rainbow-beam state machine. The underlying
/// sequence is also used by the exhaustive standalone verifier; this file supplies only the
/// cartridge ownership boundaries that exist in a loaded room: physical enemy slots own
/// ordinary body/head bytecode, the shared Samus projectile object owns cooldown `$0CCC`,
/// and the room enemy system owns global earthquake and sound requests.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Executes `$A9:B605-$B610`'s zero-health tail call into `$B8EB`. Setup happens on the
    /// same body-AI call, but the freshly installed `$B91A` timer is not decremented until
    /// Mother Brain's next enemy turn.
    /// </summary>
    private void StartLiveMotherBrainRainbowBeam(
        MotherBrainEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        SynchronizeLiveMotherBrainRainbowActor(state, sequence);
        sequence.StartAttackCycle();
        state.RainbowBeamSequence = sequence;
        state.RainbowAppliedHeadInstructionList = 0;
        ApplyLiveMotherBrainRainbowState(
            state,
            sequence,
            step: null,
            samus,
            sharedProjectiles);
    }

    /// <summary>Runs one native body-function call through the shared verified sequence.</summary>
    private void RunLiveMotherBrainRainbowBeam(
        MotherBrainEnemyState state,
        SamusState samus,
        byte enemyFrameCounter,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
            throw new InvalidOperationException(
                "Mother Brain entered a rainbow function without its live sequence owner.");

        // Body bytecode and articulated-head AI ran through the physical room records on
        // previous frames. Import those words before dispatch so walk completion and neck
        // aiming are decided from the same state the 65816 would read from WRAM.
        SynchronizeLiveMotherBrainRainbowActor(state, sequence);

        MotherBrainRainbowBeamAttackStepResult step = sequence.Step(
            _bus!,
            samus,
            enemyFrameCounter,
            _randomEnemyCounter,
            powerBombActive: sharedProjectiles?.PowerBombExplosion.Status != 0,
            randomNumberSeed: _readRandomNumber?.Invoke() ?? 0,
            nextRandomNumber: _nextRandom);
        state.LastRainbowBeamStep = step;
        ApplyLiveMotherBrainRainbowState(state, sequence, step, samus, sharedProjectiles);
    }

    private static void SynchronizeLiveMotherBrainRainbowActor(
        MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidOperationException(
            "Mother Brain's rainbow beam requires the physical head slot.");
        sequence.SynchronizeLiveActor(
            state.Body.XPosition,
            state.Body.YPosition,
            state.Pose,
            state.Form,
            state.Body.Properties,
            state.Body.ExtraProperties,
            head.XPosition,
            head.YPosition,
            head.Health,
            head.Properties,
            head.ExtraProperties,
            state.LowerNeckAngle,
            state.UpperNeckAngle,
            state.NeckMovementEnabled,
            state.LowerNeckMovementIndex,
            state.UpperNeckMovementIndex,
            state.BombCounter,
            state.HitboxesEnabled != 0);
    }

    /// <summary>
    /// Publishes only values produced by the current body-AI call. In particular, cooldown
    /// and earthquake timers are event writes rather than continuously mirrored values;
    /// continuously copying the sequence's retained word would undo the global owners'
    /// normal per-frame decrements.
    /// </summary>
    private void ApplyLiveMotherBrainRainbowState(
        MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence,
        MotherBrainRainbowBeamAttackStepResult? step,
        SamusState samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        RoomEnemySlot head = state.Head!;

        // `$A9:C447` resets the physical list timer only when body AI requests a new list.
        // The dedicated latch is essential because CurrentInstruction advances away from
        // the list origin while the request itself remains unchanged.
        if (sequence.HeadInstructionList != 0 &&
            (step is { HeadInstructionListRequested: true } ||
             sequence.HeadInstructionList != state.RainbowAppliedHeadInstructionList))
        {
            SetMotherBrainInstructionList(head, sequence.HeadInstructionList);
            state.RainbowAppliedHeadInstructionList = sequence.HeadInstructionList;
        }

        if (step is { } result &&
            (result.BodyWalkRequested || result.BodyPostureRequested))
        {
            ushort bodyInstruction = sequence.RequestedBodyInstructionList;
            if (bodyInstruction == 0)
            {
                throw new InvalidDataException(
                    "Rainbow body AI requested animation without an instruction pointer.");
            }
            SetMotherBrainInstructionList(state.Body, bodyInstruction);
        }

        state.Function = MapLiveMotherBrainRainbowFunction(sequence.Phase);
        state.FunctionTimer = sequence.FunctionTimer;
        // `$7E:8026` remains one shared native word across phase two and phase three. The
        // reusable sequence owns phase-three walking/recoil decisions, while the public
        // encounter state is the debugger-facing WRAM projection used by shot callbacks.
        // Republish it after every body call so either view observes the exact same counter.
        state.WalkCounter = sequence.Phase3WalkCounter;
        state.NeckAngleDelta = sequence.NeckAngleDelta;
        state.NeckMovementEnabled = sequence.NeckMovementEnabled != 0;
        state.LowerNeckMovementIndex = sequence.LowerNeckMovementIndex;
        state.UpperNeckMovementIndex = sequence.UpperNeckMovementIndex;
        if (sequence.Phase3NeckPhase == MotherBrainPhase3NeckPhase.HyperBeamRecoil &&
            sequence.Phase3NeckFunctionTimer == 0x000a)
        {
            // `$C395` loads `$32` during the one-shot setup call, immediately before the
            // fallthrough decrements neck timer `$0B` to `$0A`. The physical draw hook owns
            // subsequent shake decrements, so publish this event only on that exact setup
            // result rather than copying the sequence's retained word every enemy frame.
            state.BrainMainShakeTimer = 0x0032;
        }
        state.Form = sequence.Body.Form;
        state.Body.Properties = sequence.BodyProperties;
        state.Body.ExtraProperties = sequence.BodyProperties2;
        head.Health = sequence.BrainHealth;
        head.Properties = sequence.BrainProperties;
        head.ExtraProperties = sequence.BrainProperties2;
        // The live word begins at two, while the reusable sequence needs only its Boolean
        // branch meaning. Preserve every nonzero cartridge value; only a translated clear
        // is allowed to collapse it to zero.
        if (!sequence.HitboxesEnabled)
            state.HitboxesEnabled = 0;
        state.SmallPurpleBreathGenerationEnabled =
            sequence.SmallPurpleBreathGenerationEnabled;

        // These words are the renderer-facing output of the bank-$88 HDMA object. The beam
        // renderer is deliberately fed from this single translated aim calculation.
        state.RainbowBeamHdmaActive = sequence.HdmaActive;
        state.RainbowBeamAngle = sequence.RainbowBeamAngle;
        state.RainbowBeamAngularWidth = sequence.AngularWidth;

        if (step is not { } current)
            return;

        state.RainbowBeamPaletteRequested = current.PaletteRequested;
        state.LastRainbowBeamExplosion = current.Explosion;
        if (current.SoundQueued)
            state.LastSoundEffectLibrary1 = 0x0040;
        if (current.ChargeSoundQueued || current.FinalBeamSoundQueued)
            state.LastSoundEffect = 0x0071;
        if (current.Explosion is not null)
            state.LastSoundEffect = 0x0024;

        // `$BA0F` seeds the short quake, `$BA27` replaces it with the full drain duration,
        // and `$BA93` clears it when the beam contracts below its minimum width.
        if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.OneFrameDelay &&
            current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.StartDrainingSamus)
        {
            EarthquakeType = 8;
            EarthquakeTimer = 8;
        }
        else if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.StartDrainingSamus)
        {
            EarthquakeType = 8;
            EarthquakeTimer = 0x012b;
        }
        else if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.FinishFiring &&
            current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.LetSamusFall)
        {
            EarthquakeTimer = 0;
            state.LastSoundEffectLibrary1 = 0x0002;
        }

        if (sharedProjectiles is not null)
        {
            if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.WaitForCharge &&
                current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.StartFiring)
            {
                sharedProjectiles.SetSharedCooldown(8);
            }
            else if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.StartFiring &&
                current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall)
            {
                sharedProjectiles.SetSharedCooldown(0);
            }
            else if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.FinishFiring &&
                current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.LetSamusFall)
            {
                sharedProjectiles.SetSharedCooldown(8);
            }
        }

        if (current.SpriteTileTransfer is { } transfer)
            ApplyMotherBrainRainbowTileTransfer(transfer);
        if (current.BabySpawnRequested)
            SpawnMotherBrainBabyMetroid(state);
        if (current.Explosion is { } explosion)
        {
            // `$BC76` runs before `$B9F1/$BA0C/$BA4F` moves Samus. The reusable sequence
            // captures that pre-move point in its typed movement result, so use it for the
            // initializer and let `$C94C` attach to the current position on the later bank-
            // $86 pass. Finish-firing has no movement and therefore uses the unchanged actor.
            ushort baseX = current.Movement?.Before.XPosition ?? samus.XPosition;
            ushort baseY = current.Movement?.Before.YPosition ?? samus.YPosition;
            SpawnMotherBrainRainbowExplosion(baseX, baseY, explosion);
        }
    }

    /// <summary>Ports `$86:C80A-$C828`, including the initializer's same-call pin.</summary>
    private void SpawnMotherBrainRainbowChargingProjectile(MotherBrainEnemyState state)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging,
            graphicsIndex: 0);
        projectile.XVelocity = 0;
        projectile.YVelocity = 0;
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.GraphicsIndex = 0;
        RunMotherBrainRainbowChargingPreInstruction(projectile);
    }

    private void RunMotherBrainRainbowChargingPreInstruction(
        RoomEnemyProjectileSlot projectile)
    {
        RoomEnemySlot head = _motherBrain?.Head ?? throw new InvalidOperationException(
            "Mother Brain rainbow charge ran without the physical head slot.");
        projectile.XPosition = head.XPosition;
        projectile.YPosition = head.YPosition;
    }

    /// <summary>Ports `$86:C92F-$C960`; the signed offsets remain in velocity words.</summary>
    private void SpawnMotherBrainRainbowExplosion(
        ushort samusXBeforeMovement,
        ushort samusYBeforeMovement,
        MotherBrainRainbowExplosionRequest request)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion,
            graphicsIndex: 0);
        projectile.XVelocity = unchecked((ushort)request.XOffset);
        projectile.YVelocity = unchecked((ushort)request.YOffset);
        projectile.GraphicsIndex = 0;
        projectile.XPosition = unchecked((ushort)(samusXBeforeMovement + request.XOffset));
        projectile.YPosition = unchecked((ushort)(samusYBeforeMovement + request.YOffset));
    }

    private static void RunMotherBrainRainbowExplosionPreInstruction(
        RoomEnemyProjectileSlot projectile,
        SamusState? samus)
    {
        SamusState target = samus ?? throw new InvalidOperationException(
            "Mother Brain rainbow explosion requires the live Samus actor.");
        projectile.XPosition = unchecked((ushort)(target.XPosition + projectile.XVelocity));
        projectile.YPosition = unchecked((ushort)(target.YPosition + projectile.YVelocity));
    }

    private void ApplyMotherBrainRainbowTileTransfer(
        MotherBrainSpriteTileTransferRequest transfer)
    {
        var bytes = new byte[transfer.Size];
        for (int index = 0; index < bytes.Length; index++)
            bytes[index] = _bus!.ReadByte(unchecked((int)transfer.SourceAddress + index));
        _vram!.LoadBytes(transfer.VramDestination * 2, bytes);
    }

    private static MotherBrainBodyFunction MapLiveMotherBrainRainbowFunction(
        MotherBrainRainbowBeamAttackPhase phase) => phase switch
        {
            MotherBrainRainbowBeamAttackPhase.RepeatAttack =>
                MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck,
            MotherBrainRainbowBeamAttackPhase.StartCharging =>
                MotherBrainBodyFunction.SecondPhaseRainbowStartCharging,
            MotherBrainRainbowBeamAttackPhase.RetractNeck =>
                MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck,
            MotherBrainRainbowBeamAttackPhase.WaitForCharge =>
                MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge,
            MotherBrainRainbowBeamAttackPhase.ExtendNeckDown =>
                MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown,
            MotherBrainRainbowBeamAttackPhase.StartFiring =>
                MotherBrainBodyFunction.SecondPhaseRainbowStartFiring,
            MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall =>
                MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall,
            MotherBrainRainbowBeamAttackPhase.OneFrameDelay =>
                MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay,
            MotherBrainRainbowBeamAttackPhase.StartDrainingSamus =>
                MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus,
            MotherBrainRainbowBeamAttackPhase.DrainingSamus =>
                MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus,
            MotherBrainRainbowBeamAttackPhase.FinishFiring =>
                MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring,
            MotherBrainRainbowBeamAttackPhase.LetSamusFall =>
                MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall,
            MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand =>
                MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand,
            MotherBrainRainbowBeamAttackPhase.LowerHead =>
                MotherBrainBodyFunction.SecondPhaseRainbowLowerHead,
            MotherBrainRainbowBeamAttackPhase.DecideNextAction =>
                MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction,
            MotherBrainRainbowBeamAttackPhase.FinishSamusOff =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOff,
            MotherBrainRainbowBeamAttackPhase.FinishStandUp =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp,
            MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire,
            MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam,
            MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles,
            MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam,
            MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding =>
                MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidPrepareTransitionToGrey =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey =>
                MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfShowSignsOfLife =>
                MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey =>
                MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp =>
                MotherBrainBodyFunction.SecondPhaseReviveWakeUp,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch =>
                MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid =>
                MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfPrepareNeckForBabyMetroidDeath =>
                MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath =>
                MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath,
            MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack =>
                MotherBrainBodyFunction.SecondPhaseMurderBabyAttack,
            MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttackCooldown =>
                MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown,
            MotherBrainRainbowBeamAttackPhase.PrepareForFinalBabyMetroidAttack =>
                MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack,
            MotherBrainRainbowBeamAttackPhase.ExecuteFinalBabyMetroidAttack =>
                MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack,
            MotherBrainRainbowBeamAttackPhase.FinalBabyMetroidAttackHolding =>
                MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding,
            MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance =>
                MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance,
            MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting =>
                MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting,
            MotherBrainRainbowBeamAttackPhase.Phase3FightingMain =>
                MotherBrainBodyFunction.ThirdPhaseFightingMain,
            MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown =>
                MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown,
            _ => throw new NotSupportedException(
                $"Live Mother Brain rainbow phase {phase} is not attached to a room function yet."),
        };
}
