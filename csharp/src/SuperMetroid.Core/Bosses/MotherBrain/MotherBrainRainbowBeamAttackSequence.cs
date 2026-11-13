using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's repeatable phase-two rainbow-beam, finish-off, and Baby-drain/corpse
/// function chains.
/// </summary>
/// <remarks>
/// The earlier general attack-selection logic and the spawned Baby Metroid's independent AI
/// remain separate actor phases. This class owns the neck extension, both charge waits, body
/// walks and posture changes, active beam, low-energy attack selection, final charge, the four
/// updates readying the Baby's arrival, spawn request, final-beam hold, painful stagger, live neck
/// angles/brain coordinates, rear-room retreat, crouch, grey transition, and corpse handshake.
/// Palette, beam-effect, projectile, earthquake, graphics, spawn, and sound writes are retained as
/// inspectable requests; coordinate, resource, timer, animation, and Samus-command
/// mutations execute directly.
/// </remarks>
public sealed class MotherBrainRainbowBeamAttackSequence
{
    /// <summary>
    /// Signed beam-explosion positions. The index increments before
    /// lookup, so its initial zero value selects the second record.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] ExplosionOffsets =
        [new(-8, -7), new(6, 2), new(-4, 5), new(2, -4), new(3, 6), new(-6, -2), new(8, -6), new(0, 7)];

    /// <summary>Owns the forced Samus movement during beam capture and the later drain sequence.</summary>
    private readonly MotherBrainRainbowBeamSamusMovement _movement = new();
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head animations and the brain's flags, with the walking and posture requests they share.</summary>
    internal MotherBrainLateFightState State { get; }

    /// <summary>Mother Brain's rainbow-beam attack: charging, firing, pinning and draining Samus, letting her fall and deciding what comes next.</summary>
    internal MotherBrainRainbowAttackPhases RainbowAttack { get; }

    /// <summary>Mother Brain finishing Samus off: standing up, admiring, and charging and firing the final beam until the Baby Metroid arrives.</summary>
    internal MotherBrainFinishSamusOffPhases FinishSamusOff { get; }

    /// <summary>Mother Brain drained by the Baby Metroid: taken aback, firing back, running out, retreating and turning gray.</summary>
    internal MotherBrainDrainedByBabyPhases DrainedByBaby { get; }

    /// <summary>Mother Brain reviving and killing the Baby Metroid: signs of life, waking, walking up and the attacks on the baby.</summary>
    internal MotherBrainRevivalPhases Revival { get; }

    /// <summary>Mother Brain's third phase: recovering, setting up and fighting Samus after the baby cutscene.</summary>
    internal MotherBrainThirdPhaseFight ThirdPhaseFight { get; }

    /// <summary>Mother Brain's final death and the escape it starts: explosions, the fades, the corpse, the escape text, timer and door.</summary>
    internal MotherBrainThirdPhaseDeath ThirdPhaseDeath { get; }


    /// <summary>Builds the sequence's phases over one shared late-fight state.</summary>
    public MotherBrainRainbowBeamAttackSequence()
    {
        State = new MotherBrainLateFightState();
        RainbowAttack = new MotherBrainRainbowAttackPhases(_movement, State);
        FinishSamusOff = new MotherBrainFinishSamusOffPhases(State);
        DrainedByBaby = new MotherBrainDrainedByBabyPhases(State);
        Revival = new MotherBrainRevivalPhases(State);
        ThirdPhaseFight = new MotherBrainThirdPhaseFight(State);
        ThirdPhaseDeath = new MotherBrainThirdPhaseDeath(State);
    }

    /// <summary>
    /// Current angle, in 256ths of a turn, of the rainbow beam's per-line effect.
    /// Exposing the movement owner's angle lets the live room adapter publish the
    /// real beam geometry without maintaining a second aim calculation.
    /// </summary>
    public RotationAngle RainbowBeamAngle => _movement.RainbowBeamAngle;

    /// <summary>Reads and writes <paramref name="walk"/> from now on, the walk credit of the encounter attaching this sequence.</summary>
    internal void ShareWalkCounter(MotherBrainWalkCounter walk) => ThirdPhaseFight.ShareWalkCounter(Ensure.NotNull(walk));

    /// <summary>
    /// Inherits the death-and-escape explosion index from the room's encounter state when the live sequence
    /// attaches; the fake-death ascent dust last wrote it.
    /// </summary>
    internal void InheritDeathAndEscapeExplosionIndex(int bodySubPhaseTimer) =>
        ThirdPhaseDeath.DeathAndEscapeExplosionIndex = bodySubPhaseTimer;

    /// <summary>
    /// Starts the repeatable rainbow-beam cycle, before its two charge waits.
    /// </summary>
    public void StartAttackCycle()
    {
        State.BabyArrivalReadyingElapsed = 0;
        State.BabyMetroidSpawned = false;
        RainbowAttack.BeginExtendingNeckForAttack();
    }

    /// <summary>
    /// Imports the physical room actor's state before one live scheduler call. The standalone
    /// verifier may continue to own <see cref="MotherBrainLateFightState.Body"/> directly; gameplay instead lets the
    /// ordinary enemy animation move the real body and feeds the resulting state back here.
    /// This deliberately does not copy animation positions or timers: there must be only
    /// one visible animation runner in a live room.
    /// </summary>
    internal void SynchronizeLiveActor(MotherBrainLiveActorSnapshot actor)
    {
        State.Body.Position = actor.Body;
        State.Body.Pose = actor.BodyPose;
        State.Body.Form = actor.Form;
        ThirdPhaseDeath.BodyContact = actor.BodyContact;
        ThirdPhaseDeath.BodyProcessing = actor.BodyProcessing;
        ThirdPhaseDeath.BodyPresence = actor.BodyPresence;
        ThirdPhaseDeath.BodyCollidesByFrameHitboxes = actor.BodyCollidesByFrameHitboxes;
        ThirdPhaseDeath.BodyShowsNewFrame = actor.BodyShowsNewFrame;
        State.BrainPosition = actor.Brain;
        State.BrainHealth = actor.BrainHealth;
        ThirdPhaseDeath.BrainContact = actor.BrainContact;
        ThirdPhaseDeath.BrainProcessing = actor.BrainProcessing;
        ThirdPhaseDeath.BrainPresence = actor.BrainPresence;
        ThirdPhaseDeath.BrainCollidesByFrameHitboxes = actor.BrainCollidesByFrameHitboxes;
        ThirdPhaseDeath.BrainShowsNewFrame = actor.BrainShowsNewFrame;
        State.NeckMovementEnabled = actor.NeckMovementEnabled;
        State.LowerNeckMovement = actor.LowerNeckMovement;
        State.UpperNeckMovement = actor.UpperNeckMovement;
        ThirdPhaseDeath.HitboxesEnabled = actor.HitboxesEnabled;
    }

    /// <summary>
    /// The animation most recently requested by the state machine's body helper. Gameplay copies
    /// this animation into its physical body slot only when the step result reports a walk or
    /// posture request; it never advances this private mirror's animation timer.
    /// </summary>
    internal MotherBrainBodySegment? RequestedBodyAnimation => State.Body.Animation;

    /// <summary>
    /// Carries out the cross-enemy function write. The Baby actor
    /// installs the taken-aback step after it has reached the brain; Mother Brain executes that new
    /// function on her next enemy-AI turn rather than inside the Baby's current turn.
    /// </summary>
    public void InterruptFinalBeamForBabyDrain()
    {
        State.Phase = MotherBrainBodyPhase.SecondPhaseDrainedByBabyTakenAback;
        State.RainbowBeamSoundPlaying = true; // The Baby's drain writes the shared SFX flag.
    }

    /// <summary>
    /// Runs the Baby's cross-enemy write. It does not
    /// wait for Mother Brain's stand-up animation before repeatedly requesting the backward
    /// walk; the later Baby route decides when to replace this function with the final attack.
    /// </summary>
    public void PrepareForFinalBabyMetroidAttack() =>
        State.Phase = MotherBrainBodyPhase.SecondPhasePrepareForFinalBabyAttack;

    /// <summary>
    /// Runs the Baby's cross-enemy write. Mother Brain installs one last
    /// four-ring head animation and then leaves her body function at the empty holding step.
    /// </summary>
    public void ExecuteFinalBabyMetroidAttack() =>
        State.Phase = MotherBrainBodyPhase.SecondPhaseExecuteFinalBabyAttack;

    /// <summary>
    /// Applies the head's attack-counter step. Live gameplay advances the ordinary
    /// room animation, while the reusable head verifier advances its private animation;
    /// both must mutate this one saturating counter.
    /// </summary>
    internal void IncrementLiveBabyMetroidAttackCounter() =>
        Revival.BabyMetroidAttackCounter = Math.Min(Revival.BabyMetroidAttackCounter + 1, 12);

    /// <summary>Applies the head's attack-counter reset step.</summary>
    internal void ResetLiveBabyMetroidAttackCounter() => Revival.BabyMetroidAttackCounter = 0;

    /// <summary>
    /// Runs the Baby's final cross-enemy write. The body does
    /// not execute the recovery inside the Baby's slot; increasing-slot enemy order makes
    /// this installed phase visible on Mother Brain's following frame.
    /// </summary>
    public void BeginPhase3RecoveryFromBabyCutscene()
    {
        State.Phase = MotherBrainBodyPhase.ThirdPhaseRecoverMakeSomeDistance;

        // Clears only the shared enemy-slot index. This host flag is the equivalent
        // liveness witness used by head attack selection after the actor has deleted itself.
        State.BabyMetroidSpawned = false;
    }

    /// <summary>
    /// Applies the movement/recoil half of Mother Brain's shared phase-two/three shot
    /// reaction. The ordinary enemy-shot reaction owns damage,
    /// projectile deletion, and flash time; this method intentionally does not duplicate
    /// any of those separate systems.
    /// </summary>
    public void ApplyPhase2Or3ShotReaction(MotherBrainProjectileType projectileType)
    {
        // The shot reaction knows only the eight projectile types it reacts to. Validate the
        // host enum so a caller cannot accidentally smuggle a larger
        // value past that domain.
        MotherBrainShotReactionResult reaction =
            MotherBrainShotReaction.Resolve(State.Body.Form, projectileType, ThirdPhaseFight.Phase3WalkCounter);
        ThirdPhaseFight.Phase3WalkCounter = reaction.WalkCredit;
        if (reaction.HyperBeamRecoil)
        {
            // The neck function itself runs on Mother Brain's next ordinary phase-three
            // main call.
            ThirdPhaseFight.Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil;
            State.PhaseTimer = 0;
        }
    }

    /// <summary>
    /// Runs the Baby's call to the standard backwards-walk helper using
    /// animation-delay index two and target <c>Body.X-1</c>.
    /// </summary>
    public bool RequestBabyStumbleBackward()
    {
        int targetX = CoordinateAxis.Wrap(State.Body.Position.X - 1);
        return State.MakeBodyWalkBackwards(targetX, MotherBrainBodySegment.WalkBackwardReallyFast).Requested;
    }

    /// <summary>Executes one call through the current Mother Brain body function.</summary>
    public MotherBrainRainbowBeamAttackStepResult Step(SamusState samus, RainbowBeamAttackFrame frame)
    {
        Ensure.NotNull(samus);
        if (State.Phase is null)
            throw new InvalidOperationException("The active rainbow-beam sequence has not started.");

        MotherBrainBodyPhase? phaseBefore = State.Phase;
        uint headAnimationScriptRequestSerialBefore = State.HeadAnimationRequestSerial;
        var requests = new MotherBrainRainbowBeamStepRequests();

        bool handsOver;
        do
        {
            switch (State.Phase)
            {
                case MotherBrainBodyPhase.SecondPhaseRainbowExtendNeck:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowExtendNeck();
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowStartCharging:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowStartCharging();
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowRetractNeck:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowRetractNeck(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowWaitForCharge:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowWaitForCharge(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowExtendNeckDown:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowExtendNeckDown(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowStartFiring:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowStartFiring(samus, frame, requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowMoveSamusTowardWall:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowMoveSamusTowardWall(samus, frame, requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowOneFrameDelay:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowOneFrameDelay(samus, frame, requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowStartDrainingSamus:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowStartDrainingSamus(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowDrainingSamus:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowDrainingSamus(samus, frame, requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowFinishFiring:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowFinishFiring(samus, frame, requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowLetSamusFall:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowLetSamusFall(samus);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowWaitForSamusToLand:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowWaitForSamusToLand(samus, frame, requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowLowerHead:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowLowerHead();
                    break;
                case MotherBrainBodyPhase.SecondPhaseRainbowDecideNextAction:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowDecideNextAction(samus, requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseFinishSamusOff:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOff(samus, frame, requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseFinishSamusOffStandUp:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffStandUp(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseFinishSamusOffAdmire:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffAdmire();
                    break;
                case MotherBrainBodyPhase.SecondPhaseFinishSamusOffChargeFinalBeam:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffChargeFinalBeam();
                    break;
                case MotherBrainBodyPhase.SecondPhaseFinishSamusOffReadyBaby:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffReadyBaby(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseFinishSamusOffFireFinalBeam:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffFireFinalBeam(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseFinalRainbowBeamHolding:
                    handsOver = MotherBrainFinishSamusOffPhases.RunSecondPhaseFinalRainbowBeamHolding();
                    break;
                case MotherBrainBodyPhase.SecondPhaseDrainedByBabyTakenAback:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyTakenAback();
                    break;
                case MotherBrainBodyPhase.SecondPhaseDrainedByBabyRegainBalance:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyRegainBalance(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseDrainedByBabyFiringRainbowBeam:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyFiringRainbowBeam(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseDrainedByBabyRainbowBeamRunOut:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyRainbowBeamRunOut(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseDrainedByBabyMoveToBackOfRoom:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyMoveToBackOfRoom(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyGoIntoLowPowerMode(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyPrepareTransitionToGrey();
                    break;
                case MotherBrainBodyPhase.SecondPhaseDrainedByBabyTransitionToGrey:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyTransitionToGrey(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseReviveInanimateGrey:
                    handsOver = Revival.RunSecondPhaseReviveInanimateGrey();
                    break;
                case MotherBrainBodyPhase.SecondPhaseReviveShowSignsOfLife:
                    handsOver = Revival.RunSecondPhaseReviveShowSignsOfLife();
                    break;
                case MotherBrainBodyPhase.SecondPhaseReviveTransitionFromGrey:
                    handsOver = Revival.RunSecondPhaseReviveTransitionFromGrey(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseReviveWakeUp:
                    handsOver = Revival.RunSecondPhaseReviveWakeUp(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseReviveWakeUpStretch:
                    handsOver = Revival.RunSecondPhaseReviveWakeUpStretch();
                    break;
                case MotherBrainBodyPhase.SecondPhaseReviveWalkUpToBaby:
                    handsOver = Revival.RunSecondPhaseReviveWalkUpToBaby(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseRevivePrepareNeckForBabyDeath:
                    handsOver = Revival.RunSecondPhaseRevivePrepareNeckForBabyDeath();
                    break;
                case MotherBrainBodyPhase.SecondPhaseReviveFinishPreparingForBabyDeath:
                    handsOver = Revival.RunSecondPhaseReviveFinishPreparingForBabyDeath(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseMurderBabyAttack:
                    handsOver = Revival.RunSecondPhaseMurderBabyAttack(frame, requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseMurderBabyAttackCooldown:
                    handsOver = Revival.RunSecondPhaseMurderBabyAttackCooldown();
                    break;
                case MotherBrainBodyPhase.SecondPhasePrepareForFinalBabyAttack:
                    handsOver = Revival.RunSecondPhasePrepareForFinalBabyAttack(requests);
                    break;
                case MotherBrainBodyPhase.SecondPhaseExecuteFinalBabyAttack:
                    handsOver = Revival.RunSecondPhaseExecuteFinalBabyAttack();
                    break;
                case MotherBrainBodyPhase.SecondPhaseFinalBabyAttackHolding:
                    handsOver = MotherBrainRevivalPhases.RunSecondPhaseFinalBabyAttackHolding();
                    break;
                case MotherBrainBodyPhase.ThirdPhaseRecoverMakeSomeDistance:
                    handsOver = ThirdPhaseFight.RunThirdPhaseRecoverMakeSomeDistance(requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseRecoverSetupForFighting:
                    handsOver = ThirdPhaseFight.RunThirdPhaseRecoverSetupForFighting();
                    break;
                case MotherBrainBodyPhase.ThirdPhaseFightingMain:
                    handsOver = ThirdPhaseFight.RunThirdPhaseFightingMain(frame, requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseFightingAttackCooldown:
                    handsOver = ThirdPhaseFight.RunThirdPhaseFightingAttackCooldown();
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathMoveToBackOfRoom:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathMoveToBackOfRoom(requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathIdleWhilstExploding:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathIdleWhilstExploding(frame, requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathStumbleToMiddleOfRoom:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathStumbleToMiddleOfRoom(frame, requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathDisableBrainEffects:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathDisableBrainEffects(frame, requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathSetupBodyFadeOut:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathSetupBodyFadeOut(frame, requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathFadeOutBody:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathFadeOutBody(frame, requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathFinalFewExplosions:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathFinalFewExplosions(frame, requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathRealizeDecapitation:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathRealizeDecapitation();
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathBrainFallsToGround:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathBrainFallsToGround(requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathReadyCorpse:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathReadyCorpse();
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathSetupFadeToGrey:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathSetupFadeToGrey();
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathFadeToGrey:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathFadeToGrey(requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathCorpseTipsOver:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathCorpseTipsOver();
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathCorpseRotsAway:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathCorpseRotsAway(frame, requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeath20FrameDelay:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeath20FrameDelay();
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathReadyEscapeWarning:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathReadyEscapeWarning();
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathStartEscape:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathStartEscape(frame, requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathSpawnTimeBombSetSubtitle(requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathTypeOutZebesEscapeText:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathTypeOutZebesEscapeText(frame);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathDoorExplodingStartTimer:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathDoorExplodingStartTimer(frame, requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathBlowUpEscapeDoor:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathBlowUpEscapeDoor(requests);
                    break;
                case MotherBrainBodyPhase.ThirdPhaseDeathKeepEarthquakeGoing:
                    handsOver = MotherBrainThirdPhaseDeath.RunThirdPhaseDeathKeepEarthquakeGoing(frame, requests);
                    break;
                // The sequence owns only the rainbow-beam and later body functions; the others belong to the room adapter.
                case MotherBrainBodyPhase.FirstPhase:
                case MotherBrainBodyPhase.FakeDeathDescentInitialPause:
                case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeLock:
                case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeMusic:
                case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeUnlock:
                case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeFlash:
                case MotherBrainBodyPhase.FakeDeathDescentFadeToGray:
                case MotherBrainBodyPhase.FakeDeathDescentCollapseTubes:
                case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows1And2:
                case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows3And4:
                case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows5And6:
                case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows7And8:
                case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows9And10:
                case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows11And12:
                case MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Graphics:
                case MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Brain:
                case MotherBrainBodyPhase.FakeDeathAscentPauseForSuspense:
                case MotherBrainBodyPhase.FakeDeathAscentPrepareForRising:
                case MotherBrainBodyPhase.FakeDeathAscentReadyStandingBody:
                case MotherBrainBodyPhase.FakeDeathAscentContinuePausing:
                case MotherBrainBodyPhase.FakeDeathAscentStartMusicAndEarthquake:
                case MotherBrainBodyPhase.FakeDeathAscentRaiseMotherBrain:
                case MotherBrainBodyPhase.FakeDeathAscentWaitUntilUncrouched:
                case MotherBrainBodyPhase.FakeDeathAscentTransitionFromGray:
                case MotherBrainBodyPhase.SecondPhaseStretchingShakeHead:
                case MotherBrainBodyPhase.SecondPhaseStretchingBringHeadUp:
                case MotherBrainBodyPhase.SecondPhaseStretchingFinish:
                case MotherBrainBodyPhase.SecondPhaseThinking:
                case MotherBrainBodyPhase.SecondPhaseTryAttack:
                case MotherBrainBodyPhase.SecondPhaseBombDecideWalking:
                case MotherBrainBodyPhase.SecondPhaseBombWalkingBackwards:
                case MotherBrainBodyPhase.SecondPhaseBombCrouch:
                case MotherBrainBodyPhase.SecondPhaseBombFired:
                case MotherBrainBodyPhase.SecondPhaseBombStandUp:
                case MotherBrainBodyPhase.SecondPhaseLaserPositionHeadQuickly:
                case MotherBrainBodyPhase.SecondPhaseLaserPositionHeadSlowlyAndFire:
                case MotherBrainBodyPhase.SecondPhaseLaserFinishAttack:
                case MotherBrainBodyPhase.SecondPhaseHandBeam:
                case null:
                default:
                    throw new InvalidOperationException($"Unsupported rainbow-beam phase {State.Phase}.");
            }
        }
        while (handsOver);

        return requests.ToResult(phaseBefore, State.Phase, State.HeadAnimationRequestSerial != headAnimationScriptRequestSerialBefore);
    }
}
