namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain drained by the Baby Metroid: taken aback, firing back, running out, retreating and turning gray.</summary>
internal sealed class MotherBrainDrainedByBabyPhases
{
    /// <summary>The late fight's own state, with the walking and standing every late-fight phase shares.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>The encounter the late fight runs, the one owner of the state these phases read and write.</summary>
    private MotherBrainEnemyState Encounter => _state.Encounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainDrainedByBabyPhases(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>
    /// The stage of her stagger she walks (#2315): the stages walk forward and back in turn, the first forward, and the last
    /// stays once she has walked it.
    /// </summary>
    internal ListStep<MotherBrainStaggerStage> PainfulWalkingStage { get; set; } =
        ListStep<MotherBrainStaggerStage>.FirstOf(MotherBrainPainfulWalkingDefinitions.Stages);

    /// <summary>How fast she staggers in the current stage, slowing as the stages pass.</summary>
    public MotherBrainWalkSpeed PainfulWalkingSpeed { get; internal set; }

    /// <summary>Her pause at the end of the current stage's walk (#2469), or null while she walks it.</summary>
    internal BossCountdown? PainfulWalkingPause { get; set; }

    /// <summary>
    /// Steps her stagger one update: each stage walks her to one end of her stagger and pauses her there, and the pause's
    /// end turns her round into the next stage, whose first update walks her the other way.
    /// </summary>
    /// <returns>Whether this update ended the pause of her last stage, so that she has walked every stage.</returns>
    internal bool StepPainfulWalking()
    {
        if (PainfulWalkingPause is { } pause)
        {
            if (!pause.StepRunsOut())
                return false;

            PainfulWalkingPause = null;
            if (PainfulWalkingStage.Following is not { } next)
                return true;

            PainfulWalkingStage = next;
            return false;
        }

        bool reached = WalksForward(PainfulWalkingStage)
            ? _state.RequestWalkForward(MotherBrainFightDefinitions.StaggerFrontX, PainfulWalkingSpeed)
            : _state.RequestWalkBackward(MotherBrainFightDefinitions.BackOfRoomX, PainfulWalkingSpeed);
        if (reached)
            PainfulWalkingPause = BossCountdown.Lasting(PainfulWalkingStage.Entry.PauseUpdates);
        return false;
    }

    /// <summary>Whether <paramref name="stage"/> walks her forward, as its definition says.</summary>
    private static bool WalksForward(ListStep<MotherBrainStaggerStage> stage) => stage.Entry.Direction == MotherBrainStaggerDirection.Forward;

    /// <summary>
    /// Recoils from the Baby latching onto her: her form changes, her neck rises and the wait to regain her balance
    /// starts. True when her next phase runs on in this same update.
    /// </summary>
    internal bool RecoilFromBaby()
    {
        // Her recoil from the Baby runs on her own turn: it changes her form and neck and starts the regain-balance
        // wait on the same update.
        Encounter.Form = MotherBrainForm.DrainedByBabyMetroid;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.Raise;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.Raise;
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.TakenAback;
        Encounter.DrainedPhase = MotherBrainDrainedPhase.RegainBalance;
        Encounter.PhaseTimer.LastFor(MotherBrainFightDefinitions.RegainBalanceUpdates);
        return true;
    }

    /// <summary>
    /// Steps her rainbow colours until her balance returns, then starts her painful walk. True when her next phase runs
    /// on in this same update.
    /// </summary>
    internal bool RegainBalance(MotherBrainRainbowBeamStepRequests requests)
    {
        requests.Start(MotherBrainStepEffect.RainbowColours);
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            PainfulWalkingStage = ListStep<MotherBrainStaggerStage>.FirstOf(MotherBrainPainfulWalkingDefinitions.Stages);
            PainfulWalkingSpeed = MotherBrainWalkSpeed.ReallyFast;
            PainfulWalkingPause = null;
            Encounter.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
            Encounter.DrainedPhase = MotherBrainDrainedPhase.FiringRainbowBeam;
        }
        return false;
    }

    /// <summary>
    /// Staggers her through her painful walk while her beam fires, until the beam runs out. True when her next phase
    /// runs on in this same update.
    /// </summary>
    internal bool StaggerWhileFiringBeam(MotherBrainRainbowBeamStepRequests requests)
    {
        // Her brain's shake starts again only once it has run out; her brain's animation counts it down.
        if (!Encounter.BrainShake.IsShaking)
            Encounter.BrainShake.ShakeFor(MotherBrainFightDefinitions.BrainShakeUpdates);
        requests.Start(MotherBrainStepEffect.RainbowColours);
        // Her beam runs out before her last stage, so her stagger never ends while it fires.
        _ = StepPainfulWalking();

        // The outer phase updates speed and neck delta after every step of the painful walk, so the beam's last step is seen
        // on the update that ends the pause before it, before its forward walk has begun.
        MotherBrainStaggerStage stage = PainfulWalkingStage.Entry;
        PainfulWalkingSpeed = stage.Speed;
        Encounter.NeckTurnPerUpdate = stage.NeckTurn;
        if (stage.BeamRunsOut)
        {
            _state.RainbowBeamSoundPlaying = false;
            Encounter.DrainedPhase = MotherBrainDrainedPhase.RainbowBeamRunOut;
            requests.Start(MotherBrainStepEffect.DrainedBeamRanOut);
            requests.Start(MotherBrainStepEffect.RainbowBeamSound); // The rainbow-drain sound stops as the beam runs out.
        }
        return false;
    }

    /// <summary>
    /// Finishes her painful walk once the beam has run out, then raises her neck and starts her drooling. True when her
    /// next phase runs on in this same update.
    /// </summary>
    internal bool FinishStaggering()
    {
        if (StepPainfulWalking())
        {
            Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Slow;
            Encounter.LowerNeckMovement = MotherBrainNeckMovement.Raise;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.Raise;
            _state.SetHeadAnimation(MotherBrainHeadSegment.DyingDrool);
            Encounter.DrainedPhase = MotherBrainDrainedPhase.MoveToBackOfRoom;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Walks her back really slowly to the back of the room. True when her next phase runs on in this same update.
    /// </summary>
    internal bool RetreatToBackOfRoom()
    {
        // The walk back is the really-slow backward walk, the painful walk's last animation.
        if (_state.RequestWalkBackward(MotherBrainFightDefinitions.BackOfRoomX, MotherBrainWalkSpeed.ReallySlow))
        {
            Encounter.DrainedPhase = MotherBrainDrainedPhase.SlumpOnceNeckIsRaised;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.None;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Once her neck has finished rising, stops her drool and, when she stands, crouches her and starts the wait before
    /// she greys. True when her next phase runs on in this same update.
    /// </summary>
    internal bool SlumpOnceNeckIsRaised()
    {
        // Her brain raises her neck on its own; once both halves of the neck have stopped moving her drool stops, before
        // she waits for her pose.
        if (Encounter.LowerNeckMovement == MotherBrainNeckMovement.None &&
            Encounter.UpperNeckMovement == MotherBrainNeckMovement.None)
        {
            Encounter.DroolGenerationEnabled = false;
            if (Encounter.Pose != MotherBrainBodyPose.Standing)
                return false;
            _state.SetBodyAnimation(MotherBrainBodySegment.CrouchFast);
            Encounter.DrainedPhase = MotherBrainDrainedPhase.WaitBeforeGreying;
            Encounter.PhaseTimer.LastFor(MotherBrainFightDefinitions.BeforeGreyUpdates);
        }
        return false;
    }

    /// <summary>
    /// When the wait runs out, starts her greying. True when her next phase runs on in this same update.
    /// </summary>
    internal bool StartGreying()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.DrainedGreying.StartAfter(MotherBrainFightDefinitions.GreyStepUpdates);
            Encounter.DrainedPhase = MotherBrainDrainedPhase.TransitionToGrey;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Greys her through the drain's fade; the update the fade is over leaves her a grey corpse with her third phase's
    /// health. True when her next phase runs on in this same update.
    /// </summary>
    internal bool GreyOut(MotherBrainRainbowBeamStepRequests requests)
    {
        if (Encounter.DrainedGreying.Step() is { } image)
            requests.DrainColourImage = image;
        else if (Encounter.DrainedGreying.IsOver)
        {
            Encounter.Head.Health = MotherBrainFightDefinitions.ThirdPhaseHealth;
            _state.Phase2CorpseState = MotherBrainPhase2CorpseState.Corpse;
            Encounter.SmallPurpleBreathGenerationEnabled = false;
            Encounter.Form = MotherBrainForm.SecondPhase;
            Encounter.RevivalPhase = MotherBrainRevivalPhase.InanimateGrey;
        }
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainDrainedPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it in the same update.
    /// </summary>
    internal bool Run(MotherBrainRainbowBeamStepRequests requests) => Encounter.DrainedPhase switch
    {
        MotherBrainDrainedPhase.TakenAback => RecoilFromBaby(),
        MotherBrainDrainedPhase.RegainBalance => RegainBalance(requests),
        MotherBrainDrainedPhase.FiringRainbowBeam => StaggerWhileFiringBeam(requests),
        MotherBrainDrainedPhase.RainbowBeamRunOut => FinishStaggering(),
        MotherBrainDrainedPhase.MoveToBackOfRoom => RetreatToBackOfRoom(),
        MotherBrainDrainedPhase.SlumpOnceNeckIsRaised => SlumpOnceNeckIsRaised(),
        MotherBrainDrainedPhase.WaitBeforeGreying => StartGreying(),
        MotherBrainDrainedPhase.TransitionToGrey => GreyOut(requests),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainDrainedPhase)} {Encounter.DrainedPhase}."),
    };
}
