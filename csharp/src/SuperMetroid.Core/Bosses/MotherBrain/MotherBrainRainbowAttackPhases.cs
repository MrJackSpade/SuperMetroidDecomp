using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's rainbow-beam attack: charging, firing, pinning and draining Samus, letting her fall and deciding what comes next.</summary>
internal sealed class MotherBrainRainbowAttackPhases
{
    /// <summary>Owns the forced Samus movement during beam capture and the later drain sequence.</summary>
    private readonly MotherBrainRainbowBeamSamusMovement _movement ;
    /// <summary>The late fight's own state, with the walking and posture helpers every late-fight phase shares.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>The encounter the late fight runs, the one owner of the state these phases read and write.</summary>
    private MotherBrainEnemyState Encounter => _state.Encounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainRainbowAttackPhases(MotherBrainRainbowBeamSamusMovement movement, MotherBrainLateFightState state)
    {
        _movement = movement;
        _state = state;
    }

    /// <summary>
    /// Rainbow-beam angular width: two steps of a 256-step turn when the beam starts, widening by one and a half steps up
    /// to twelve and narrowing back to two as it shuts down.
    /// </summary>
    public RotationAngle AngularWidth { get; internal set; }

    /// <summary>The beam's starting and narrowest width: two steps.</summary>
    private static readonly RotationAngle NarrowestWidth = RotationAngle.FromSteps(2);

    /// <summary>The beam's widest width: twelve steps.</summary>
    private static readonly RotationAngle WidestWidth = RotationAngle.FromSteps(12);

    /// <summary>How far one widening or narrowing changes the width: one and a half steps.</summary>
    private static readonly RotationAngle WidthStep = RotationAngle.FromSteps(1, 128);

    /// <summary>The beam's sound attempts left, zero or more: each continuing beam step makes one until none are left (#2253).</summary>
    public int SoundAttemptsLeft { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Rainbow-beam explosion timer.</summary>
    public MotherBrainCountdown ExplosionTimer { get; } = new();

    /// <summary>Index of the offset pair the latest beam explosion used, cycling through the eight pairs.</summary>
    public int ExplosionIndex { get; private set; }

    /// <summary>Whether the rainbow attack has started its beam and not yet ended it.</summary>
    public bool BeamActive { get; internal set; }

    /// <summary>
    /// Starts, immediately after the power-bomb gate and charge countdown.
    /// </summary>
    public void StartActiveBeam(SamusState samus)
    {
        Ensure.NotNull(samus);

        _state.SetHeadAnimation(MotherBrainHeadSegment.FiringRainbowBeam);
        AngularWidth = NarrowestWidth;
        ExplosionIndex = 0;
        ExplosionTimer.Count = 0;
        SoundAttemptsLeft = MotherBrainBeamDefinitions.SoundAttempts;
        _state.RainbowBeamSoundPlaying = false;
        BeamActive = true;
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Slow;
        Encounter.NeckMovementEnabled = true;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.BobUp;

        // The rainbow's drain setup leaves Samus able to stand at 700 energy or above and unable to stand
        // below it. The test reads the wrapped difference as signed, retained by AtLeastBySignedDifference.
        if (samus.Health >= 700)
            samus.Drained.SetupForRainbowBeamAbleToStand(samus);
        else
            samus.Drained.SetupForRainbowBeamUnableToStand(samus);

        Encounter.RainbowPhase = MotherBrainRainbowPhase.MoveSamusTowardWall;
    }

    /// <summary>Resets the phase-two head animation and starts the 256-count neck extension before beam charging.</summary>
    internal void BeginExtendingNeckForAttack()
    {
        // The rainbow setup resets the neutral phase-two head animation and selects the
        // ordinary 2/4 neck geometry before beginning its first 256-count wait.
        _state.SetHeadAnimation(MotherBrainHeadSegment.NeutralPhase2);
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Slow;
        Encounter.NeckMovementEnabled = true;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
        Encounter.RainbowPhase = MotherBrainRainbowPhase.StartCharging;
        Encounter.PhaseTimer.Count = 256;
    }

    /// <summary>Requests the slow forward body animation until its target is reached.</summary>
    internal MotherBrainWalkResult RequestWalkForwardReallySlow(int targetX) =>
        _state.MakeBodyWalkForwards(targetX, MotherBrainBodySegment.WalkForwardReallySlow);

    /// <summary>Widens the beam by one and a half steps up to twelve and refreshes its angle toward Samus.</summary>
    private void IncreaseWidthAndAim(SamusState samus)
    {
        RotationAngle widened = AngularWidth + WidthStep;
        AngularWidth = widened > WidestWidth ? WidestWidth : widened;
        AimBeamAtSamus(samus);
    }

    /// <summary>
    /// Advances the held beam's sound countdown, alternating colour request, aim, width,
    /// and explosion timer in the same call after the charge phase completes.
    /// </summary>
    private void RunContinuingBeamEffects(
        SamusState samus,
        MotherBrainRainbowBeamGrowth growth,
        MotherBrainRainbowBeamStepRequests requests)
    {
        if (SoundAttemptsLeft > 0)
        {
            SoundAttemptsLeft--;
            _state.RainbowBeamSoundPlaying = true;
            requests.SoundQueued = true;
        }

        // Her rainbow colours' handler runs on every call; it steps them only on its phase of the body's update count.
        requests.RainbowColours = true;

        if (growth == MotherBrainRainbowBeamGrowth.Widening)
            IncreaseWidthAndAim(samus);
        else
            AimBeamAtSamus(samus);
        requests.Explosion = StepExplosionTimer();
    }

    /// <summary>Converts the angle from the offset brain origin to Samus into the rainbow beam's orientation.</summary>
    private void AimBeamAtSamus(SamusState samus)
    {
        // The aim step aims from brain position (+16,+4) to Samus. The angle calculation consumes
        // signed offsets; the aim step converts its clockwise-up angle to the
        // rainbow beam's anti-clockwise-down convention with `-angle + half turn`.
        RotationAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(
            samus.Position.Whole - (_state.BrainPosition + new Offset16<RoomPixels>(16, 4)));
        _movement.RainbowBeamAngle = RotationAngle.HalfTurn - sourceAngle;
    }

    /// <summary>Emits the next cyclic beam explosion offset when the eight-frame countdown runs out.</summary>
    private MotherBrainRainbowExplosionRequest? StepExplosionTimer()
    {
        if (ExplosionTimer.StepKeepsRunning())
            return null;

        ExplosionTimer.Count = 8;
        ExplosionIndex = (ExplosionIndex + 1) % MotherBrainRainbowBeamAttackSequence.ExplosionOffsets.Length;
        return new MotherBrainRainbowExplosionRequest(
            MotherBrainRainbowBeamAttackSequence.ExplosionOffsets[ExplosionIndex]);
    }

    /// <summary>Drains one health with Varia or two without, clamping health below zero to zero.</summary>
    private static void DamageSamusDueToRainbowBeam(SamusState samus)
    {
        // The drain step starts from -2 in both branches; the Varia test then adds one back
        // with Varia, producing -1 with Varia and -2 without it. The below-zero check then clamps
        // wrapped/below-one health to zero.
        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? 1 : 2;
        int candidate = samus.Health - damage;
        samus.Health = candidate >= 1 ? candidate : 0;
    }

    /// <summary>
    /// Drains missiles and supers every fourth enemy pass and power bombs every pass while
    /// the rainbow beam holds Samus.
    /// </summary>
    private static void DecrementAmmoDueToRainbowBeam(
        SamusState samus,
        int mainEnemyExecutionCounter)
    {
        // Missiles and supers are visited only every fourth main-enemy pass. Power bombs
        // are visited every call, matching the branch layout rather than sharing the gate.
        if (MotherBrainCadenceDefinitions.RainbowMissileDrainEveryFourthEnemyPass.IsDue(mainEnemyExecutionCounter))
        {
            samus.Missiles = DecrementOneAmmoType(samus, samus.Missiles, selectedItem: HudItem.Missile);
            samus.SuperMissiles = DecrementOneAmmoType(samus, samus.SuperMissiles, selectedItem: HudItem.SuperMissile);
        }

        samus.PowerBombs = DecrementOneAmmoType(samus, samus.PowerBombs, selectedItem: HudItem.PowerBomb);
    }

    /// <summary>Decrements one ammo count and clears the selected HUD item on depletion.</summary>
    private static int DecrementOneAmmoType(
        SamusState samus,
        int current,
        HudItem selectedItem)
    {
        if (current == 0)
            return 0;

        int decremented = current - 1;
        if (decremented >= 1)
            return decremented;

        // The step briefly reloads the selected HUD item for the comparison, but both the
        // matching and nonmatching branches converge on a step that first loads zero.
        // Therefore the depleted count is always zero; only the selected-item
        // store itself is conditional. Spell that convergence out to avoid the tempting but
        // incorrect reading that the HUD item number leaks into the ammo count.
        if (samus.SelectedHudItem == selectedItem)
            samus.SelectedHudItem = HudItem.Beam;

        samus.AutoCancelledItem = HudItem.Beam;
        return 0;
    }

    /// <summary>
    /// Runs the second phase rainbow extend neck body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowExtendNeck()
    {
        // The rainbow's restart only installs the setup; the setup executes on the next enemy
        // AI call and deliberately does not fall through into its 256 timer.
        BeginExtendingNeckForAttack();
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow start charging body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowStartCharging()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.ChargingRainbowBeam);
            Encounter.RainbowPhase = MotherBrainRainbowPhase.RetractNeck;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow retract neck body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowRetractNeck()
    {
        MotherBrainWalkResult retract = _state.RequestWalkBackwardReallySlow(targetX: 40);
        if (retract.ReachedTarget)
        {
            _state.RetractHead();
            Encounter.RainbowPhase = MotherBrainRainbowPhase.WaitForCharge;
            Encounter.PhaseTimer.Count = 256;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow wait for charge body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowWaitForCharge(MotherBrainRainbowBeamStepRequests requests)
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            requests.ChargeSoundQueued = true; // The rainbow beam's charge sound.
            Encounter.RainbowPhase = MotherBrainRainbowPhase.ExtendNeckDown;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow extend neck down body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowExtendNeckDown(MotherBrainRainbowBeamStepRequests requests)
    {
        // Lowering the neck holds Samus's shots off before anything else; she is still free to fire.
        requests.SamusProjectileCooldown = MotherBrainBeamDefinitions.NeckLoweredSamusCooldown;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.Lower;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.Lower;
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast; // The Japanese and North American turn; the European release turns seven steps an update.
        Encounter.RainbowPhase = MotherBrainRainbowPhase.StartFiring;
        Encounter.PhaseTimer.Count = 16; // The Japanese and North American value; the European release uses 12.
        return true;
    }

    /// <summary>
    /// Runs the second phase rainbow start firing body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowStartFiring(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The widen-and-aim step widens/aims even while an active power bomb freezes the timer.
        IncreaseWidthAndAim(samus);
        if (!frame.PowerBombActive)
        {
            if (Encounter.PhaseTimer.StepRunsOut())
            {
                // The step rereads the power-bomb flag here. The
                // parameter is one stable sample of the flag for this AI call.
                // The beam clears Samus's cooldown before it captures her.
                requests.SamusProjectileCooldown = MotherBrainBeamDefinitions.BeamStartedSamusCooldown;
                StartActiveBeam(samus);
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow move samus toward wall body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowMoveSamusTowardWall(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            growth: MotherBrainRainbowBeamGrowth.Widening,
            requests);
        requests.Movement = _movement.MoveTowardWall(samus, frame.CameraCheckpoint);
        if (requests.Movement.Value.ReachedVerticalBoundary)
        {
            Encounter.RainbowPhase = MotherBrainRainbowPhase.OneFrameDelay;
            Encounter.PhaseTimer.Count = 0;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow one frame delay body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowOneFrameDelay(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            growth: MotherBrainRainbowBeamGrowth.Widening,
            requests);
        requests.Movement = _movement.MoveTowardWall(samus, frame.CameraCheckpoint);

        // The start-draining step's countdown of zero runs out on this very call.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            requests.StartEarthquake(MotherBrainDeathDefinitions.RainbowBeamEarthquake, 8);
            Encounter.RainbowPhase = MotherBrainRainbowPhase.StartDrainingSamus;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow start draining samus body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowStartDrainingSamus(MotherBrainRainbowBeamStepRequests requests)
    {
        // The drain start installs draining, seeds both its timer and the earthquake with 299, and falls
        // through into the draining phase in the same enemy-processing call.
        Encounter.RainbowPhase = MotherBrainRainbowPhase.DrainingSamus;
        Encounter.PhaseTimer.Count = 299;
        requests.StartEarthquake(MotherBrainDeathDefinitions.RainbowBeamEarthquake, 299);
        return true;
    }

    /// <summary>
    /// Runs the second phase rainbow draining samus body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowDrainingSamus(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            growth: MotherBrainRainbowBeamGrowth.Widening,
            requests);
        DamageSamusDueToRainbowBeam(samus);
        DecrementAmmoDueToRainbowBeam(samus, frame.MainEnemyExecutionCounter);
        requests.Movement = MotherBrainRainbowBeamSamusMovement.MoveTowardMiddleOfWall(samus, frame.CameraCheckpoint);

        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.RainbowPhase = MotherBrainRainbowPhase.FinishFiring;
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow finish firing body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowFinishFiring(SamusState samus, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            growth: MotherBrainRainbowBeamGrowth.FullWidth,
            requests);

        // The finish narrows first, then compares the result with the narrowest width. Width is pinned only after it
        // crosses below that floor; it never narrows below half a step, so it never passes zero.
        AngularWidth -= WidthStep;
        if (AngularWidth < NarrowestWidth)
        {
            AngularWidth = NarrowestWidth;
            _movement.BeginFallingAfterRainbowBeam();
            BeamActive = false;
            requests.BeamEnded = true;
            requests.StopEarthquake();
            _state.RainbowBeamSoundPlaying = false;
            samus.Control.Unlock(); // Restores Samus's control.
            // Released, Samus waits out the beam's cooldown before her next shot.
            requests.SamusProjectileCooldown = MotherBrainBeamDefinitions.BeamEndedSamusCooldown;
            Encounter.RainbowPhase = MotherBrainRainbowPhase.LetSamusFall;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow let samus fall body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowLetSamusFall(SamusState samus)
    {
        // Controller zero changes the pose/radius, then the fall step installs the wait
        // phase and falls straight through to its first custom movement call.
        samus.Drained.LetFall(samus);
        Encounter.RainbowPhase = MotherBrainRainbowPhase.WaitForSamusToLand;
        return true;
    }

    /// <summary>
    /// Runs the second phase rainbow wait for samus to land body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowWaitForSamusToLand(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        requests.Movement = _movement.StepFallingAfterRainbowBeam(samus, frame.CameraCheckpoint);
        if (requests.Movement.Value.ReachedVerticalBoundary)
            Encounter.RainbowPhase = MotherBrainRainbowPhase.LowerHead;
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow lower head body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowLowerHead()
    {
        // Neck positions are presentation/actor-animation state. This phase's movement-
        // relevant effect is the exact phase/timer handoff to the lower-head step.
        Encounter.RainbowPhase = MotherBrainRainbowPhase.DecideNextAction;
        Encounter.PhaseTimer.Count = 128;
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow decide next action body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowDecideNextAction(SamusState samus)
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            if (samus.Health >= 400)
            {
                Encounter.RainbowPhase = MotherBrainRainbowPhase.ExtendNeck;
            }
            else
            {
                // The lower-head step performs this call before installing the later
                // finish-off phase. It starts one complete really-slow forward
                // body animation when Mother Brain is standing and left of 128.
                RequestWalkForwardReallySlow(
                    CoordinateAxis.Wrap(_state.BodyPosition.X + 16));
                _state.BabyArrivalReadyingElapsed = 0;
                _state.BabyMetroidSpawned = false;
                Encounter.FinishOffPhase = MotherBrainFinishOffPhase.GetSamusToLowEnergy;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainRainbowPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it within the same call.
    /// </summary>
    internal bool Run(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests) => Encounter.RainbowPhase switch
    {
        MotherBrainRainbowPhase.ExtendNeck => RunSecondPhaseRainbowExtendNeck(),
        MotherBrainRainbowPhase.StartCharging => RunSecondPhaseRainbowStartCharging(),
        MotherBrainRainbowPhase.RetractNeck => RunSecondPhaseRainbowRetractNeck(),
        MotherBrainRainbowPhase.WaitForCharge => RunSecondPhaseRainbowWaitForCharge(requests),
        MotherBrainRainbowPhase.ExtendNeckDown => RunSecondPhaseRainbowExtendNeckDown(requests),
        MotherBrainRainbowPhase.StartFiring => RunSecondPhaseRainbowStartFiring(samus, frame, requests),
        MotherBrainRainbowPhase.MoveSamusTowardWall => RunSecondPhaseRainbowMoveSamusTowardWall(samus, frame, requests),
        MotherBrainRainbowPhase.OneFrameDelay => RunSecondPhaseRainbowOneFrameDelay(samus, frame, requests),
        MotherBrainRainbowPhase.StartDrainingSamus => RunSecondPhaseRainbowStartDrainingSamus(requests),
        MotherBrainRainbowPhase.DrainingSamus => RunSecondPhaseRainbowDrainingSamus(samus, frame, requests),
        MotherBrainRainbowPhase.FinishFiring => RunSecondPhaseRainbowFinishFiring(samus, requests),
        MotherBrainRainbowPhase.LetSamusFall => RunSecondPhaseRainbowLetSamusFall(samus),
        MotherBrainRainbowPhase.WaitForSamusToLand => RunSecondPhaseRainbowWaitForSamusToLand(samus, frame, requests),
        MotherBrainRainbowPhase.LowerHead => RunSecondPhaseRainbowLowerHead(),
        MotherBrainRainbowPhase.DecideNextAction => RunSecondPhaseRainbowDecideNextAction(samus),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainRainbowPhase)} {Encounter.RainbowPhase}."),
    };
}
