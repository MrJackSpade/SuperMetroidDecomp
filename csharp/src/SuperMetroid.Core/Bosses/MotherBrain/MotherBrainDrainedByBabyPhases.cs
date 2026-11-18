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

    /// <summary>The stage of her stagger she walks (#2315), or null once she has walked every stage.</summary>
    internal ListStep<MotherBrainStaggerStage>? PainfulWalkingStage { get; set; }

    /// <summary>How fast she staggers in the current stage, slowing as the stages pass.</summary>
    public MotherBrainWalkSpeed PainfulWalkingSpeed { get; internal set; }

    /// <summary>Pause timer used after each completed forward/backward body animation.</summary>
    public int PainfulWalkingPauseTimer { get; private set; }

    /// <summary>True while the nested painful walk is on its forward half.</summary>
    public bool PainfulWalkingForward { get; internal set; }

    /// <summary>Alternates painful forward and backward walk requests, advancing the stage only after its pause expires.</summary>
    internal void StepPainfulWalking()
    {
        // Each stage walks her to one end of her stagger and pauses her there; the pause's end turns her around and
        // begins the next stage, whose first update walks her the other way.
        if (PainfulWalkingPauseTimer != 0)
        {
            if (--PainfulWalkingPauseTimer == 0)
            {
                PainfulWalkingStage = WalkedStage.Following;
                PainfulWalkingForward = !PainfulWalkingForward;
            }
            return;
        }

        bool reached = PainfulWalkingForward
            ? _state.RequestWalkForward(MotherBrainFightDefinitions.StaggerFrontX, PainfulWalkingSpeed)
            : _state.RequestWalkBackward(MotherBrainFightDefinitions.BackOfRoomX, PainfulWalkingSpeed);
        if (reached)
            PainfulWalkingPauseTimer = WalkedStage.Entry.PauseUpdates;
    }

    /// <summary>The stage of her stagger she walks.</summary>
    /// <exception cref="InvalidOperationException">She has walked every stage.</exception>
    private ListStep<MotherBrainStaggerStage> WalkedStage => PainfulWalkingStage ??
        throw new InvalidOperationException("Mother Brain's stagger walked on after its last stage.");

    /// <summary>
    /// Recoils from the Baby latching onto her: her form changes, her neck rises and the wait to regain her balance
    /// starts. True when her next phase runs on in this same update.
    /// </summary>
    internal bool RecoilFromBaby()
    {
        // The taken-aback step is not executed by the Baby. It runs on Mother Brain's next
        // actor turn, changes form/neck geometry, starts the regain-balance wait, and falls through so
        // that this update is the wait's first.
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
        requests.RainbowColours = true;
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            PainfulWalkingForward = true;
            PainfulWalkingStage = ListStep<MotherBrainStaggerStage>.FirstOf(MotherBrainPainfulWalkingDefinitions.Stages);
            PainfulWalkingSpeed = MotherBrainWalkSpeed.ReallyFast;
            PainfulWalkingPauseTimer = 0;
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
        if (Encounter.BrainMainShakeTimer == 0)
            Encounter.BrainMainShakeTimer = MotherBrainFightDefinitions.BrainShakeUpdates;
        requests.RainbowColours = true;
        StepPainfulWalking();

        // The outer phase updates speed and neck delta after every step of the painful walk, so the beam's last step is seen
        // on the update that ends the pause before it, before its forward walk has begun.
        if (PainfulWalkingStage is { Entry: { } stage })
        {
            PainfulWalkingSpeed = stage.Speed;
            Encounter.NeckTurnPerUpdate = stage.NeckTurn;
        }
        if (ReferenceEquals(PainfulWalkingStage?.Entry, MotherBrainPainfulWalkingDefinitions.BeamRunsOut))
        {
            _state.RainbowBeamSoundPlaying = false;
            Encounter.DrainedPhase = MotherBrainDrainedPhase.RainbowBeamRunOut;
            requests.DrainedBeamRanOut = true;
            requests.SoundQueued = true; // The rainbow-drain sound stops as the beam runs out.
        }
        return false;
    }

    /// <summary>
    /// Finishes her painful walk once the beam has run out, then raises her neck and starts her drooling. True when her
    /// next phase runs on in this same update.
    /// </summary>
    internal bool FinishStaggering()
    {
        StepPainfulWalking();
        if (PainfulWalkingStage is null)
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
            Encounter.DrainedPhase = MotherBrainDrainedPhase.GoIntoLowPowerMode;
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
        // The brain enemy advances the one-way neck raises on its own. The body
        // clears drool once both indices reach zero, before waiting for its pose.
        if (Encounter.LowerNeckMovement == MotherBrainNeckMovement.None &&
            Encounter.UpperNeckMovement == MotherBrainNeckMovement.None)
        {
            Encounter.DroolGenerationEnabled = false;
            if (Encounter.Pose != MotherBrainBodyPose.Standing)
                return false;
            _state.SetBodyAnimation(MotherBrainBodySegment.CrouchFast);
            Encounter.DrainedPhase = MotherBrainDrainedPhase.PrepareTransitionToGrey;
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
            Encounter.DrainedGreying.Restart();
            Encounter.DrainedPhase = MotherBrainDrainedPhase.TransitionToGrey;
            Encounter.PhaseTimer.LastFor(MotherBrainFightDefinitions.GreyStepUpdates);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Greys her one step each time the wait runs out; the step after the last leaves her a grey corpse with her third
    /// phase's health. True when her next phase runs on in this same update.
    /// </summary>
    internal bool GreyOut(MotherBrainRainbowBeamStepRequests requests)
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.PhaseTimer.LastFor(MotherBrainFightDefinitions.GreyStepUpdates);

            // The body greys over its grey steps, one each time the timer runs out; the ninth finishes the transition
            // without new colours.
            if (MotherBrainFadeDefinitions.DrainToGrey.ImageAt(Encounter.DrainedGreying.TakeStep()) is { } image)
                requests.DrainColourImage = image;
            else
            {
                Encounter.Head.Health = MotherBrainFightDefinitions.ThirdPhaseHealth;
                _state.Phase2CorpseState = MotherBrainPhase2CorpseState.Corpse;
                Encounter.SmallPurpleBreathGenerationEnabled = false;
                Encounter.Form = MotherBrainForm.SecondPhase;
                Encounter.RevivalPhase = MotherBrainRevivalPhase.InanimateGrey;
            }
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
        MotherBrainDrainedPhase.GoIntoLowPowerMode => SlumpOnceNeckIsRaised(),
        MotherBrainDrainedPhase.PrepareTransitionToGrey => StartGreying(),
        MotherBrainDrainedPhase.TransitionToGrey => GreyOut(requests),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainDrainedPhase)} {Encounter.DrainedPhase}."),
    };
}
