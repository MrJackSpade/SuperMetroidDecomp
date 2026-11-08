using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's rainbow-beam attack: charging, firing, pinning and draining Samus, letting her fall and deciding what comes next.</summary>
internal sealed class MotherBrainRainbowAttackPhases
{
    /// <summary>Owns the forced Samus movement during beam capture and the later drain sequence.</summary>
    private readonly MotherBrainRainbowBeamSamusMovement _movement ;
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs, the brain's flags and the earthquake, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainRainbowAttackPhases(MotherBrainRainbowBeamSamusMovement movement, MotherBrainLateFightState state)
    {
        _movement = movement;
        _state = state;
    }

    /// <summary>The session's record of what the beam's push hides from the camera, which every push requires.</summary>
    private static RainbowBeamCameraCheckpoint CameraCheckpointOf(RainbowBeamAttackFrame frame) =>
        frame.CameraCheckpoint ?? throw new InvalidOperationException(
            "Mother Brain's rainbow-beam push requires the session's camera checkpoint.");

    /// <summary>Rainbow-beam angular width in 8.8 table units, initialized and floored and capped.</summary>
    public int AngularWidth { get; internal set; }

    /// <summary>Sound queue attempts left, counted down until it runs past zero. Six produces seven queue attempts.</summary>
    public int SoundQueueCount { get; internal set; }

    /// <summary>Rainbow-beam explosion timer.</summary>
    public int ExplosionTimer { get; private set; }

    /// <summary>Index of the offset pair the latest beam explosion used, cycling through the eight pairs.</summary>
    public int ExplosionIndex { get; private set; }

    /// <summary>Whether the rainbow attack has started its beam and not yet ended it.</summary>
    public bool BeamActive { get; internal set; }

    /// <summary>Samus projectile cooldown written at beam shutdown.</summary>
    public int SamusProjectileCooldownTimer { get; private set; }

    /// <summary>
    /// Starts, immediately after the power-bomb gate and charge countdown.
    /// </summary>
    public void StartActiveBeam(SamusState samus)
    {
        Ensure.NotNull(samus);

        _state.SetHeadProgram(MotherBrainHeadProgram.FiringRainbowBeam);
        AngularWidth = 512;
        ExplosionIndex = 0;
        ExplosionTimer = 0;
        SoundQueueCount = 6;
        _state.RainbowBeamSoundPlaying = false;
        BeamActive = true;
        _state.EarthquakeType = 0;
        _state.EarthquakeTimer = WrappingCountdown.Start(0);
        SamusProjectileCooldownTimer = 0;
        _state.NeckAngleDelta = 64;
        _state.NeckMovementEnabled = true;
        _state.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
        _state.UpperNeckMovement = MotherBrainNeckMovement.BobUp;

        // The rainbow's Samus command selects command five at 700 energy or above and command 24
        // below it. CPY/BPL is a signed 16-bit branch, retained by AtLeastBySignedDifference.
        if (samus.Health >= 700)
            samus.Drained.SetupForRainbowBeamAbleToStand(samus);
        else
            samus.Drained.SetupForRainbowBeamUnableToStand(samus);

        _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall;
    }

    /// <summary>Resets the phase-two head list and starts the 256-count neck extension before beam charging.</summary>
    internal void BeginExtendingNeckForAttack()
    {
        // The rainbow setup resets the neutral phase-two head program and selects the
        // ordinary 2/4 neck geometry before beginning its first 256-count wait.
        _state.SetHeadProgram(MotherBrainHeadProgram.NeutralPhase2);
        _state.NeckAngleDelta = 64;
        _state.NeckMovementEnabled = true;
        _state.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
        _state.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
        _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowStartCharging;
        _state.FunctionTimer = 256;
    }

    /// <summary>Requests the slow forward body animation until its target is reached.</summary>
    internal MotherBrainWalkResult RequestWalkForwardReallySlow(int targetX) =>
        _state.MakeBodyWalkForwards(targetX, MotherBrainBodyProgram.WalkForwardReallySlow);

    /// <summary>Widens the beam by $180 up to $C00 and refreshes its angle toward Samus.</summary>
    private void IncreaseWidthAndAim(SamusState samus)
    {
        AngularWidth = Math.Min(AngularWidth + 384, 3072);
        AimBeamAtSamus(samus);
    }

    /// <summary>
    /// Advances the held beam's sound countdown, alternating palette request, aim, width,
    /// and explosion timer in the same call after the charge phase completes.
    /// </summary>
    private void RunContinuingBeamEffects(
        SamusState samus,
        int enemyFrameCounter,
        MotherBrainRainbowBeamGrowth growth,
        MotherBrainRainbowBeamStepRequests requests)
    {
        // The cry step checks the signed counter before the decrement. Starting at six therefore queues
        // on values 6,5,4,3,2,1,0 and leaves -1 to suppress all later calls.
        if (SoundQueueCount >= 0)
        {
            SoundQueueCount--;
            _state.RainbowBeamSoundPlaying = true;
            requests.SoundQueued = true;
        }

        // Palette handling runs only when enemy frame-counter bit one is set. The palette
        // program itself remains presentation work, but its cadence is observable.
        requests.PaletteRequested = (enemyFrameCounter & 2) != 0;

        if (growth == MotherBrainRainbowBeamGrowth.Widening)
            IncreaseWidthAndAim(samus);
        else
            AimBeamAtSamus(samus);
        requests.Explosion = StepExplosionTimer();
    }

    /// <summary>Converts the angle from the offset brain origin to Samus into the rainbow beam's orientation.</summary>
    private void AimBeamAtSamus(SamusState samus)
    {
        // The aim step aims from brain position (+16,+4) to Samus. The angle routine consumes
        // signed 16-bit offsets; the aim step converts its clockwise-up angle to the
        // rainbow beam's anti-clockwise-down convention with `-angle + half turn`.
        RotationAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(
            samus.Position.Whole - (_state.BrainPosition + new Offset16<RoomPixels>(16, 4)));
        _movement.RainbowBeamAngle = RotationAngle.HalfTurn - sourceAngle;
    }

    /// <summary>Emits the next cyclic beam explosion offset when the eight-frame countdown underflows.</summary>
    private MotherBrainRainbowExplosionRequest? StepExplosionTimer()
    {
        if (--ExplosionTimer >= 0)
            return null;

        ExplosionTimer = 8;
        ExplosionIndex = (ExplosionIndex + 1) % MotherBrainRainbowBeamAttackSequence.ExplosionOffsets.Length;
        return new MotherBrainRainbowExplosionRequest(
            MotherBrainRainbowBeamAttackSequence.ExplosionOffsets[ExplosionIndex]);
    }

    /// <summary>Drains one health with Varia or two without, clamping underflow to zero.</summary>
    private static void DamageSamusDueToRainbowBeam(SamusState samus)
    {
        // The drain step loads -2 in both branches. The meaningful Varia distinction is
        // carry left by LSR EquippedItems: bit zero adds one back, producing -1 with Varia
        // and -2 without it. CMP #1/BPL then clamps wrapped/below-one health to zero.
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
        if ((mainEnemyExecutionCounter & 3) == 0)
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

        // The original briefly reloads A with SelectedHUDItem for the comparison, but both the
        // matching and nonmatching branches converge at a label that first loads zero.
        // Therefore the depleted count is always zero; only the selected-item
        // store itself is conditional. Spell that convergence out to avoid the tempting but
        // incorrect interpretation that the HUD item number leaks into the ammo count.
        if (samus.SelectedHudItem == selectedItem)
            samus.SelectedHudItem = HudItem.Beam;

        samus.AutoCancelHudItemIndex = HudItem.Beam;
        return 0;
    }

    /// <summary>
    /// Runs the second phase rainbow extend neck body function. True when it installs the next body function and the game runs
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
    /// Runs the second phase rainbow start charging body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowStartCharging()
    {
        if (--_state.FunctionTimer < 0)
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.ChargingRainbowBeam);
            _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow retract neck body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowRetractNeck(MotherBrainRainbowBeamStepRequests requests)
    {
        MotherBrainWalkResult retract = _state.RequestWalkBackwardReallySlow(targetX: 40);
        requests.BodyWalkRequested = retract.Requested;
        if (retract.ReachedTarget)
        {
            _state.RetractHead();
            _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge;
            _state.FunctionTimer = 256;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow wait for charge body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowWaitForCharge(MotherBrainRainbowBeamStepRequests requests)
    {
        if (--_state.FunctionTimer < 0)
        {
            requests.ChargeSoundQueued = true; // The rainbow beam's charge sound.
            _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow extend neck down body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowExtendNeckDown()
    {
        SamusProjectileCooldownTimer = 8;
        _state.LowerNeckMovement = MotherBrainNeckMovement.Lower;
        _state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
        _state.NeckAngleDelta = 1280; // NTSC value of the regional pair 1280/1792.
        _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowStartFiring;
        _state.FunctionTimer = 16; // NTSC value of the regional pair 16/12.
        return true;
    }

    /// <summary>
    /// Runs the second phase rainbow start firing body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowStartFiring(SamusState samus, RainbowBeamAttackFrame frame)
    {
        // The widen-and-aim step widens/aims even while an active power bomb freezes the timer.
        IncreaseWidthAndAim(samus);
        if (!frame.PowerBombActive)
        {
            if (--_state.FunctionTimer < 0)
            {
                // The original game redundantly rereads the power-bomb flag here. The
                // parameter is one stable sample of the flag for this translated AI call.
                SamusProjectileCooldownTimer = 0;
                StartActiveBeam(samus);
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow move samus toward wall body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowMoveSamusTowardWall(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            frame.EnemyFrameCounter,
            growth: MotherBrainRainbowBeamGrowth.Widening,
            requests);
        requests.Movement = _movement.MoveTowardWall(samus, CameraCheckpointOf(frame));
        if (requests.Movement.Value.ReachedVerticalBoundary)
        {
            _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay;
            _state.FunctionTimer = 0;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow one frame delay body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowOneFrameDelay(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            frame.EnemyFrameCounter,
            growth: MotherBrainRainbowBeamGrowth.Widening,
            requests);
        requests.Movement = _movement.MoveTowardWall(samus, CameraCheckpointOf(frame));

        // The start-draining step decrements zero to -1; the sign test fails on this very call.
        if (--_state.FunctionTimer < 0)
        {
            _state.EarthquakeType = 8;
            _state.EarthquakeTimer = WrappingCountdown.Start(8);
            _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow start draining samus body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowStartDrainingSamus()
    {
        // The drain start installs draining, seeds both timers with 299, and falls
        // through into the draining function in the same enemy-processing call.
        _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus;
        _state.FunctionTimer = 299;
        _state.EarthquakeTimer = WrappingCountdown.Start(299);
        _state.EarthquakeType = 8;
        return true;
    }

    /// <summary>
    /// Runs the second phase rainbow draining samus body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowDrainingSamus(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            frame.EnemyFrameCounter,
            growth: MotherBrainRainbowBeamGrowth.Widening,
            requests);
        DamageSamusDueToRainbowBeam(samus);
        DecrementAmmoDueToRainbowBeam(samus, frame.MainEnemyExecutionCounter);
        requests.Movement = MotherBrainRainbowBeamSamusMovement.MoveTowardMiddleOfWall(samus, CameraCheckpointOf(frame));

        if (--_state.FunctionTimer < 0)
            _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring;
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow finish firing body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowFinishFiring(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            frame.EnemyFrameCounter,
            growth: MotherBrainRainbowBeamGrowth.FullWidth,
            requests);

        // The finish narrows first, then compares the signed result with
        // an eighth of a turn. Width is pinned only after it crosses below that floor.
        AngularWidth -= 384;
        if (AngularWidth < 512)
        {
            AngularWidth = 512;
            _movement.BeginFallingAfterRainbowBeam();
            BeamActive = false;
            _state.EarthquakeTimer = WrappingCountdown.Start(0);
            _state.RainbowBeamSoundPlaying = false;
            samus.Control.Unlock(); // Samus command one.
            SamusProjectileCooldownTimer = 8;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow let samus fall body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowLetSamusFall(SamusState samus)
    {
        // Controller zero changes the pose/radius, then the fall step installs the wait
        // function and falls straight through to its first custom movement call.
        samus.Drained.LetFall(samus);
        _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand;
        return true;
    }

    /// <summary>
    /// Runs the second phase rainbow wait for samus to land body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowWaitForSamusToLand(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        requests.Movement = _movement.StepFallingAfterRainbowBeam(samus, CameraCheckpointOf(frame));
        if (requests.Movement.Value.ReachedVerticalBoundary)
            _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowLowerHead;
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow lower head body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowLowerHead()
    {
        // Neck words are presentation/actor-animation state. This phase's movement-
        // relevant effect is the exact function/timer handoff to the lower-head step.
        _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction;
        _state.FunctionTimer = 128;
        return false;
    }

    /// <summary>
    /// Runs the second phase rainbow decide next action body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRainbowDecideNextAction(SamusState samus, MotherBrainRainbowBeamStepRequests requests)
    {
        if (--_state.FunctionTimer < 0)
        {
            if (samus.Health >= 400)
            {
                _state.Phase = MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck;
            }
            else
            {
                // The lower-head step performs this call before installing the later
                // finish-off function. It starts one complete really-slow forward
                // body animation when Mother Brain is standing and left of 128.
                requests.BodyWalkRequested = RequestWalkForwardReallySlow(
                    CoordinateAxis.Wrap(_state.Body.Position.X + 16)).Requested;
                _state.BabyArrivalReadyingElapsed = 0;
                _state.BabyMetroidSpawned = false;
                _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOff;
            }
        }
        return false;
    }
}
