using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's rainbow-beam attack: charging, firing, pinning and draining Samus, letting her fall and deciding what comes next.</summary>
internal sealed partial class MotherBrainRainbowAttackPhases
{
    /// <summary>Owns the forced Samus movement during beam capture and the later drain sequence.</summary>
    private readonly MotherBrainRainbowBeamSamusMovement _movement ;
    /// <summary>The late fight's own state, with the walking and standing every late-fight phase shares.</summary>
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
    /// Rainbow-beam angular width: its narrowest when the beam starts, widening a step at a time up to its widest and
    /// narrowing back to its narrowest as it shuts down.
    /// </summary>
    public RotationAngle AngularWidth { get; internal set; }

    /// <summary>
    /// The beam's sound attempts (#2253): each continuing beam step makes one and counts it, until none are left.
    /// </summary>
    public UpdateCountdown SoundAttempts { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>Rainbow-beam explosion timer.</summary>
    public BossCountdown ExplosionTimer { get; } = new();

    /// <summary>
    /// The next beam explosion's offset among <see cref="MotherBrainBeamDefinitions.ExplosionOffsets"/>, cycling
    /// through them in order.
    /// </summary>
    public ListStep<Offset16<RoomPixels>> NextExplosion { get; private set; } = FirstExplosion;

    /// <summary>The beam's first explosion offset, which every beam bursts first.</summary>
    private static ListStep<Offset16<RoomPixels>> FirstExplosion =>
        ListStep<Offset16<RoomPixels>>.FirstOf(MotherBrainBeamDefinitions.ExplosionOffsets);

    /// <summary>
    /// Whether the rainbow attack has started its beam and not yet ended it (#2560): the beam takes hold as the push toward
    /// the wall begins and ends as Samus is let fall, so it holds from that push through the beam's finish and in no other
    /// phase or stage.
    /// </summary>
    public bool BeamActive => Encounter.Stage == MotherBrainBodyStage.RainbowBeam && Encounter.RainbowPhase is
        MotherBrainRainbowPhase.MoveSamusTowardWall or MotherBrainRainbowPhase.PinSamusToWall or
        MotherBrainRainbowPhase.StartDrainingSamus or MotherBrainRainbowPhase.DrainingSamus or MotherBrainRainbowPhase.FinishFiring;

    /// <summary>
    /// Starts the beam proper once its charge has run out with no Power Bomb going off: the beam takes hold of Samus.
    /// </summary>
    internal void StartActiveBeam(SamusState samus)
    {
        Ensure.NotNull(samus);

        _state.SetHeadAnimation(MotherBrainHeadSegment.FiringRainbowBeam);
        AngularWidth = MotherBrainBeamDefinitions.RainbowNarrowestWidth;
        NextExplosion = FirstExplosion;
        ExplosionTimer.LastFor(1);
        SoundAttempts = UpdateCountdown.Lasting(MotherBrainBeamDefinitions.SoundAttempts);
        _state.RainbowBeamSoundPlaying = false;
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Slow;
        Encounter.NeckMovementEnabled = true;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.BobUp;

        // The rainbow's drain setup leaves Samus able to stand at its standing energy or above and unable to stand
        // below it.
        if (samus.Health >= MotherBrainLateFightDefinitions.SamusStandsInBeamHealth)
            samus.Drained.SetupForRainbowBeamAbleToStand(samus);
        else
            samus.Drained.SetupForRainbowBeamUnableToStand(samus);

        Encounter.RainbowPhase = MotherBrainRainbowPhase.MoveSamusTowardWall;
    }

    /// <summary>Resets the phase-two head animation and starts the neck extension before beam charging.</summary>
    internal void BeginExtendingNeckForAttack()
    {
        // The rainbow's setup shows the neutral head and bobs the neck slowly before its neck extends.
        _state.SetHeadAnimation(MotherBrainHeadSegment.NeutralPhase2);
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Slow;
        Encounter.NeckMovementEnabled = true;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
        Encounter.RainbowPhase = MotherBrainRainbowPhase.StartCharging;
        Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.NeckExtendUpdates);
    }

    /// <summary>Widens the beam by its width step up to its widest and refreshes its angle toward Samus.</summary>
    private void IncreaseWidthAndAim(SamusState samus)
    {
        RotationAngle widened = AngularWidth + MotherBrainBeamDefinitions.RainbowWidthStep;
        AngularWidth = widened > MotherBrainBeamDefinitions.RainbowWidestWidth ? MotherBrainBeamDefinitions.RainbowWidestWidth : widened;
        AimBeamAtSamus(samus);
    }

    /// <summary>
    /// Advances the held beam's sound countdown, its colours, aim, width and explosion timer in one update once its
    /// charge has completed.
    /// </summary>
    private void RunContinuingBeamEffects(
        SamusState samus,
        MotherBrainRainbowBeamGrowth growth,
        MotherBrainRainbowBeamStepRequests requests)
    {
        if (SoundAttempts.IsRunning)
        {
            SoundAttempts.Count();
            _state.RainbowBeamSoundPlaying = true;
            requests.Start(MotherBrainStepEffect.RainbowBeamSound);
        }

        // Her rainbow colours are asked to step on every update; they step only on their phase of the body's update count.
        requests.Start(MotherBrainStepEffect.RainbowColours);

        if (growth == MotherBrainRainbowBeamGrowth.Widening)
            IncreaseWidthAndAim(samus);
        else
            AimBeamAtSamus(samus);
        requests.Explosion = StepExplosionTimer();
    }

    /// <summary>Converts the angle from the offset brain origin to Samus into the rainbow beam's orientation.</summary>
    private void AimBeamAtSamus(SamusState samus)
    {
        // The beam aims from its offset from the brain to Samus. Her bearing is measured clockwise
        // from up; the beam's angle turns anti-clockwise from down, a half turn less the bearing.
        RotationAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(
            samus.Position.Whole - (_state.BrainPosition + MotherBrainFightDefinitions.BeamOffset));
        _movement.RainbowBeamAngle = RotationAngle.HalfTurn - sourceAngle;
    }

    /// <summary>Emits the next cyclic beam explosion offset when its countdown runs out.</summary>
    private MotherBrainRainbowExplosionRequest? StepExplosionTimer()
    {
        if (ExplosionTimer.StepKeepsRunning())
            return null;

        ExplosionTimer.LastFor(MotherBrainLateFightDefinitions.BeamExplosionIntervalUpdates);
        Offset16<RoomPixels> offset = NextExplosion.Entry;
        NextExplosion = NextExplosion.NextInCycle;
        return new MotherBrainRainbowExplosionRequest(offset);
    }

    /// <summary>Drains the beam's health with Varia or more without, clamping health below zero to zero.</summary>
    private static void DamageSamusDueToRainbowBeam(SamusState samus)
    {
        // The beam takes less energy with Varia than without; energy that would fall below one becomes zero.
        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? MotherBrainLateFightDefinitions.BeamDrainWithVaria : MotherBrainLateFightDefinitions.BeamDrainWithoutVaria;
        int candidate = samus.Health - damage;
        samus.Health = candidate >= 1 ? candidate : 0;
    }

    /// <summary>
    /// Drains missiles and supers at their drain cadence and power bombs every pass while
    /// the rainbow beam holds Samus.
    /// </summary>
    private static void DecrementAmmoDueToRainbowBeam(
        SamusState samus,
        int enemyPassCount)
    {
        // Missiles and Super Missiles drain at their cadence, Power Bombs on every update.
        if (MotherBrainCadenceDefinitions.RainbowMissileDrain.IsDue(enemyPassCount))
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

        // An emptied count is zero whichever item is selected; only the selection changes, and only when the emptied
        // weapon was selected.
        if (samus.SelectedHudItem == selectedItem)
            samus.SelectedHudItem = HudItem.Beam;

        samus.AutoCancelledItem = null;
        return 0;
    }

    /// <summary>
    /// Sets the rainbow attack up: her neutral head and a slow bob of her neck, whose extension starts on the next
    /// update. True when her next phase runs on in this same update.
    /// </summary>
    internal bool SetUpBeamAttack()
    {
        // The rainbow's restart only sets the attack up; its neck's extension starts on the next update.
        BeginExtendingNeckForAttack();
        return false;
    }

    /// <summary>
    /// Waits out her neck's extension, then shows her head charging the beam. True when her next phase runs on in this
    /// same update.
    /// </summary>
    internal bool ChargeOnceNeckExtends()
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
    /// Walks her back really slowly to the back of the room, then pulls her head in and starts the charge's wait. True
    /// when her next phase runs on in this same update.
    /// </summary>
    internal bool BackAwayToCharge()
    {
        if (_state.RequestWalkBackward(MotherBrainFightDefinitions.BackOfRoomX, MotherBrainWalkSpeed.ReallySlow))
        {
            _state.RetractHead();
            Encounter.RainbowPhase = MotherBrainRainbowPhase.WaitForCharge;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.ChargeWaitUpdates);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Waits out the beam's charge, then plays its charge sound. True when her next phase runs on in this same update.
    /// </summary>
    internal bool WaitForCharge(MotherBrainRainbowBeamStepRequests requests)
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            requests.Start(MotherBrainStepEffect.ChargeSound); // The rainbow beam's charge sound.
            Encounter.RainbowPhase = MotherBrainRainbowPhase.ExtendNeckDown;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Holds Samus's shots off, lowers her neck quickly and starts the wait before the beam takes hold. True when her
    /// next phase runs on in this same update.
    /// </summary>
    internal bool LowerNeckToFire(MotherBrainRainbowBeamStepRequests requests)
    {
        // Lowering the neck holds Samus's shots off before anything else; she is still free to fire.
        requests.SamusProjectileCooldown = MotherBrainBeamDefinitions.NeckLoweredSamusCooldown;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.Lower;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.Lower;
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast;
        Encounter.RainbowPhase = MotherBrainRainbowPhase.StartFiring;
        Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.NeckLoweringUpdates);
        return true;
    }

    /// <summary>
    /// Widens and aims the beam until its wait runs out with no Power Bomb going off, then the beam takes hold of
    /// Samus. True when her next phase runs on in this same update.
    /// </summary>
    internal bool WidenBeamUntilItTakesHold(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The beam widens and aims even while a Power Bomb going off holds its timer.
        IncreaseWidthAndAim(samus);
        if (!frame.PowerBombActive)
        {
            if (Encounter.PhaseTimer.StepRunsOut())
            {
                // The beam clears Samus's cooldown before it captures her.
                requests.SamusProjectileCooldown = MotherBrainBeamDefinitions.BeamStartedSamusCooldown;
                StartActiveBeam(samus);
            }
        }
        return false;
    }

    /// <summary>
    /// Pushes the held Samus toward the wall behind her until she reaches it. True when her next phase runs on in this
    /// same update.
    /// </summary>
    internal bool PushSamusToWall(SamusState samus, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            growth: MotherBrainRainbowBeamGrowth.Widening,
            requests);
        requests.Movement = _movement.MoveTowardWall(samus);
        if (requests.Movement.Value.ReachedVerticalBoundary)
        {
            Encounter.RainbowPhase = MotherBrainRainbowPhase.PinSamusToWall;
            Encounter.PhaseTimer.LastFor(1);
        }
        return false;
    }

    /// <summary>
    /// Pushes Samus for one more update, then shakes the room as the beam pins her to the wall. True when her next
    /// phase runs on in this same update.
    /// </summary>
    internal bool PinSamusToWall(SamusState samus, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            growth: MotherBrainRainbowBeamGrowth.Widening,
            requests);
        requests.Movement = _movement.MoveTowardWall(samus);

        // The one-update delay runs out on this very update.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            requests.StartEarthquake(MotherBrainDeathDefinitions.RainbowBeamEarthquake, MotherBrainLateFightDefinitions.BeamPinShakeUpdates);
            Encounter.RainbowPhase = MotherBrainRainbowPhase.StartDrainingSamus;
        }
        return false;
    }

    /// <summary>
    /// Starts the drain's wait and its earthquake; the drain itself begins in this same update. True when her next
    /// phase runs on in this same update.
    /// </summary>
    internal bool StartDrainingSamus(MotherBrainRainbowBeamStepRequests requests)
    {
        // The drain starts with the earthquake, which ends on the drain's last update but one, and drains in this same update.
        Encounter.RainbowPhase = MotherBrainRainbowPhase.DrainingSamus;
        Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.DrainUpdates);
        requests.StartEarthquake(MotherBrainDeathDefinitions.RainbowBeamEarthquake, MotherBrainLateFightDefinitions.DrainShakeUpdates);
        return true;
    }

    /// <summary>
    /// Drains Samus's energy and ammunition and holds her against the middle of the wall until the drain's wait runs
    /// out. True when her next phase runs on in this same update.
    /// </summary>
    internal bool DrainSamus(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            growth: MotherBrainRainbowBeamGrowth.Widening,
            requests);
        DamageSamusDueToRainbowBeam(samus);
        DecrementAmmoDueToRainbowBeam(samus, frame.EnemyPassCount);
        requests.Movement = MotherBrainRainbowBeamSamusMovement.MoveTowardMiddleOfWall(samus);

        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.RainbowPhase = MotherBrainRainbowPhase.FinishFiring;
        return false;
    }

    /// <summary>
    /// Narrows the beam until it passes its narrowest, then ends it, stops the earthquake and gives Samus back her
    /// control. True when her next phase runs on in this same update.
    /// </summary>
    internal bool NarrowBeamAndReleaseSamus(SamusState samus, MotherBrainRainbowBeamStepRequests requests)
    {
        RunContinuingBeamEffects(
            samus,
            growth: MotherBrainRainbowBeamGrowth.FullWidth,
            requests);

        // The finish narrows first, then compares the result with the narrowest width. Width is pinned only after it
        // crosses below that floor; it never narrows below half a step, so it never passes zero.
        AngularWidth -= MotherBrainBeamDefinitions.RainbowWidthStep;
        if (AngularWidth < MotherBrainBeamDefinitions.RainbowNarrowestWidth)
        {
            AngularWidth = MotherBrainBeamDefinitions.RainbowNarrowestWidth;
            _movement.BeginFallingAfterRainbowBeam();
            requests.Start(MotherBrainStepEffect.BeamEnded);
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
    /// Lets the released Samus fall; the wait for her landing moves her in this same update. True when her next phase
    /// runs on in this same update.
    /// </summary>
    internal bool LetSamusFall(SamusState samus)
    {
        // Samus is let fall, and the wait for her landing moves her in this same update.
        samus.Drained.LetFall(samus);
        Encounter.RainbowPhase = MotherBrainRainbowPhase.WaitForSamusToLand;
        return true;
    }

    /// <summary>
    /// Moves the falling Samus until she lands. True when her next phase runs on in this same update.
    /// </summary>
    internal bool WaitForSamusToLand(SamusState samus, MotherBrainRainbowBeamStepRequests requests)
    {
        requests.Movement = _movement.StepFallingAfterRainbowBeam(samus);
        if (requests.Movement.Value.ReachedVerticalBoundary)
            Encounter.RainbowPhase = MotherBrainRainbowPhase.LowerHead;
        return false;
    }

    /// <summary>
    /// Starts the pause before she decides what comes next. True when her next phase runs on in this same update.
    /// </summary>
    internal bool StartPauseBeforeDeciding()
    {
        // Lowering the head waits before Mother Brain decides what comes next.
        Encounter.RainbowPhase = MotherBrainRainbowPhase.DecideNextAction;
        Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.LowerHeadUpdates);
        return false;
    }

    /// <summary>
    /// When the pause runs out, fires the beam again while Samus keeps enough energy, and otherwise steps forward and
    /// begins finishing her off. True when her next phase runs on in this same update.
    /// </summary>
    internal bool FireAgainOrFinishSamusOff(SamusState samus)
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            if (samus.Health >= MotherBrainLateFightDefinitions.FinishOffSamusHealth)
            {
                Encounter.RainbowPhase = MotherBrainRainbowPhase.ExtendNeck;
            }
            else
            {
                // Before the finish-off begins, Mother Brain starts one really slow step forward when she is standing
                // and short of the arena's front limit.
                _state.RequestWalkForward(
                    _state.BodyPosition.X + MotherBrainLateFightDefinitions.FinishOffStepX, MotherBrainWalkSpeed.ReallySlow);
                _state.BabyMetroidSpawned = false;
                Encounter.FinishOffPhase = MotherBrainFinishOffPhase.AttackUntilSamusIsLow;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainRainbowPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it in the same update.
    /// </summary>
    internal bool Run(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests) => Encounter.RainbowPhase switch
    {
        MotherBrainRainbowPhase.ExtendNeck => SetUpBeamAttack(),
        MotherBrainRainbowPhase.StartCharging => ChargeOnceNeckExtends(),
        MotherBrainRainbowPhase.RetractNeck => BackAwayToCharge(),
        MotherBrainRainbowPhase.WaitForCharge => WaitForCharge(requests),
        MotherBrainRainbowPhase.ExtendNeckDown => LowerNeckToFire(requests),
        MotherBrainRainbowPhase.StartFiring => WidenBeamUntilItTakesHold(samus, frame, requests),
        MotherBrainRainbowPhase.MoveSamusTowardWall => PushSamusToWall(samus, requests),
        MotherBrainRainbowPhase.PinSamusToWall => PinSamusToWall(samus, requests),
        MotherBrainRainbowPhase.StartDrainingSamus => StartDrainingSamus(requests),
        MotherBrainRainbowPhase.DrainingSamus => DrainSamus(samus, frame, requests),
        MotherBrainRainbowPhase.FinishFiring => NarrowBeamAndReleaseSamus(samus, requests),
        MotherBrainRainbowPhase.LetSamusFall => LetSamusFall(samus),
        MotherBrainRainbowPhase.WaitForSamusToLand => WaitForSamusToLand(samus, requests),
        MotherBrainRainbowPhase.LowerHead => StartPauseBeforeDeciding(),
        MotherBrainRainbowPhase.DecideNextAction => FireAgainOrFinishSamusOff(samus),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainRainbowPhase)} {Encounter.RainbowPhase}."),
    };
}
